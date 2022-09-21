using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GlobExpressions;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.Twitter;
using Octokit;
using Serilog;
using Services;

public partial class CICD // Common
{
    private Target RestoreSolution => _ => _
        .After(BuildStatusCheck, UnitTestStatusCheck)
        .Executes(() =>
        {
            DotNetTasks.DotNetRestore(s => DotNetRestoreSettingsExtensions.SetProjectFile<DotNetRestoreSettings>(s, this.Solution));
        });

    private void CreateNugetPackage()
    {
        DeleteAllNugetPackages();

        DotNetTasks.DotNetPack(s => DotNetPackSettingsExtensions.SetConfiguration<DotNetPackSettings>(s, Configuration)
            .SetProject(MainProjPath)
            .SetOutputDirectory(NugetOutputPath)
            .EnableNoRestore());
    }

    private void PublishNugetPackage()
    {
        var packages = Glob.Files(NugetOutputPath, "*.nupkg").ToArray();

        if (packages.Length <= 0)
        {
            Assert.Fail($"Could not find a nuget package in path '{NugetOutputPath}' to publish to nuget.org");
        }

        var fullPackagePath = $"{NugetOutputPath}/{packages[0]}";

        if (File.Exists(fullPackagePath))
        {
            DotNetTasks.DotNetNuGetPush(s => DotNetNuGetPushSettingsExtensions.SetTargetPath<DotNetNuGetPushSettings>(s, fullPackagePath)
                .SetSource(NugetOrgSource)
                .SetApiKey(NugetOrgApiKey));
        }
        else
        {
            throw new FileNotFoundException("The nuget package could not be found.", fullPackagePath);
        }
    }

    private void SendReleaseTweet(string templateFilePath, string releaseVersion)
    {
        const string leftBracket = "{";
        const string rightBracket = "}";
        const string projLocation = $"{leftBracket}PROJECT_NAME{rightBracket}";
        const string repoOwner = $"{leftBracket}REPO_OWNER{rightBracket}";
        const string version = $"{leftBracket}VERSION{rightBracket}";

        if (File.Exists(templateFilePath) is false)
        {
            return;
        }

        var tweetTemplate = File.ReadAllText(templateFilePath);

        tweetTemplate = tweetTemplate.Replace(projLocation, MainProjName);
        tweetTemplate = tweetTemplate.Replace(repoOwner, "KinsonDigital");
        tweetTemplate = tweetTemplate.Replace(version, releaseVersion);

        TwitterTasks.SendTweet(tweetTemplate, TwitterConsumerApiKey, TwitterConsumerApiSecret, TwitterAccessToken, TwitterAccessTokenSecret);
    }

    private bool IsPullRequest() => GitHubActions?.IsPullRequest ?? false;

    private bool ReleaseNotesExist(ReleaseType releaseType, string version)
    {
        var releaseNotesDirPath = releaseType switch
        {
            ReleaseType.Production => ProductionReleaseNotesDirPath,
            ReleaseType.Preview => PreviewReleaseNotesDirPath,
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
        };

        nameof(ReleaseNotesExist)
            .LogRequirementTitle($"Checking if the '{releaseType}' release notes exist.");

        return (from f in Glob.Files(releaseNotesDirPath, "*.md")
            where f.Contains(version)
            select f).Any();
    }

    private bool ReleaseNotesDoNotExist(ReleaseType releaseType, string version)
        => !ReleaseNotesExist(releaseType, version);

    private void PrintPullRequestInfo()
    {
        // If the build is on the server and the GitHubActions object exists
        if (IsServerBuild)
        {
            Log.Information("Is Server Build: {Value}", IsServerBuild);
            Log.Information("Repository Owner: {Value}", GitHubActions?.RepositoryOwner);
            Log.Information("Status Check Invoked By: {Value}", GitHubActions?.Actor);
            Log.Information("Is Local Build: {Value}", IsLocalBuild);
            Log.Information("Is PR: {Value}", IsPullRequest());
            Log.Information("Ref: {Value}", GitHubActions?.Ref);
            Log.Information("Source Branch: {Value}", GitHubActions?.HeadRef);
            Log.Information("Destination Branch: {Value}", GitHubActions?.BaseRef);
        }
        else
        {
            Log.Information("Local Build");
        }
    }

    private void DeleteAllNugetPackages()
    {
        // If the build is local, find and delete the package first if it exists.
        // This is to essentially overwrite the package so it is "updated".
        // Without doing this, the package already exists but does not get overwritten to be "updated"
        if (IsLocalBuild)
        {
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
    }

    private async Task<string> GetProdMilestoneDescription(string version)
    {
        version = version.StartsWith('v')
            ? version
            : $"v{version}";

        var detailsStartTag = $"<details closed><summary>Preview Releases</summary>{Environment.NewLine}";
        const string tableHeader = "|Preview Release|Total Issues|";
        const string alignmentRow = "|:----|:----:|";
        const string detailsEndTag = "</details>";

        var issueClient = GitHubClient.Issue;
        var request = new MilestoneRequest { State = ItemStateFilter.All };
        var previewMilestones = (await issueClient.Milestone.GetAllForRepository(Owner, MainProjName, request))
            .Where(m => m.Title.IsPreviewVersion() && m.Title.StartsWith(version)).ToArray();

        var result = $"Container for holding everything released in version {version}";

        if (previewMilestones.Length > 0)
        {
            var tableDataRows = previewMilestones.Select(m =>
            {
                var totalMilestoneIssues = issueClient.IssuesForMilestone(Owner, MainProjName, m.Title).Result.Length;

                return $"{Environment.NewLine}|[🚀{m.Title}]({m.HtmlUrl})|{totalMilestoneIssues}|";
            });

            result += $"{Environment.NewLine}{detailsStartTag}";
            result += $"{Environment.NewLine}{tableHeader}";
            result += $"{Environment.NewLine}{alignmentRow}";

            foreach (var dataRow in tableDataRows)
            {
                result += dataRow;
            }
        }

        result += $"{Environment.NewLine}{detailsEndTag}";

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

        var releaseNotesFilePath = this.Solution.BuildReleaseNotesFilePath(releaseType, version);
        var releaseNotes = this.Solution.GetReleaseNotes(releaseType, version);

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
            TargetCommitish = this.Repo.Commit,
        };

        var releaseClient = GitHubClient.Repository.Release;

        var releaseResult = await releaseClient.Create(Owner, MainProjName, newRelease);
        await releaseClient.UploadTextFileAsset(releaseResult, releaseNotesFilePath);

        return releaseResult.HtmlUrl;
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

        var separator = branchType switch
        {
            BranchType.Feature => "feature/",
            BranchType.PreviewFeature => "preview/feature/",
            BranchType.HotFix => "hotfix/",
            _ => throw new ArgumentOutOfRangeException(nameof(branchType), "Not a valid issue type branch")
        };

        var sections = branch.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        var issueNumStr = sections[0].Split('-')[0];

        var parseResult = int.TryParse(issueNumStr, out var issueNum);

        if (parseResult)
        {
            return issueNum;
        }

        return -1;
    }

    private bool NugetPackageDoesNotExist()
    {
        var project = this.Solution.GetProject(MainProjName);
        var errors = new List<string>();

        nameof(NugetPackageDoesNotExist)
            .LogRequirementTitle($"Checking that the nuget package does not already exist.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{MainProjName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;

        // TODO: This package name might be the owner.reponame.  It could be something different entirely
        var packageName = MainProjName;
        var nugetService = new NugetDataService();

        var packageVersions = nugetService.GetNugetVersions(packageName).Result;

        var nugetPackageExists = packageVersions.Any(i => i == projectVersion);

        if (nugetPackageExists)
        {
            errors.Add($"The nuget package '{packageName}' version 'v{projectVersion}' already exists.");
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    private string GetBranchSyntax(BranchType branchType)
        => branchType switch
        {
            BranchType.Master => "master",
            BranchType.Develop => "develop",
            BranchType.Feature => "feature/#-*",
            BranchType.PreviewFeature => "preview/feature/#-*",
            BranchType.Release => "release/v#.#.#",
            BranchType.Preview => "preview/v#.#.#-preview.#",
            BranchType.HotFix => "hotfix/#-*",
            BranchType.Other => "*",
            _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, null)
        };

    private async Task<string> MergeBranch(string sourceBranch, string targetBranch)
    {
        var mergeClient = GitHubClient.Repository.Merging;

        var newMerge = new NewMerge(targetBranch, sourceBranch)
        {
            CommitMessage = $"Merge the branch '{sourceBranch}' into the branch '{targetBranch}' for production release.",
        };

        var mergeResult = await mergeClient.Create(Owner, MainProjName, newMerge);

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

        var prodContainsPreviewReleases = (await milestoneClient.GetAllForRepository(Owner, MainProjName, milestoneRequest))
            .Any(m => m.Title.IsPreviewVersion() && m.Title.StartsWith(prodVersion));

        return prodContainsPreviewReleases;
    }
}
