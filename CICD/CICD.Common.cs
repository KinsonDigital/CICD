// <copyright file="CICD.Common.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CICDSystem.Services.Interfaces;
using GlobExpressions;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Octokit;
using Serilog;

namespace CICDSystem;

/// <summary>
/// Contains all of the common functionality.
/// </summary>
public partial class CICD // Common
{
    private Target RestoreSolution => _ => _
        .After(PRBuildStatusCheck, PRUnitTestStatusCheck)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => s.SetProjectFile<DotNetRestoreSettings>(Solution));
        });

    /// <summary>
    /// Gets a target that generates workflow templates at the location determined by the <see cref="WorkflowTemplateOutput"/> build parameter.
    /// </summary>
    /// <returns>This is a directory path only.</returns>
    [SuppressMessage("ReSharper", "UnusedMember.Local", Justification = "Used in the CLI as a build parameter.")]
    private Target GenerateWorkflows => _ => _
        .Requires(() => ThatWorkflowOutputDirPathIsValid())
        .Executes(() =>
        {
            // If a file name exists at the end of the path, remove it.  Also replace back slashes with forwards slashes.
            var outputDirPath = Path.HasExtension(WorkflowTemplateOutput)
                ? Path.GetDirectoryName(WorkflowTemplateOutput)?.Replace('\\', '/') ?? string.Empty
                : WorkflowTemplateOutput?.Replace('\\', '/') ?? string.Empty;

            var workflowService = App.Container.GetInstance<IWorkflowService>();
            workflowService.GenerateWorkflows(outputDirPath);
        });

    /// <summary>
    /// Gets a target that is used to get the version number and print it to the console.
    /// </summary>
    // ReSharper disable UnusedMember.Local
    private Target Version => _ => _
        .Executes(() =>
        {
            var version = Assembly.GetEntryAssembly()?
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? string.Empty;

            version = string.IsNullOrEmpty(version) ? string.Empty : $"v{version}";

            if (version == "v1.0.0")
            {
                Log.Warning("The version is unknown or not set.");
            }
            else if (version == string.Empty)
            {
                Log.Error("There was a problem getting the version number.");
            }
            else
            {
                Log.Information("KinsonDigital.CICD Version: {Value}", version);
            }
        });

    // ReSharper restore UnusedMember.Local

    /// <summary>
    /// Creates a NuGet package.
    /// </summary>
    /// <exception cref="Exception">Thrown if the project is null.</exception>
    private void CreateNugetPackage()
    {
        DeleteAllNugetPackages();

        var project = SolutionService.GetProjects(ProjectName).FirstOrDefault();

        if (project is null)
        {
            var exMsg = $"The project named '{ProjectName}' could not be found.";
            exMsg += $"{Environment.NewLine}Check that the 'ProjectName' param in the parameters.json is set correctly.";
            throw new Exception(exMsg);
        }

        DotNetTasks.DotNetPack(s => s.SetConfiguration<DotNetPackSettings>(Configuration)
            .SetProject(project.Path)
            .SetOutputDirectory(NugetOutputPath)
            .EnableNoRestore());
    }

    private void PublishNugetPackage()
    {
        var packages = Glob.Files(NugetOutputPath, "*.nupkg").ToArray();

        if (packages.Length <= 0)
        {
            Assert.Fail($"Could not find a NuGet package in path '{NugetOutputPath}' to publish to nuget.org");
        }

        var fullPackagePath = $"{NugetOutputPath}/{packages[0]}";

        if (File.Exists(fullPackagePath))
        {
            DotNetTasks.DotNetNuGetPush(s =>
                s.SetTargetPath<DotNetNuGetPushSettings>(fullPackagePath)
                .SetSource(NugetOrgSource)
                .SetApiKey(NugetOrgApiKey));
        }
        else
        {
            throw new FileNotFoundException("The NuGet package could not be found.", fullPackagePath);
        }
    }

    private bool ReleaseNotesExist(ReleaseType releaseType, string version)
    {
        var releaseNotesDirPath = releaseType switch
        {
            ReleaseType.Production => ProductionReleaseNotesDirPath,
            ReleaseType.Preview => PreviewReleaseNotesDirPath,
            ReleaseType.HotFix => throw new NotImplementedException("Hot Fix release type not implemented."),
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
        };

        return (from f in Glob.Files(releaseNotesDirPath, "*.md")
            where f.Contains(version)
            select f).Any();
    }

    private bool ReleaseNotesDoNotExist(ReleaseType releaseType, string version)
        => !ReleaseNotesExist(releaseType, version);

    private void DeleteAllNugetPackages()
    {
        // If this is a local build, find and delete all NuGet packages first.
        // This overwrites the package so it is "updated".
        // Without doing this, the package already exists but does not get overwritten to be "updated"
        if (!ExecutionContext.IsLocalBuild)
        {
            return;
        }

        var packages = Glob.Files(NugetOutputPath, "*.nupkg").ToArray();

        foreach (var package in packages)
        {
            var filePath = $"{NugetOutputPath}/{package}";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }

    /// <summary>
    /// Creates a custom milestone description with the given <paramref name="title"/>.
    /// </summary>
    /// <param name="title">The title of the milestone.</param>
    /// <param name="releaseType">The type of release that the milestone represents.</param>
    /// <returns>The custom description.</returns>
    /// <remarks>The title of a milestone is the same as the version number.</remarks>
    private async Task<string> CreateMilestoneDescription(string title, ReleaseType releaseType)
    {
        title = title.StartsWith('v')
            ? title
            : $"v{title}";

        var releaseTypeStr = releaseType.ToString();
        var detailsStartTag = $"<details closed><summary>{releaseTypeStr} Release</summary>{Environment.NewLine}";
        const string detailsEndTag = "</details>";

        var issueClient = GitHubClient.Issue;
        var issues = await issueClient.IssuesForMilestone(RepoOwner, RepoName, title);
        var pullRequests = await GitHubClient.PullRequest.PullRequestsForMilestone(RepoOwner, RepoName, title);
        var result = $"Container for holding everything released in version {title}";

        var mostTimeSpent = issues.MostTimeSpentOnIssue();
        var totalTimeSpent = issues.TotalTimeToComplete();

        var totalItems = $"Total Items: {issues.Length + pullRequests.Length}";
        var totalIssues = $"Total Issues: {issues.Length}";
        var totalPullRequests = $"Total Pull Requests: {pullRequests.Length}";
        var mostTimeSpendOnSingleIssue = $"Most Time Spend On Single Issue: {mostTimeSpent}";
        var totalTimeSpentToCompleteRelease = $"Total Time To Complete Release: {totalTimeSpent}";
        var typeOfWorkLabels = issues.GetDistinctLabelNames().ToArray();
        var assigneeData = issues.GetDistinctAssigneeLoginAndUrl().ToArray();

        var labelStrBuilder = new StringBuilder();

        // Build up the string of label names surrounded by back ticks
        foreach (var label in typeOfWorkLabels)
        {
            labelStrBuilder.Append($"  - `{label}`{Environment.NewLine}");
        }

        var typeOfWorkReleased = $"Type Of Work Released:{Environment.NewLine}{labelStrBuilder.ToString().TrimEnd()}";

        var assigneeDataStrBuilder = new StringBuilder();

        foreach (var data in assigneeData)
        {
            assigneeDataStrBuilder.Append($"  - [{data.login}]({data.url}){Environment.NewLine}");
        }

        var workCompletedBy = $"Work Completed By:{Environment.NewLine}{assigneeDataStrBuilder.ToString().TrimEnd().TrimEnd(',')}";

        result += $"{Environment.NewLine}{detailsStartTag}{Environment.NewLine}";

        result += $"{totalItems}{Environment.NewLine}";
        result += $"{totalIssues}{Environment.NewLine}";
        result += $"{totalPullRequests}{Environment.NewLine}";
        result += $"{mostTimeSpendOnSingleIssue}{Environment.NewLine}";
        result += $"{totalTimeSpentToCompleteRelease}{Environment.NewLine}";
        result += $"{typeOfWorkReleased}{Environment.NewLine}";
        result += $"{Environment.NewLine}{workCompletedBy}{Environment.NewLine}";

        result += detailsEndTag;

        return result;
    }

    private async Task<string> CreateNewGitHubRelease(ReleaseType releaseType, string version)
    {
        if (string.IsNullOrEmpty(version))
        {
            throw new ArgumentException("The version must not be null or empty.", nameof(version));
        }

        version = version.StartsWith("v")
            ? version
            : $"v{version}";

        var validVersionSyntax = releaseType switch
        {
            ReleaseType.Preview => version.IsPreviewVersion(),
            ReleaseType.Production => version.IsProductionVersion(),
            ReleaseType.HotFix => version.IsProductionVersion(),
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
        };

        if (validVersionSyntax is false)
        {
            throw new ArgumentException($"The version does not have the correct syntax for a {releaseType.ToString().ToLower()} release.");
        }

        var releaseNotesFilePath = SolutionService.BuildReleaseNotesFilePath(releaseType, version);
        var releaseNotes = SolutionService.GetReleaseNotes(releaseType, version);

        if (string.IsNullOrEmpty(releaseNotes))
        {
            throw new Exception($"The release notes could not be found at the path '{releaseNotesFilePath}'.");
        }

        var newRelease = new NewRelease(version)
        {
            Name = $"🚀{releaseType} Release - {version}",
            Body = releaseNotes,
            Prerelease = releaseType == ReleaseType.Preview,
            Draft = false,
            TargetCommitish = Repo.Commit,
        };

        var releaseClient = GitHubClient.Repository.Release;

        try
        {
            var releaseResult = await releaseClient.Create(RepoOwner, RepoName, newRelease);
            await releaseClient.UploadTextFileAsset(releaseResult, releaseNotesFilePath);

            return releaseResult.HtmlUrl;
        }
        catch (ApiValidationException e)
        {
            Log.Error(e.ToLogMessage());

            throw;
        }
    }

    private int ExtractIssueNumber(BranchType branchType, string branch)
    {
        var isNotIssueBranch = branchType != BranchType.Feature &&
                               branchType != BranchType.PreviewFeature &&
                               branchType != BranchType.HotFix;

        if (isNotIssueBranch || branch.DoesNotContainNumbers())
        {
            return -1; // All of the other branches do not contain an issue number
        }

        var startSection = branchType switch
        {
            BranchType.Feature => "feature/",
            BranchType.PreviewFeature => "preview/feature/",
            BranchType.HotFix => "hotfix/",
            _ => throw new ArgumentOutOfRangeException(nameof(branchType), "Not a valid issue type branch")
        };

        var valueToSplit = branch.Replace(startSection, string.Empty);
        var issueNumStr = valueToSplit.Split('-')[0];

        var parseResult = int.TryParse(issueNumStr, out var issueNum);

        if (parseResult)
        {
            return issueNum;
        }

        return -1;
    }

    private async Task<string> MergeBranch(string sourceBranch, string targetBranch)
    {
        var mergeClient = GitHubClient.Repository.Merging;

        var newMerge = new NewMerge(targetBranch, sourceBranch)
        {
            CommitMessage = $"Merge the branch '{sourceBranch}' into the branch '{targetBranch}' for production release.",
        };

        var mergeResult = await mergeClient.Create(RepoOwner, RepoName, newMerge);

        return mergeResult?.HtmlUrl ?? string.Empty;
    }

    private async Task<bool> ProdVersionHasPreviewReleases(string prodVersion)
    {
        if (prodVersion.IsProductionVersion() is false)
        {
            throw new Exception($"The version '{prodVersion}' must not be a preview version.");
        }

        var issueClient = GitHubClient.Issue;
        var milestoneClient = issueClient.Milestone;

        var milestoneRequest = new MilestoneRequest
        {
            State = ItemStateFilter.All,
        };

        var prodContainsPreviewReleases = (await milestoneClient.GetAllForRepository(RepoOwner, RepoName, milestoneRequest))
            .Any(m => m.Title.IsPreviewVersion() && m.Title.StartsWith(prodVersion));

        return prodContainsPreviewReleases;
    }
}
