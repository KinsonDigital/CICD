// <copyright file="CICD.Requirements.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using CICDSystem.Services;
using Nuke.Common;
using Octokit;
using Serilog;

namespace CICDSystem;

/// <summary>
/// Contains all of the requirement related methods for Target requires setup.
/// </summary>
public partial class CICD // Requirements
{
    private bool ThatPullRequestNumberIsProvided()
    {
        nameof(ThatPullRequestNumberIsProvided)
            .LogRequirementTitle("Checking that a pull request number has been provided.");

        return PullRequestNumber > 0;
    }

    private bool ThatThePullRequestExists()
    {
        nameof(ThatThePullRequestExists)
            .LogRequirementTitle("Checking if the pull request number exists.");

        var prNumber = PullRequestService.PullRequestNumber;
        var result = GitHubClient.PullRequest.Exists(RepoOwner, RepoName, prNumber).Result;

        if (result)
        {
            Console.WriteLine($"{ConsoleTab}The pull request number '{prNumber}' exists.");
            return true;
        }

        Log.Error($"The pull request number '{prNumber}' does not exist.");
        Assert.Fail("Pull request failure.");
        return false;
    }

    private bool ThatThePRHasBeenAssigned()
    {
        var prClient = GitHubClient.PullRequest;

        nameof(ThatThePRHasBeenAssigned)
            .LogRequirementTitle("Checking if the pull request as been assigned to someone.");

        var prNumber = PullRequestService.PullRequestNumber;

        if (prClient.HasAssignees(RepoOwner, RepoName, prNumber).Result)
        {
            Console.WriteLine($"{ConsoleTab}The pull request '{prNumber}' is properly assigned.");
        }
        else
        {
            var prLink = $"https://github.com/{RepoOwner}/{RepoName}/pull/{prNumber}";
            var errorMsg = "The pull request '{Value1}' is not assigned to anyone.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To set an assignee, go to 👉🏼 '{{Value2}}'.";
            Log.Error(errorMsg, prNumber, prLink);
            Assert.Fail("The pull request is not assigned to anybody.");
        }

        return true;
    }

    /// <summary>
    /// Returns a value indicating whether or not the source branch of a pull request contains an issue number
    /// and that the issue number exists.
    /// </summary>
    /// <returns><c>true</c> if the issue exists.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Occurs if the source branch type is out of range.
    /// </exception>
    private bool ThatThePRSourceBranchIssueNumberExists()
    {
        var sourceBranch = PullRequestService.SourceBranch;

        var branchType = sourceBranch.GetBranchType();
        var branchTypeStr = branchType.ToString().ToSpaceDelimitedSections().ToLower();

        nameof(ThatThePRSourceBranchIssueNumberExists)
            .LogRequirementTitle($"Checking that the issue number in the '{branchTypeStr}' branch exists.");

        var isNotIssueNumberBranch = branchType switch
        {
            BranchType.Master => true,
            BranchType.Develop => true,
            BranchType.Feature => false,
            BranchType.PreviewFeature => false,
            BranchType.Release => true,
            BranchType.Preview => true,
            BranchType.HotFix => false,
            BranchType.Other => true,
            _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, null)
        };

        if (isNotIssueNumberBranch)
        {
            var issueNumBranches = $"'{BranchType.Feature.ToString().ToLower()}', ";
            issueNumBranches += $"'{BranchType.PreviewFeature.ToString().ToSpaceDelimitedSections().ToLower()}', and ";
            issueNumBranches += $"'{BranchType.HotFix.ToString().ToLower()}'.";

            var errorMsg = $"The branch '{branchTypeStr}' must be a branch that contains an issue number.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}Valid issue number branches are {issueNumBranches}";
            Log.Error(errorMsg);
            Assert.Fail("Invalid issue number branch.");
            return false;
        }

        var branchIssueNumber = ExtractIssueNumber(branchType, sourceBranch);
        var issueExists = GitHubClient.Issue.IssueExists(RepoOwner, RepoName, branchIssueNumber).Result;

        if (issueExists is false)
        {
            var errorMsg = $"The issue '{branchIssueNumber}' does not exist for the branch '{sourceBranch}'.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}The source branch '{sourceBranch}' must be recreated with the correct issue number.";

            Log.Error(errorMsg);
            Assert.Fail($"The branch issue number does not exist.");
            return false;
        }

        Console.WriteLine($"{ConsoleTab}The issue number in the branch '{sourceBranch}' is valid.");

        return true;
    }

    private bool ThatFeaturePRIssueHasLabel(BranchType branchType)
    {
        var errors = new List<string>();
        var validBranchTypes = new[]
        {
            BranchType.Feature,
            BranchType.PreviewFeature,
            BranchType.HotFix,
        };
        var branchTypeStr = branchType.ToString().ToSpaceDelimitedSections().ToLower();

        nameof(ThatFeaturePRIssueHasLabel)
            .LogRequirementTitle($"Checking if the issue for the '{branchTypeStr}' branch contains at least one label.");

        // If the branch type is invalid
        if (validBranchTypes.Contains(branchType) is false)
        {
            errors.Add($"The branch type '{branchType}' is not valid for the '{nameof(ThatFeaturePRIssueHasLabel)}' check.");
        }
        else
        {
            var sourceBranch = PullRequestService.SourceBranch;
            var branchIssueNumber = ExtractIssueNumber(branchType, sourceBranch);
            var issueExists = GitHubClient.Issue.IssueExists(RepoOwner, RepoName, branchIssueNumber).Result;

            if (issueExists)
            {
                var containsLabels = GitHubClient.Issue.HasLabels(RepoOwner, RepoName, branchIssueNumber).Result;

                if (containsLabels)
                {
                    Console.WriteLine($"{ConsoleTab}The issue '{branchIssueNumber}' contains at least 1 label.");
                }
                else
                {
                    errors.Add($"The issue '{branchIssueNumber}' does not contain any labels.");
                }
            }
            else
            {
                var errorMsg = $"The issue '{branchIssueNumber}' does not exist for preview feature branch '{sourceBranch}'.";
                errorMsg += $"{Environment.NewLine}{ConsoleTab}The source branch '{sourceBranch}' must be recreated with the correct issue number.";
                errorMsg += $"{Environment.NewLine}{ConsoleTab}The syntax requirements for a preview feature branch is 'preview/feature/#-*'.";
                errors.Add(errorMsg);
            }
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    private bool ThatPRHasLabels()
    {
        var prClient = GitHubClient.PullRequest;

        nameof(ThatPRHasLabels)
            .LogRequirementTitle($"Checking if the pull request has labels.");

        var prNumber = PullRequestService.PullRequestNumber;

        if (prClient.HasLabels(RepoOwner, RepoName, prNumber).Result)
        {
            Console.WriteLine($"{ConsoleTab}The pull request '{prNumber}' has labels.");
        }
        else
        {
            var prLink = $"https://github.com/{RepoOwner}/{RepoName}/pull/{prNumber}";
            var errorMsg = "The pull request '{Value1}' does not have any labels.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To add a label, go to 👉🏼 '{{Value2}}'.";
            Log.Error(errorMsg, prNumber, prLink);
            Assert.Fail("The pull request does not have one or more labels.");
        }

        return true;
    }

    /// <summary>
    /// Returns a value indicating whether or not the pull request contains a label that matches the given <paramref name="labels"/>.
    /// </summary>
    /// <param name="labels">The name of the label.</param>
    /// <returns><c>true</c> if the pull request has the label.</returns>
    private bool ThatThePRHasTheLabel(params string[] labels)
    {
        labels = labels.Select(l => l.Trim().Trim('\'')).ToArray();

        var prNumber = PullRequestService.PullRequestNumber;

        nameof(ThatThePRHasTheLabel)
            .LogRequirementTitle($"Checking if the pull request has the label '{labels}'.");

        if (prNumber <= 0)
        {
            var errorMsg = $"The pull request number '{prNumber}' is invalid.";
            Log.Error(errorMsg);
            Assert.Fail("The pull request number is invalid.");
            return false;
        }

        var prLabels = GitHubClient.PullRequest.Get(RepoOwner, RepoName, prNumber)
            .Result
            .Labels.Select(l => l.Name).ToArray();

        var missingLabels = labels.Where(l => prLabels.All(pl => pl != l)).Select(l => l).ToArray();

        if (missingLabels.Length <= 0)
        {
            Console.WriteLine($"{ConsoleTab}The pull request '{prNumber}' has the correct labels.");
        }
        else
        {
            // Construct a string of comma delimited label names
            var missingLabelsStr = string.Join(string.Empty, missingLabels.Select(l => $"{l}, ").ToArray()).TrimEnd().TrimEnd(',');

            var prLink = $"https://github.com/{RepoOwner}/{RepoName}/pull/{prNumber}";
            var errorMsg = $"The pull request '{{Value1}}' is missing the labels '{missingLabelsStr}'.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To add the label, go to 👉🏼 '{{Value2}}'.";
            Log.Error(errorMsg, prNumber, prLink);
            Assert.Fail("The pull request does not have a preview release label.");
            return false;
        }

        return true;
    }

    private bool ThatTheCurrentBranchIsCorrect(BranchType branchType)
    {
        var branchTypeStr = branchType.ToString().ToLower();

        nameof(ThatTheCurrentBranchIsCorrect)
            .LogRequirementTitle($"Checking that the current branch is a {branchTypeStr} branch.");

        if (string.IsNullOrEmpty(Repo.Branch))
        {
            return false;
        }

        var isCorrectBranch = branchType switch
        {
            BranchType.Master => Repo.Branch.IsMasterBranch(),
            BranchType.Develop => Repo.Branch.IsDevelopBranch(),
            BranchType.Feature => Repo.Branch.IsFeatureBranch(),
            BranchType.PreviewFeature => Repo.Branch.IsPreviewFeatureBranch(),
            BranchType.Release => Repo.Branch.IsReleaseBranch(),
            BranchType.Preview => Repo.Branch.IsPreviewBranch(),
            BranchType.HotFix => Repo.Branch.IsHotFixBranch(),
            BranchType.Other => true,
            _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, null)
        };

        if (isCorrectBranch is false)
        {
            Log.Error($"The current branch {Repo.Branch} is not a '{branchTypeStr}' branch.");
            Assert.Fail("The current branch is incorrect.");
        }

        return true;
    }

    private bool ThatThePRBranchIsValid(PRBranchContext branchContext, params BranchType[] branchTypes)
    {
        var branchContextStr = Enum.GetName(branchContext)?.ToLower() ?? string.Empty;

        string branch;

        try
        {
            branch = branchContext switch
            {
                PRBranchContext.Source => PullRequestService.SourceBranch,
                PRBranchContext.Target => PullRequestService.TargetBranch,
                _ => throw new ArgumentOutOfRangeException(nameof(branchContext), branchContext, null)
            };
        }
        catch (Exception e) when (e is NotFoundException || (e.InnerException is not null && e.InnerException is NotFoundException))
        {
            Log.Error($"The pull request '{PullRequestNumber}' was not found.");
            Assert.Fail("Pull request not found.");
            return false;
        }

        var foundBranchType = branchTypes.FirstOrDefault(t => t switch
        {
            BranchType.Develop => branch.IsDevelopBranch(),
            BranchType.Master => branch.IsMasterBranch(),
            BranchType.Feature => branch.IsFeatureBranch(),
            BranchType.PreviewFeature => branch.IsPreviewFeatureBranch(),
            BranchType.Release => branch.IsReleaseBranch(),
            BranchType.Preview => branch.IsPreviewBranch(),
            BranchType.HotFix => branch.IsHotFixBranch(),
            BranchType.Other => false,
            _ => throw new ArgumentOutOfRangeException(nameof(branchTypes), branchTypes, null)
        });

        var branchTypeStr = Enum.GetName(foundBranchType)?.ToSpaceDelimitedSections().ToLower() ?? string.Empty;

        nameof(ThatThePRBranchIsValid)
            .LogRequirementTitle($"Validating the pull request '{branchContextStr}' branch is a '{branchTypeStr}' branch.");

        var isValid = branchTypes.Any(t =>
        {
            return t switch
            {
                BranchType.Master => BranchValidator.Reset().IsMasterBranch(branch).GetValue(),
                BranchType.Develop => BranchValidator.Reset().IsDevelopBranch(branch).GetValue(),
                BranchType.Feature => BranchValidator.Reset().IsFeatureBranch(branch).GetValue(),
                BranchType.PreviewFeature => BranchValidator.Reset().IsPreviewFeatureBranch(branch).GetValue(),
                BranchType.Release => BranchValidator.Reset().IsReleaseBranch(branch).GetValue(),
                BranchType.Preview => BranchValidator.Reset().IsPreviewBranch(branch).GetValue(),
                BranchType.HotFix => BranchValidator.Reset().IsHotFixBranch(branch).GetValue(),
                BranchType.Other => false,
                _ => throw new ArgumentOutOfRangeException(nameof(branchTypes), branchTypes, null)
            };
        });

        if (isValid)
        {
            Console.WriteLine($"{Environment.NewLine}{ConsoleTab}The '{branchContextStr}' branch '{branch}' is valid.");
        }
        else
        {
            Log.Error($"The pull request '{branchContextStr}' branch '{branch}' is not a valid '{branchTypeStr}' branch.");
            Assert.Fail($"Pull request '{branchContextStr}' branch is not valid.");
        }

        return isValid;
    }

    private bool ThatThePreviewPRBranchVersionsMatch(ReleaseType releaseType)
    {
        var sourceBranch = PullRequestService.SourceBranch;
        var targetBranch = PullRequestService.TargetBranch;
        var errors = new List<string>();
        var releaseTypeStr = releaseType.ToString().ToLower();

        nameof(ThatThePreviewPRBranchVersionsMatch)
            .LogRequirementTitle($"Checking that the version section for the {releaseType} release pull request source and target branches match.");

        if (string.IsNullOrEmpty(sourceBranch) || string.IsNullOrEmpty(targetBranch))
        {
            errors.Add("The workflow must be executed from a pull request in the GitHub environment.");
        }

        var srcBranchSyntax = releaseType switch
        {
            ReleaseType.Preview => "preview/v#.#.#-preview.#",
            ReleaseType.Production => "release/v#.#.#",
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
        };

        var targetBranchSyntax = releaseType switch
        {
            ReleaseType.Preview => "release/v#.#.#",
            ReleaseType.Production => "master OR develop",
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
        };

        var validSrcBranch = releaseType switch
        {
            ReleaseType.Preview => sourceBranch.IsPreviewBranch(),
            ReleaseType.Production => sourceBranch.IsReleaseBranch(),
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
        };

        var validTargetBranch = releaseType switch
        {
            ReleaseType.Preview => targetBranch.IsReleaseBranch(),
            ReleaseType.Production => targetBranch.IsMasterBranch() || targetBranch.IsDevelopBranch(),
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
        };

        if (validSrcBranch is false)
        {
            var errorMsg = $"The pull request source branch '{sourceBranch}' must be a {releaseTypeStr} branch.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}{releaseType} Branch Syntax: '{srcBranchSyntax}'";
            errors.Add(errorMsg);
        }

        if (validTargetBranch is false)
        {
            var errorMsg = $"The pull request target branch '{targetBranch}' must be a release branch.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}Release Branch Syntax: '{targetBranchSyntax}'";
            errors.Add(errorMsg);
        }

        var bothBranchesAreVersionBranches = sourceBranch.IsPreviewBranch() && targetBranch.IsReleaseBranch();
        var srcBranchVersion = bothBranchesAreVersionBranches
            ? sourceBranch.ExtractBranchVersion().version.Split('-')[0]
            : string.Empty;
        var targetBranchVersion = bothBranchesAreVersionBranches
            ? targetBranch.ExtractBranchVersion().version
            : string.Empty;

        if (srcBranchVersion != targetBranchVersion)
        {
            var errorMsg = $"The main version sections of the source branch '{sourceBranch}' and the target branch '{targetBranch}' do not match.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}Source Branch Syntax: 'preview/v#.#.#-preview.#'";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}Target Branch Syntax: 'release/v#.#.#'";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    private bool ThatTheCurrentBranchVersionMatchesProjectVersion(BranchType branchType)
    {
        var targetBranch = Repo.Branch ?? string.Empty;
        var project = SolutionService.GetProject(RepoName);

        var errors = new List<string>();
        var branchTypeStr = branchType.ToString().ToSpaceDelimitedSections().ToLower();

        nameof(ThatTheCurrentBranchVersionMatchesProjectVersion)
            .LogRequirementTitle(branchType is BranchType.Preview or BranchType.Release
                ? $"Checking that the version section of the {branchTypeStr} branch matches the project version."
                : "No check required.  The target branch is not a branch that contains a version.");

        if (string.IsNullOrEmpty(targetBranch))
        {
            errors.Add("The branch not be null or empty.");
        }

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var branchVersion = Repo.Branch?.ExtractBranchVersion().version.TrimStart('v');
        var projectVersion = string.IsNullOrEmpty(branchVersion)
            ? string.Empty
            : project?.GetVersion() ?? string.Empty;

        projectVersion = projectVersion.IsPreviewVersion()
            ? projectVersion.Split('-')[0]
            : projectVersion;

        if (projectVersion != branchVersion)
        {
            var errorMsg = "The major, minor, patch section of the project and branch versions do not match.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}Project Version Section: {projectVersion}";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}Branch Version Section: {branchVersion}";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    private bool ThatTheProjectVersionsAreValid(ReleaseType releaseType)
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatTheProjectVersionsAreValid)
            .LogRequirementTitle($"Checking that all of the versions in the csproj file are valid.");

        if (project is null)
        {
            Log.Error($"Could not find the project '{RepoName}'");
            Assert.Fail("There was an issue getting the project.");
            return false;
        }

        var versionExists = project.VersionExists();
        var fileVersionExists = project.FileVersionExists();
        var assemblyVersionExists = project.AssemblyVersionExists();

        // Check if the regular version value exists
        if (versionExists is false)
        {
            errors.Add("The version '<Version/>' value in the csproj file does not exist.");
        }

        // Check if the file version value exists
        if (fileVersionExists is false)
        {
            errors.Add("The version '<FileVersion/>' value in the csproj file does not exist.");
        }

        // Check if the assembly version value exists
        if (assemblyVersionExists is false)
        {
            errors.Add("The version '<AssemblyVersion/>' value in the csproj file does not exist.");
        }

        const string previewBranchSyntax = "#.#.#-preview.#";
        const string productionBranchSyntax = "#.#.#";

        var statusMsg = string.Empty;

        if (versionExists)
        {
            var validVersionSyntax = releaseType switch
            {
                ReleaseType.Preview => project.HasCorrectVersionSyntax(previewBranchSyntax),
                ReleaseType.Production => project.HasCorrectVersionSyntax(productionBranchSyntax),
                _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
            };

            if (validVersionSyntax)
            {
                statusMsg += $"{ConsoleTab}The version '{project.GetVersion()}' is valid.{Environment.NewLine}";
            }
            else
            {
                var msg = "The syntax for the '<Version/>' value in the csproj file is invalid.";
                msg += $"{Environment.NewLine}{ConsoleTab}Valid syntax is '{previewBranchSyntax}'";
                errors.Add(msg);
            }
        }

        if (fileVersionExists)
        {
            var validFileVersionSyntax = releaseType switch
            {
                ReleaseType.Preview => project.HasCorrectFileVersionSyntax(previewBranchSyntax),
                ReleaseType.Production => project.HasCorrectFileVersionSyntax(productionBranchSyntax),
                _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
            };

            if (validFileVersionSyntax)
            {
                statusMsg += $"{ConsoleTab}The file version '{project.GetVersion()}' is valid.{Environment.NewLine}";
            }
            else
            {
                var msg = "The syntax for the '<FileVersion/>' value in the csproj file is invalid.";
                msg += $"{Environment.NewLine}{ConsoleTab}Valid syntax is '{previewBranchSyntax}'";
                errors.Add(msg);
            }
        }

        if (assemblyVersionExists)
        {
            var validAssemblyVersionSyntax = releaseType switch
            {
                ReleaseType.Preview => project.HasCorrectAssemblyVersionSyntax(productionBranchSyntax),
                ReleaseType.Production => project.HasCorrectAssemblyVersionSyntax(productionBranchSyntax),
                _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
            };

            if (validAssemblyVersionSyntax)
            {
                statusMsg += $"{ConsoleTab}The assembly version '{project.GetAssemblyVersion()}' is valid.{Environment.NewLine}";
            }
            else
            {
                var msg = "The syntax for the '<AssemblyVersion/>' value in the csproj file is invalid.";
                msg += $"{Environment.NewLine}{ConsoleTab}Valid syntax is '{productionBranchSyntax}'";
                errors.Add(msg);
            }
        }

        if (errors.Count <= 0)
        {
            Console.WriteLine($"{statusMsg}{Environment.NewLine}");
            return true;
        }

        errors.PrintErrors();

        return errors.Count <= 0;
    }

    private bool ThatThePRSourceBranchVersionSectionMatchesProjectVersion(ReleaseType releaseType)
    {
        var sourceBranch = PullRequestService.SourceBranch;
        var errors = new List<string>();

        var introMsg = "Checking that the project version matches the version section";
        introMsg += $" of the pull request source {releaseType.ToString().ToLower()} branch.";
        introMsg += $"{Environment.NewLine}{ConsoleTab}This validation is only checked for preview and release source branches.";

        nameof(ThatThePRSourceBranchVersionSectionMatchesProjectVersion)
            .LogRequirementTitle(introMsg);

        if (string.IsNullOrEmpty(sourceBranch))
        {
            errors.Add("The workflow must be executed from a pull request in the GitHub environment.");
        }

        var branchType = sourceBranch.GetBranchType();

        if (branchType is BranchType.Preview or BranchType.Release)
        {
            var project = SolutionService.GetProject(RepoName);

            if (project is null)
            {
                errors.Add($"Could not find the project '{RepoName}'");
            }

            var setProjectVersion = project?.GetVersion() ?? string.Empty;
            var branchVersionSection = sourceBranch.ExtractBranchVersion().version.TrimStart('v');

            if (setProjectVersion != branchVersionSection)
            {
                var errorMsg = $"The set project version '{setProjectVersion}' does not match the version";
                errorMsg += " branch section '{branchVersionSection}' of the source branch.";
                errors.Add(errorMsg);
            }
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    private bool ThatTheReleaseMilestoneExists()
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatTheReleaseMilestoneExists)
            .LogRequirementTitle($"Checking that the release milestone exists for the current version.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;
        var milestoneClient = GitHubClient.Issue.Milestone;

        var milestoneExists = milestoneClient.MilestoneExists(RepoOwner, RepoName, $"v{projectVersion}").Result;

        if (milestoneExists is false)
        {
            var milestoneUrl = $"https://github.com/{RepoOwner}/{RepoName}/milestones/new";
            var errorMsg = $"The milestone for version '{projectVersion}' does not exist.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To create a milestone, go here 👉🏼 {milestoneUrl}";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    /// <summary>
    /// Returns a value indicating whether or not the milestone contains only a single item
    /// that matches the given <paramref name="itemType"/>.
    /// </summary>
    /// <param name="itemType">The type of item to check.</param>
    /// <returns><c>true</c> if there is only a single item.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     Thrown if the <paramref name="itemType"/> is out of range.
    /// </exception>
    private bool ThatTheMilestoneContainsOnlySingleItemOfType(ItemType itemType)
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        var itemTypeStr = itemType.ToString().ToSpaceDelimitedSections().ToLower();

        nameof(ThatTheMilestoneContainsOnlySingleItemOfType)
            .LogRequirementTitle($"Checking that the release milestone for the current version contains only a single {itemTypeStr}.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;
        var milestoneClient = GitHubClient.Issue.Milestone;
        var milestoneTitle = $"v{projectVersion}";

        var milestone = milestoneClient.GetByTitle(RepoOwner, RepoName, milestoneTitle).Result;

        if (milestone is null)
        {
            errors.Add($"Could not find a milestone with the title '{milestoneTitle}'");
        }

        var repoIssueRequest = new RepositoryIssueRequest()
        {
            State = ItemStateFilter.All,
            Milestone = milestone?.Number.ToString() ?? string.Empty,
        };

        var milestoneItems = GitHubClient.Issue.GetAllForRepository(RepoOwner, RepoName, repoIssueRequest).Result;

        var totalIssues = milestoneItems.Count(i => itemType switch
        {
            ItemType.Issue => i.IsIssue(),
            ItemType.PullRequest => i.IsPullRequest(),
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, $"{nameof(ItemType)} is out of range.")
        });

        if (totalIssues != 1)
        {
            var errorMsg = $"The milestone '{milestoneTitle}' can only contain a single hot fix {itemTypeStr}.";
            errors.Add(errorMsg);

            return false;
        }

        if (errors.Count > 0)
        {
            errors.PrintErrors($"Hot fix release milestone does not have only a single {itemTypeStr}.");

            return false;
        }

        Console.WriteLine($"{ConsoleTab}The milestone '{milestoneTitle}' is valid with only a single {itemTypeStr}.");

        return true;
    }

    private bool ThatTheReleaseMilestoneContainsIssues()
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatTheReleaseMilestoneContainsIssues)
            .LogRequirementTitle("Checking that the release milestone for the current version contains issues.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;
        var milestoneClient = GitHubClient.Issue.Milestone;

        var milestone = milestoneClient.GetByTitle(RepoOwner, RepoName, $"v{projectVersion}").Result;

        if (milestone is null)
        {
            var milestoneUrl = $"https://github.com/{RepoOwner}/{RepoName}/milestones/new";
            var errorMsg = $"The milestone for version '{projectVersion}' does not exist.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To create a milestone, go here 👉🏼 {milestoneUrl}";
            errors.Add(errorMsg);
        }

        var totalMilestoneIssues = (milestone?.OpenIssues ?? 0) + (milestone?.ClosedIssues ?? 0);

        if (totalMilestoneIssues == 0)
        {
            var errorMsg = $"The milestone for version '{projectVersion}' does not contain any issues or pull requests.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}Add some issues to the milestone";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    /// <summary>
    /// Returns a value indicating whether or not a milestone only contains a single release item
    /// of the given <paramref name="releaseType"/> and <paramref name="itemType"/>.
    /// </summary>
    /// <param name="releaseType">The release type of the item.</param>
    /// <param name="itemType">The type of item.</param>
    /// <returns><c>true</c> if only a single item exists in the milestone.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown if the <paramref name="releaseType"/> or <paramref name="itemType"/> is out of range.
    /// </exception>
    private bool ThatTheReleaseMilestoneOnlyContainsSingleReleaseItem(ReleaseType releaseType, ItemType itemType)
    {
        const int totalSpaces = 15;
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();
        var releaseTypeStr = releaseType.ToString().ToLower();

        var introMsg = "Checking that the release milestone only contains a single release ";
        introMsg += $"{(itemType == ItemType.Issue ? "todo issue" : "pull request")} item.";
        nameof(ThatTheReleaseMilestoneOnlyContainsSingleReleaseItem)
            .LogRequirementTitle(introMsg);

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;
        var mileStoneTitle = $"v{projectVersion}";
        var issueClient = GitHubClient.Issue;
        var mileStoneClient = GitHubClient.Issue.Milestone;
        var milestone = mileStoneClient.GetByTitle(RepoOwner, RepoName, mileStoneTitle).Result;

        if (milestone is null)
        {
            var milestoneUrl = $"https://github.com/{RepoOwner}/{RepoName}/milestones/new";
            var errorMsg = "Cannot check a milestone that does not exist.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To create a milestone, go here 👉🏼 {milestoneUrl}";
            errors.Add(errorMsg);
        }

        var milestoneItems = itemType switch
        {
            ItemType.Issue => issueClient.IssuesForMilestone(RepoOwner, RepoName, mileStoneTitle).Result,
            ItemType.PullRequest => issueClient.PullRequestsForMilestone(RepoOwner, RepoName, mileStoneTitle).Result,
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
        };

        if (milestoneItems.Length <= 0)
        {
            var errorMsg = $"The milestone does not contain any {(itemType == ItemType.Issue ? "todo issues." : "pull requests.")}.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To view the milestone, go here 👉🏼 {milestone?.HtmlUrl}";
            errors.Add(errorMsg);
        }

        var itemTitleAndLabel = releaseType switch
        {
            ReleaseType.Preview => "🚀Preview Release",
            ReleaseType.Production => "🚀Production Release",
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
        };

        var allReleaseItems = milestoneItems.Where(i =>
        {
            return itemType switch
            {
                ItemType.Issue => i.IsReleaseToDoIssue(releaseType),
                ItemType.PullRequest => i.IsReleasePullRequest(releaseType),
                _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
            };
        }).ToArray();
        var indent = Environment.NewLine + totalSpaces.CreateDuplicateCharacters(' ');

        if (allReleaseItems.Length != 1)
        {
            var itemTypeStr = itemType switch
            {
                ItemType.Issue => "to do issue",
                ItemType.PullRequest => "pull request",
                _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
            };

            var errorMsg =
                $"The {releaseTypeStr} release milestone '{mileStoneTitle}' has '{allReleaseItems.Length}' release {itemTypeStr}s.";
            errorMsg += $"{indent}Release milestones should only have a single release {itemTypeStr}.";
            errorMsg += $"{indent}Release {itemTypeStr.CapitalizeWords()} Requirements:";
            errorMsg += $"{indent}  - Title must be equal to '{itemTitleAndLabel}'";
            errorMsg += $"{indent}  - Contain only a single '{itemTitleAndLabel}' label";
            errorMsg += $"{indent}  - The milestone should only contain 1 release {itemTypeStr}.";

            errors.Add(errorMsg);
        }

        if (allReleaseItems.Length == 1)
        {
            allReleaseItems.LogAsInfo(15);
        }
        else
        {
            allReleaseItems.LogAsError(15);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    private bool ThatAllOfTheReleaseMilestoneIssuesAreClosed(ReleaseType releaseType, bool skipReleaseToDoIssues)
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatAllOfTheReleaseMilestoneIssuesAreClosed)
            .LogRequirementTitle($"Checking that all of the release milestone issues are closed.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;
        projectVersion = projectVersion.StartsWith('v')
            ? projectVersion
            : $"v{projectVersion}";

        var milestoneUrl = GitHubClient.Issue.Milestone.GetHtmlUrl(RepoOwner, RepoName, projectVersion).Result;

        var openMilestoneIssues = GitHubClient.Issue.IssuesForMilestone(RepoOwner, RepoName, projectVersion)
            .Result
            .Where(i => skipReleaseToDoIssues is false && i.IsReleaseToDoIssue(releaseType) && i.State == ItemState.Open).ToArray();

        if (openMilestoneIssues.Length > 0)
        {
            var errorMsg = $"The milestone for version '{projectVersion}' contains opened issues.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To view the opened issues for the milestone, go here 👉🏼 {milestoneUrl}";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();
        openMilestoneIssues.LogAsError(15);

        return false;
    }

    private bool ThatAllOfTheReleaseMilestonePullRequestsAreClosed(ReleaseType releaseType, bool skipReleaseToDoPullRequests)
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatAllOfTheReleaseMilestonePullRequestsAreClosed)
            .LogRequirementTitle($"Checking that all of the release milestone pull requests are closed.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;

        var milestoneUrl = GitHubClient.Issue.Milestone.GetHtmlUrl(RepoOwner, RepoName, $"v{projectVersion}").Result;

        var openMilestonePullRequests = GitHubClient.Issue.PullRequestsForMilestone(RepoOwner, RepoName, $"v{projectVersion}")
            .Result
            .Where(i => skipReleaseToDoPullRequests is false && i.IsReleasePullRequest(releaseType) && i.State == ItemState.Open).ToArray();

        if (openMilestonePullRequests.Length > 0)
        {
            var errorMsg = $"The milestone for version '{projectVersion}' contains opened pull requests.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To view the opened pull requests for the milestone, go here 👉🏼 {milestoneUrl}";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();
        openMilestonePullRequests.LogAsError(15);

        return false;
    }

    private bool ThatAllMilestoneIssuesHaveLabels()
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatAllMilestoneIssuesHaveLabels)
            .LogRequirementTitle($"Checking that all issues in the milestone have a label.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;
        var milestoneTitle = $"v{projectVersion}";
        var milestoneClient = GitHubClient.Issue.Milestone;
        var milestoneUrl = milestoneClient.GetHtmlUrl(RepoOwner, RepoName, milestoneTitle).Result;

        var issueClient = GitHubClient.Issue;

        var milestoneIssues = issueClient.IssuesForMilestone(RepoOwner, RepoName, milestoneTitle).Result;

        var issueHasNoLabels = milestoneIssues.Any(i => i.Labels.Count <= 0);

        if (issueHasNoLabels)
        {
            var errorMsg = $"The milestone '{milestoneTitle}' contains at least 1 issue that has no labels.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To view the milestone, go here 👉🏼 {milestoneUrl}";
            errors.Add(errorMsg);

            errors.Add(milestoneIssues.GetLogText(15));
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    /// <summary>
    /// Returns a value indicating whether or not all of the items of the given <paramref name="itemType"/> are assigned.
    /// </summary>
    /// <returns><c>true</c> if all of the items are assigned.</returns>
    private bool ThatAllMilestoneItemsAreAssigned(ItemType itemType)
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        var itemTypeStr = itemType.ToString().ToSpaceDelimitedSections().ToLower();

        nameof(ThatAllMilestoneItemsAreAssigned)
            .LogRequirementTitle($"Checking that all {itemTypeStr} in the milestone are assigned.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;
        var milestoneTitle = $"v{projectVersion}";
        var issueClient = GitHubClient.Issue;

        var items = issueClient.IssuesForMilestone(RepoOwner, RepoName, milestoneTitle)
            .Result
            .Where(i => itemType switch
        {
            ItemType.Issue => i.IsIssue(),
            ItemType.PullRequest => i.IsPullRequest(),
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, $"{nameof(itemType)} is out of range.")
        }).ToArray();

        var unassignedIssues = items.Where(i => i.Assignee is null).Select(i => i).ToArray();

        if (unassignedIssues.Length > 0)
        {
            var errorMsg = $"The milestone '{milestoneTitle}' contains issues that are not assigned.";
            errorMsg += $"{Environment.NewLine}{unassignedIssues.GetLogText(15)}";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors("1 or more issues are not assigned.");

        return false;
    }

    private bool ThatAllMilestonePullRequestsHaveLabels()
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatAllMilestonePullRequestsHaveLabels)
            .LogRequirementTitle("Checking that all pull requests in the milestone have a label.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;
        var milestoneTitle = $"v{projectVersion}";
        var milestoneClient = GitHubClient.Issue.Milestone;
        var milestoneUrl = milestoneClient.GetHtmlUrl(RepoOwner, RepoName, milestoneTitle).Result;

        var issueClient = GitHubClient.Issue;

        var milestonePullRequests = issueClient.PullRequestsForMilestone(RepoOwner, RepoName, milestoneTitle).Result;

        var issueHasNoLabels = milestonePullRequests.Any(i => i.Labels.Count <= 0);

        if (issueHasNoLabels)
        {
            var errorMsg = $"The milestone '{milestoneTitle}' contains at least 1 pull request that has no labels.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To view the milestone, go here 👉🏼 {milestoneUrl}";
            errors.Add(errorMsg);
            errors.Add(milestonePullRequests.GetLogText(15));
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    /// <summary>
    /// Verifies that a pull request is assigned to a milestone.
    /// </summary>
    /// <returns><c>true</c> if assigned to a milestone.</returns>
    private bool ThatThePRIsAssignedToMilestone()
    {
        nameof(ThatAllMilestonePullRequestsHaveLabels)
            .LogRequirementTitle("Checking that the pull request is assigned to a milestone.");

        if (PullRequestNumber <= 0)
        {
            Log.Error("The pull request number is not valid.");
            return false;
        }

        (var isAssigned, Milestone? milestone) = GitHubClient.PullRequest.AssignedToMilestone(RepoOwner, RepoName, PullRequestNumber).Result;

        if (isAssigned)
        {
            var title = milestone?.Title ?? string.Empty;
            Console.WriteLine($"{ConsoleTab}The pull request '{PullRequestNumber}' is assigned to milestone '{title}'.");
            return true;
        }

        Log.Error($"The pull request '{PullRequestNumber}' is not assigned to a milestone.");
        Assert.Fail("Pull request not assigned to a milestone.");
        return false;
    }

    private bool ThatTheReleaseTagDoesNotAlreadyExist(ReleaseType releaseType)
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        var releaseTypeStr = releaseType.ToString().ToLower();

        nameof(ThatTheReleaseTagDoesNotAlreadyExist)
            .LogRequirementTitle($"Checking that a {releaseTypeStr} release tag that matches the set project version does not already exist.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;

        var repoClient = GitHubClient.Repository;
        var tagExists = repoClient.TagExists(RepoOwner, RepoName, $"v{projectVersion}").Result;

        if (tagExists)
        {
            var tagUrl = $"https://github.com/{RepoOwner}/{RepoName}/tree/{projectVersion}";
            var errorMsg = $"The {releaseTypeStr} release tag '{projectVersion}' already exists.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To view the tag, go here 👉🏼 {tagUrl}";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors($"The {releaseTypeStr} release tag already exists.");

        return false;
    }

    private bool ThatTheReleaseNotesExist(ReleaseType releaseType)
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        var releaseTypeStr = releaseType.ToString().ToLower();

        nameof(ThatTheReleaseNotesExist)
            .LogRequirementTitle($"Checking that the release notes for the {releaseTypeStr} release exist.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;

        var releaseNotesDoNotExist = ReleaseNotesDoNotExist(releaseType, projectVersion);

        if (releaseNotesDoNotExist)
        {
            var notesDirPath = $"./Documentation/ReleaseNotes/{releaseType.ToString()}Releases";
            var errorMsg = $"The {releaseTypeStr} release notes do not exist for version {projectVersion}";
            var notesFileName = $"Release-Notes-v{projectVersion}.md";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}The {releaseTypeStr} release notes go in the directory '{notesDirPath}'";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}The {releaseTypeStr} release notes file name should be '{notesFileName}'.";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors($"The {releaseTypeStr} release notes do not exist.");

        return false;
    }

    private bool ThatTheReleaseNotesTitleIsCorrect(ReleaseType releaseType)
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        var releaseTypeStr = releaseType.ToString().ToLower();

        nameof(ThatTheReleaseNotesTitleIsCorrect)
            .LogRequirementTitle($"Checking that the release notes for the {releaseTypeStr} release exist.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;

        var releaseNotesDoNotExist = ReleaseNotesDoNotExist(releaseType, projectVersion);

        if (releaseNotesDoNotExist)
        {
            var notesDirPath = $"./Documentation/ReleaseNotes/{releaseType.ToString()}Releases";
            var errorMsg = $"The {releaseTypeStr} release notes do not exist for version {projectVersion}";
            var notesFileName = $"Release-Notes-v{projectVersion}.md";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}The {releaseTypeStr} release notes go in the directory '{notesDirPath}'";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}The {releaseTypeStr} release notes file name should be '{notesFileName}'.";
            errors.Add(errorMsg);
        }

        var releaseNotes = SolutionService.GetReleaseNotesAsLines(releaseType, projectVersion);

        var releaseNotesTitleSection = $"{RepoName} {releaseType} Release Notes - ";
        var foundTitle = releaseNotes.Where(l => l.Contains(releaseNotesTitleSection)).ToArray();

        if (foundTitle.Length <= 0)
        {
            var expectedReleaseNotesTitle = $"{RepoName} {releaseType} Release Notes - v{projectVersion}";
            const string titleSyntax = "<project-name> <release-type> Release Notes - v#.#.#-preview.#";

            var errorMsg = $"A release notes title with the syntax '{titleSyntax}' could not be found.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}Expected Title: {expectedReleaseNotesTitle}";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    private bool ThatMilestoneIssuesExistInReleaseNotes(ReleaseType releaseType)
    {
        const int totalIndexSpaces = 15;
        var indent = totalIndexSpaces.CreateDuplicateCharacters(' ');
        const string baseUrl = "https://github.com";
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        var releaseTypeStr = releaseType.ToString().ToLower();

        nameof(ThatMilestoneIssuesExistInReleaseNotes)
            .LogRequirementTitle($"Checking that the {releaseTypeStr} release notes contain the release issues.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;
        var milestoneTitle = $"v{projectVersion}";

        var milestoneIssues = GitHubClient.Issue.IssuesForMilestone(RepoOwner, RepoName, milestoneTitle).Result;
        var releaseNotes = SolutionService.GetReleaseNotes(releaseType, projectVersion);
        if (string.IsNullOrEmpty(releaseNotes))
        {
            errors.Add($"No {releaseTypeStr} release notes exist to check for issue numbers.");
        }

        var issuesNotInReleaseNotes = string.IsNullOrEmpty(releaseNotes)
            ? Array.Empty<Issue>()
            : milestoneIssues.Where(i =>
            {
                var issueNote = $"[#{i.Number}]({baseUrl}/{RepoOwner}/{RepoName}/issues/{i.Number})";
                return !i.IsReleaseToDoIssue(releaseType) &&
                       !releaseNotes.Contains(issueNote);
            }).ToArray();

        if (issuesNotInReleaseNotes.Length > 0)
        {
            var errorMsg = $"The {releaseTypeStr} release notes does not contain any notes for the following issues.";
            errorMsg += $"{Environment.NewLine}{indent}{issuesNotInReleaseNotes.GetLogText(totalIndexSpaces)}";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors($"The {releaseTypeStr} release notes is missing notes for 1 ore more issues.");

        return false;
    }

    private bool ThatTheProdReleaseNotesContainsPreviewReleaseSection()
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatTheProdReleaseNotesContainsPreviewReleaseSection)
            .LogRequirementTitle("Checking if the 'production' release notes contains a preview releases section if required.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var prodVersion = project?.GetVersion() ?? string.Empty;
        prodVersion = $"v{prodVersion}";
        var isProdVersion = prodVersion.IsProductionVersion();

        if (isProdVersion is false)
        {
            var errorMsg = $"The version '{prodVersion}' must be a production version.";
            errors.Add(errorMsg);
        }

        var containsPreviewReleases = isProdVersion && ProdVersionHasPreviewReleases(prodVersion).Result;

        if (containsPreviewReleases)
        {
            var releaseNotes = SolutionService.GetReleaseNotes(ReleaseType.Production, prodVersion);

            if (string.IsNullOrEmpty(releaseNotes))
            {
                const string notesDirPath = $"./Documentation/ReleaseNotes/ProductionReleases";
                var errorMsg = $"The production release notes do not exist for version {prodVersion}";
                var notesFileName = $"Release-Notes-{prodVersion}.md";
                errorMsg += $"{Environment.NewLine}{ConsoleTab}The production release notes go in the directory '{notesDirPath}'";
                errorMsg += $"{Environment.NewLine}{ConsoleTab}The production release notes file name should be '{notesFileName}'.";
                errors.Add(errorMsg);
            }
            else
            {
                const string previewReleaseSection = "<h2 style=\"font-weight:bold\" align=\"center\">Preview Releases 🚀</h2>";

                if (releaseNotes.Contains(previewReleaseSection) is false)
                {
                    var errorMsg = $"The production release '{prodVersion}' release notes does not contain a preview release section.";
                    errorMsg += $"{Environment.NewLine}{ConsoleTab}This section is required if the production release contains previous preview releases.";
                    errorMsg += $"{Environment.NewLine}{ConsoleTab}Expected Section: {previewReleaseSection}";

                    errors.Add(errorMsg);
                }
            }
        }

        if (errors.Count <= 0)
        {
            var logMsg = $"The production release '{prodVersion}' does not have any previous preview releases.";
            logMsg += $"{Environment.NewLine}{ConsoleTab}Release notes check for preview release items complete.";
            Log.Information(logMsg);
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    private bool ThatTheProdReleaseNotesContainsPreviewReleaseItems()
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatTheProdReleaseNotesContainsPreviewReleaseSection)
            .LogRequirementTitle("Checking if the 'production' release notes contains a preview releases section if required.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var prodVersion = project?.GetVersion() ?? string.Empty;
        prodVersion = $"v{prodVersion}";
        var isProdVersion = prodVersion.IsProductionVersion();

        if (isProdVersion is false)
        {
            var errorMsg = $"The version '{prodVersion}' must be a production version.";
            errors.Add(errorMsg);
        }

        var containsPreviewReleases = isProdVersion && ProdVersionHasPreviewReleases(prodVersion).Result;

        if (containsPreviewReleases)
        {
            var releaseNotes = SolutionService.GetReleaseNotes(ReleaseType.Production, prodVersion);

            if (string.IsNullOrEmpty(releaseNotes))
            {
                const string notesDirPath = $"./Documentation/ReleaseNotes/ProductionReleases";
                var errorMsg = $"The production release notes do not exist for version {prodVersion}";
                var notesFileName = $"Release-Notes-{prodVersion}.md";
                errorMsg += $"{Environment.NewLine}{ConsoleTab}The production release notes go in the directory '{notesDirPath}'";
                errorMsg += $"{Environment.NewLine}{ConsoleTab}The production release notes file name should be '{notesFileName}'.";
                errors.Add(errorMsg);
            }
            else
            {
                var milestoneRequest = new MilestoneRequest { State = ItemStateFilter.All };

                var prevReleaseItems =
                    (from m in GitHubClient.Issue.Milestone.GetAllForRepository(RepoOwner, RepoName, milestoneRequest).Result
                        where m.Title.IsPreviewVersion() && m.Title.StartsWith(prodVersion)
                        select (
                            m.Title,
                            prevReleaseItem: $"[Preview Release {m.Title}]({m.HtmlUrl}) - Issues from preview release.")).ToArray();

                // Check if any of the preview release items do not exist
                var itemsThatDoNotExist =
                    prevReleaseItems.Where(m => releaseNotes.Contains(m.prevReleaseItem) is false).ToArray();

                if (itemsThatDoNotExist.Length > 0)
                {
                    const string milestoneUrl = "https://github.com/<owner>/<repo-name>/milestone/<milestone-number>";
                    errors.Add($"Preview Release Item Syntax: [Preview Release <preview-version>]({milestoneUrl}) - Issues from preview release.");
                }

                // Add errors for each item that does not exist
                foreach (var item in itemsThatDoNotExist)
                {
                    errors.Add($"The preview release item for preview release '{item.Title}' is missing.");
                }
            }
        }

        if (errors.Count <= 0)
        {
            var logMsg = $"The production release '{prodVersion}' does not have any previous preview releases.";
            logMsg += $"{Environment.NewLine}{ConsoleTab}Release notes check for preview release items complete.";
            Log.Information(logMsg);
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    private bool ThatGitHubReleaseDoesNotExist(ReleaseType releaseType)
    {
        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        var releaseTypeStr = releaseType.ToString().ToLower();

        nameof(ThatGitHubReleaseDoesNotExist)
            .LogRequirementTitle($"Checking that the {releaseTypeStr} GitHub release does not already exist.");

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var projectVersion = project?.GetVersion() ?? string.Empty;

        var versionSyntax = releaseType switch
        {
            ReleaseType.Preview => "#.#.#-preview.#",
            ReleaseType.Production => "#.#.#",
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
        };

        var versionSyntaxValid = project?.HasCorrectVersionSyntax(versionSyntax);

        if (versionSyntaxValid is false)
        {
            var errorMsg = $"The set project version '{projectVersion}' has invalid syntax.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}Required Version Syntax: '{versionSyntax}'";
            errors.Add(errorMsg);
        }

        var releaseTag = $"v{projectVersion}";

        var releaseClient = GitHubClient.Repository.Release;

        var releaseExists = releaseClient.ReleaseExists(RepoOwner, RepoName, releaseTag).Result;

        if (releaseExists)
        {
            var errorMsg = $"The {releaseTypeStr} release for version '{releaseTag}' already exists.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}Verify that the project versions have been correctly updated.";
            errors.Add(errorMsg);
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }

    private bool ThatWorkflowOutputDirPathIsValid()
    {
        var switchStr = $"--{nameof(WorkflowTemplateOutput).ToKebabCase()}";
        var example = $"Example: {switchStr} \"C:/my-destination-dir-path\"";

        if (WorkflowTemplateOutput == null)
        {
            var errorMsg = $"The switch '{switchStr}' must be used and contain a value.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}{example}";
            Log.Error(errorMsg);
        }
        else if (WorkflowTemplateOutput == string.Empty)
        {
            var errorMsg = $"The switch '{switchStr}' must contain a value.  Example:";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}{example}";
            Log.Error(errorMsg);
        }
        else
        {
            return true;
        }

        Assert.Fail($"The '{switchStr}' switch is invalid or not used.");
        return false;
    }

    private bool ThatTheNugetPackageDoesNotExist()
    {
        nameof(ThatTheNugetPackageDoesNotExist)
            .LogRequirementTitle("Checking that the nuget package does not already exist.");

        var project = SolutionService.GetProject(RepoName);
        var errors = new List<string>();

        if (project is null)
        {
            var exMsg = $"The project named '{ProjectName}' could not be found.";
            exMsg += $"{Environment.NewLine}Check that the 'ProjectName' param in the parameters.json is set correctly.";
            throw new Exception(exMsg);
        }

        var projectVersion = project.GetVersion();

        var nugetService = new NugetDataService();
        var packageVersions = Array.Empty<string>();

        try
        {
            packageVersions = nugetService.GetNugetVersions("CICDTest").Result;
        }
        catch (AggregateException e) when (e.InnerException is HttpRequestException exception)
        {
            if (exception.StatusCode == HttpStatusCode.NotFound)
            {
                return true;
            }
        }

        var nugetPackageExists = packageVersions.Any(i => i == projectVersion);

        if (nugetPackageExists)
        {
            errors.Add($"The nuget package '{RepoName}' version 'v{projectVersion}' already exists.");
        }

        if (errors.Count <= 0)
        {
            return true;
        }

        errors.PrintErrors();

        return false;
    }
}
