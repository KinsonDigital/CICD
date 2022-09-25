// <copyright file="CICD.Requirements.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem;

using System;
using System.Collections.Generic;
using System.Linq;
using Nuke.Common;
using Octokit;
using Serilog;

/// <summary>
/// Contains all of the requirement related methods for Target requires setup.
/// </summary>
public partial class CICD // Requirements
{
    private bool ThatThisIsExecutedFromPullRequest(params BranchType[] targetBranches)
    {
        nameof(ThatThisIsExecutedFromPullRequest)
            .LogRequirementTitle($"Checking if the run was started automatically from a pull request.");

        if (IsPullRequest() is false)
        {
            var errorMsg = "Can only be executed automatically from a pull request";
            errorMsg += targetBranches.Length > 0
                ? " on the following branches."
                : ".";

            errorMsg = targetBranches
                .Aggregate(errorMsg, (current, branch) =>
                    current + $"{Environment.NewLine}{ConsoleTab}  - {branch.ToString().ToSpaceDelimitedSections().ToLower()}");

            Log.Error(errorMsg);
            Assert.Fail("Executed automatically.");
        }

        return true;
    }

    private bool ThatThisIsExecutedManually(params BranchType[] targetBranches)
    {
        nameof(ThatThisIsExecutedManually)
            .LogRequirementTitle($"Checking if the run was manually executed.");

        if (IsPullRequest())
        {
            var errorMsg = "Can only be executed automatically from a pull request";
            errorMsg += targetBranches.Length > 0
                ? " on the following branches."
                : ".";

            errorMsg = targetBranches
                .Aggregate(errorMsg, (current, branch) =>
                    current + $"{Environment.NewLine}{ConsoleTab}  - {branch.ToString().ToSpaceDelimitedSections().ToLower()}");

            Log.Error(errorMsg);
            Assert.Fail("Executed from a pull request.");
        }

        return true;
    }

    private bool ThatThePRHasBeenAssigned()
    {
        var prClient = GitHubClient.PullRequest;

        nameof(ThatThePRHasBeenAssigned)
            .LogRequirementTitle($"Checking if the pull request as been assigned to someone.");

        var prNumber = GitHubActions?.PullRequestNumber ?? -1;

        if (prClient.HasAssignees(RepoOwner, RepoName, prNumber).Result)
        {
            Log.Information($"{ConsoleTab}✅The pull request '{prNumber}' is properly assigned.");
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

    private bool ThatFeaturePRIssueNumberExists()
    {
        var sourceBranch = GitHubActions?.HeadRef ?? string.Empty;

        nameof(ThatFeaturePRIssueNumberExists)
            .LogRequirementTitle($"Checking that the issue number in the feature branch exists.");

        var branchIssueNumber = ExtractIssueNumber(BranchType.Feature, sourceBranch);
        var issueExists = GitHubClient.Issue.IssueExists(RepoOwner, RepoName, branchIssueNumber).Result;

        if (issueExists is false)
        {
            var errorMsg = $"The issue '{branchIssueNumber}' does not exist for feature branch '{sourceBranch}'.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}The source branch '{sourceBranch}' must be recreated with the correct issue number.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}The syntax requirements for a feature branch is 'feature/#-*'.";
            Log.Error(errorMsg);
            Assert.Fail("The feature branch issue number does not exist.");
            return false;
        }

        Log.Information($"{ConsoleTab}✅The feature branch '{sourceBranch}' is valid.");

        return true;
    }

    private bool ThatPreviewFeaturePRIssueNumberExists()
    {
        var sourceBranch = GitHubActions?.HeadRef ?? string.Empty;

        nameof(ThatPreviewFeaturePRIssueNumberExists)
            .LogRequirementTitle($"Checking that the issue number in the preview feature branch exists.");

        var branchIssueNumber = ExtractIssueNumber(BranchType.PreviewFeature, sourceBranch);
        var issueExists = GitHubClient.Issue.IssueExists(RepoOwner, RepoName, branchIssueNumber).Result;

        if (issueExists is false)
        {
            var errorMsg = $"The issue '{branchIssueNumber}' does not exist for preview feature branch '{sourceBranch}'.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}The source branch '{sourceBranch}' must be recreated with the correct issue number.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}The syntax requirements for a preview feature branch is 'preview/feature/#-*'.";
            Log.Error(errorMsg);
            Assert.Fail("The preview feature branch issue number does not exist.");
            return false;
        }

        Log.Information($"{ConsoleTab}✅The preview feature branch '{sourceBranch}' is valid.");

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
            .LogRequirementTitle($"Checking that the issue number in the '{branchTypeStr}' branch exists.");

        // If the branch type is invalid
        if (validBranchTypes.Contains(branchType) is false)
        {
            errors.Add($"The branch type '{branchType}' is not valid for the '{nameof(ThatFeaturePRIssueHasLabel)}' check.");
        }
        else
        {
            var sourceBranch = GitHubActions?.HeadRef ?? string.Empty;
            var branchIssueNumber = ExtractIssueNumber(branchType, sourceBranch);
            var issueExists = GitHubClient.Issue.IssueExists(RepoOwner, RepoName, branchIssueNumber).Result;

            if (issueExists)
            {
                var containsLabels = GitHubClient.Issue.HasLabels(RepoOwner, RepoName, branchIssueNumber).Result;

                if (containsLabels)
                {
                    Log.Information($"{ConsoleTab}✅The issue '{branchIssueNumber}' contains at least 1 label.");
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

        var prNumber = GitHubActions is null || GitHubActions.PullRequestNumber is null
            ? -1
            : (int)GitHubActions.PullRequestNumber;

        if (prClient.HasLabels(RepoOwner, RepoName, prNumber).Result)
        {
            Log.Information($"{ConsoleTab}✅The pull request '{prNumber}' has labels.");
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

    private bool ThatThePRHasTheLabel(string labelName)
    {
        var prNumber = GitHubActions?.PullRequestNumber ?? -1;

        nameof(ThatThePRHasTheLabel)
            .LogRequirementTitle($"Checking if the pull request has a preview release label.");

        if (prNumber is -1)
        {
            const string errorMsg = "The pull request number could not be found.  This must only run as a pull request in GitHub, not locally.";
            Log.Error(errorMsg);
            Assert.Fail("The workflow is not being executed as a pull request in the GitHub environment.");
        }

        var labelExists = GitHubClient.PullRequest.LabelExists(RepoOwner, RepoName, prNumber, labelName).Result;

        if (labelExists)
        {
            Log.Information($"{ConsoleTab}✅The pull request '{prNumber}' has a preview label.");
        }
        else
        {
            var prLink = $"https://github.com/{RepoOwner}/{RepoName}/pull/{prNumber}";
            var errorMsg = $"The pull request '{{Value1}}' does not have the preview release label '{labelName}'.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To add the label, go to 👉🏼 '{{Value2}}'.";
            Log.Error(errorMsg, prNumber, prLink);
            Assert.Fail("The pull request does not have a preview release label.");
        }

        return true;
    }

    private bool ThatThePRTargetBranchIsValid(BranchType branchType)
    {
        var targetBranch = GitHubActions?.BaseRef ?? string.Empty;
        var validMsg = string.Empty;
        var isValidBranch = false;

        nameof(ThatThePRTargetBranchIsValid)
            .LogRequirementTitle($"Checking if pull request target branch '{targetBranch}' is valid.");

        var branchTypeStr = branchType.ToString().ToSpaceDelimitedSections().ToLower();
        validMsg += $"{Environment.NewLine}{ConsoleTab}✅The '{branchTypeStr}' branch '{targetBranch}' valid.";

        var branchSyntax = GetBranchSyntax(branchType);
        var errorMsg = $"The {branchTypeStr} branch '{{Value}}' is invalid.";
        errorMsg += $"{Environment.NewLine}{ConsoleTab}The syntax for the develop branch is '{branchSyntax}'.";

        isValidBranch = branchType switch
        {
            BranchType.Develop => targetBranch.IsDevelopBranch(),
            BranchType.Master => targetBranch.IsMasterBranch(),
            BranchType.Feature => targetBranch.IsFeatureBranch(),
            BranchType.PreviewFeature => targetBranch.IsPreviewFeatureBranch(),
            BranchType.Release => targetBranch.IsReleaseBranch(),
            BranchType.Preview => targetBranch.IsPreviewBranch(),
            BranchType.HotFix => targetBranch.IsHotFixBranch(),
            _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, null)
        };

        if (isValidBranch)
        {
            Log.Information(validMsg);
            return true;
        }

        Log.Error(errorMsg, targetBranch);
        var runType = IsPullRequest() ? "pull request" : "manual";
        Assert.Fail($"Invalid target branch for the {runType} run.");
        return false;
    }

    private bool ThatTheCurrentBranchIsCorrect(BranchType branchType)
    {
        var branchTypeStr = branchType.ToString().ToLower();

        nameof(ThatTheCurrentBranchIsCorrect)
            .LogRequirementTitle($"Checking that the current branch is a {branchTypeStr} branch.");

        if (string.IsNullOrEmpty(this.Repo.Branch))
        {
            return false;
        }

        var isCorrectBranch = branchType switch
        {
            BranchType.Master => this.Repo.Branch.IsMasterBranch(),
            BranchType.Develop => this.Repo.Branch.IsDevelopBranch(),
            BranchType.Feature => this.Repo.Branch.IsFeatureBranch(),
            BranchType.PreviewFeature => this.Repo.Branch.IsPreviewFeatureBranch(),
            BranchType.Release => this.Repo.Branch.IsReleaseBranch(),
            BranchType.Preview => this.Repo.Branch.IsPreviewBranch(),
            BranchType.HotFix => this.Repo.Branch.IsHotFixBranch(),
            BranchType.Other => true,
            _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, null)
        };

        if (isCorrectBranch is false)
        {
            Log.Error($"The current branch {this.Repo.Branch} is not a '{branchTypeStr}' branch.");
            Assert.Fail("The current branch is incorrect.");
        }

        return true;
    }

    private bool ThatThePRSourceBranchIsValid(BranchType branchType)
    {
        var sourceBranch = GitHubActions?.HeadRef ?? string.Empty;
        var validMsg = string.Empty;
        var isValidBranch = false;

        nameof(ThatThePRSourceBranchIsValid)
            .LogRequirementTitle("Validating Pull Request Source Branch:");

        validMsg += $"{Environment.NewLine}{ConsoleTab}✅The '{branchType}' branch '{sourceBranch}' is valid.";
        var branchTypeStr = branchType.ToString().ToSpaceDelimitedSections().ToLower();
        var branchSyntax = GetBranchSyntax(branchType);
        var errorMsg = $"The {branchTypeStr} branch '{{Value}}' is invalid.";
        errorMsg += $"{Environment.NewLine}{ConsoleTab}The syntax for the develop branch is '{branchSyntax}'.";

        isValidBranch = branchType switch
        {
            BranchType.Develop => sourceBranch.IsDevelopBranch(),
            BranchType.Master => sourceBranch.IsMasterBranch(),
            BranchType.Feature => sourceBranch.IsFeatureBranch(),
            BranchType.PreviewFeature => sourceBranch.IsPreviewFeatureBranch(),
            BranchType.Release => sourceBranch.IsReleaseBranch(),
            BranchType.Preview => sourceBranch.IsPreviewBranch(),
            BranchType.HotFix => sourceBranch.IsHotFixBranch(),
            _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, null)
        };

        if (isValidBranch)
        {
            Log.Information(validMsg);
            return true;
        }

        Log.Error(errorMsg, sourceBranch);
        Assert.Fail("Invalid pull request source branch.");
        return false;
    }

    private bool ThatThePreviewPRBranchVersionsMatch(ReleaseType releaseType)
    {
        var sourceBranch = GitHubActions?.HeadRef ?? string.Empty;
        var targetBranch = GitHubActions?.BaseRef ?? string.Empty;
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
        var targetBranch = this.Repo.Branch ?? string.Empty;
        var project = this.Solution.GetProject(RepoName);

        var errors = new List<string>();
        var branchTypeStr = branchType.ToString().ToSpaceDelimitedSections().ToLower();

        if (branchType is BranchType.Preview or BranchType.Release)
        {
            nameof(ThatTheCurrentBranchVersionMatchesProjectVersion)
                .LogRequirementTitle($"Checking that the version section of the {branchTypeStr} branch matches the project version.");
        }
        else
        {
            nameof(ThatTheCurrentBranchVersionMatchesProjectVersion)
                .LogRequirementTitle("No check required.  The target branch is not a branch that contains a version.");
        }

        if (string.IsNullOrEmpty(targetBranch))
        {
            errors.Add("The branch not be null or empty.");
        }

        if (project is null)
        {
            errors.Add($"Could not find the project '{RepoName}'");
        }

        var branchVersion = this.Repo.Branch?.ExtractBranchVersion().version.TrimStart('v');
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
        var project = this.Solution.GetProject(RepoName);
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
                statusMsg += $"The version '{project.GetVersion()}' is valid.{Environment.NewLine}";
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
                statusMsg += $"The file version '{project.GetVersion()}' is valid.{Environment.NewLine}";
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
                statusMsg += $"The assembly version '{project.GetAssemblyVersion()}' is valid.{Environment.NewLine}";
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
            Log.Information($"{statusMsg}{Environment.NewLine}");
            return true;
        }

        errors.PrintErrors();

        return errors.Count <= 0;
    }

    private bool ThatThePRSourceBranchVersionSectionMatchesProjectVersion(ReleaseType releaseType)
    {
        var sourceBranch = GitHubActions?.HeadRef ?? string.Empty;
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
            var project = this.Solution.GetProject(RepoName);
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
        var project = this.Solution.GetProject(RepoName);
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

    private bool ThatTheReleaseMilestoneContainsIssues()
    {
        var project = this.Solution.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatTheReleaseMilestoneContainsIssues)
            .LogRequirementTitle($"Checking that the release milestone for the current version contains issues.");

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

    private bool ThatTheReleaseMilestoneOnlyContainsSingle(ReleaseType releaseType, ItemType itemType)
    {
        const int totalSpaces = 15;
        var project = this.Solution.GetProject(RepoName);
        var errors = new List<string>();
        var releaseTypeStr = releaseType.ToString().ToLower();

        var introMsg = "Checking that the release milestone only contains a single release ";
        introMsg += $"{(itemType == ItemType.Issue ? "todo issue" : "pull request")} item.";
        nameof(ThatTheReleaseMilestoneOnlyContainsSingle)
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

        var pullRequests = itemType switch
        {
            ItemType.Issue => issueClient.IssuesForMilestone(RepoOwner, RepoName, mileStoneTitle).Result,
            ItemType.PullRequest => issueClient.PullRequestsForMilestone(RepoOwner, RepoName, mileStoneTitle).Result,
            _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
        };

        if (pullRequests.Length <= 0)
        {
            var errorMsg = $"The milestone does not contain any {(itemType == ItemType.Issue ? "todo issues." : "pull requests.")}.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}To view the milestone, go here 👉🏼 {milestone?.HtmlUrl}";
            errors.Add(errorMsg);
        }

        var prTitleAndLabel = releaseType switch
        {
            ReleaseType.Preview => "🚀Preview Release",
            ReleaseType.Production => "🚀Production Release",
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
        };

        var allReleasePullRequests = pullRequests.Where(i =>
        {
            return itemType switch
            {
                ItemType.Issue => i.IsReleaseToDoIssue(releaseType),
                ItemType.PullRequest => i.IsReleasePullRequest(releaseType),
                _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
            };
        }).ToArray();
        var indent = Environment.NewLine + totalSpaces.CreateDuplicateCharacters(' ');

        if (allReleasePullRequests.Length != 1)
        {
            var itemTypeStr = itemType switch
            {
                ItemType.Issue => "to do issue",
                ItemType.PullRequest => "pull request",
                _ => throw new ArgumentOutOfRangeException(nameof(itemType), itemType, null)
            };

            var errorMsg =
                $"The {releaseTypeStr} release milestone '{mileStoneTitle}' has '{allReleasePullRequests.Length}' release {itemTypeStr}s.";
            errorMsg += $"{indent}Release milestones should only have a single release {itemTypeStr}.";
            errorMsg += $"{indent}Release {itemTypeStr.CapitalizeWords()} Requirements:";
            errorMsg += $"{indent}  - Title must be equal to '{prTitleAndLabel}'";
            errorMsg += $"{indent}  - Contain only a single '{prTitleAndLabel}' label";
            errorMsg += $"{indent}  - The milestone should only contain 1 release {itemTypeStr}.";

            errors.Add(errorMsg);
        }

        if (allReleasePullRequests.Length == 1)
        {
            allReleasePullRequests.LogAsInfo(15);
        }
        else
        {
            allReleasePullRequests.LogAsError(15);
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
        var project = this.Solution.GetProject(RepoName);
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
            .Where(i => (skipReleaseToDoIssues || i.IsReleaseToDoIssue(releaseType)) && i.State == ItemState.Open).ToArray();

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
        var project = this.Solution.GetProject(RepoName);
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
            .Where(i => (skipReleaseToDoPullRequests || i.IsReleasePullRequest(releaseType)) && i.State == ItemState.Open).ToArray();

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
        var project = this.Solution.GetProject(RepoName);
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

    private bool ThatAllMilestonePullRequestsHaveLabels()
    {
        var project = this.Solution.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatAllMilestonePullRequestsHaveLabels)
            .LogRequirementTitle($"Checking that all pull requests in the milestone have a label.");

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

    private bool ThatTheReleaseTagDoesNotAlreadyExist(ReleaseType releaseType)
    {
        var project = this.Solution.GetProject(RepoName);
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
        var project = this.Solution.GetProject(RepoName);
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
            var notesDirPath = $"~/Documentation/ReleaseNotes/{releaseType.ToString()}Releases";
            var errorMsg = $"The {releaseTypeStr} release notes do not exist for version {projectVersion}";
            var notesFileName = $"Release-Notes-{projectVersion}.md";
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
        var project = this.Solution.GetProject(RepoName);
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
            var notesDirPath = $"~/Documentation/ReleaseNotes/{releaseType.ToString()}Releases";
            var errorMsg = $"The {releaseTypeStr} release notes do not exist for version {projectVersion}";
            var notesFileName = $"Release-Notes-{projectVersion}.md";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}The {releaseTypeStr} release notes go in the directory '{notesDirPath}'";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}The {releaseTypeStr} release notes file name should be '{notesFileName}'.";
            errors.Add(errorMsg);
        }

        var releaseNotes = this.Solution.GetReleaseNotesAsLines(releaseType, projectVersion);

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
        var project = this.Solution.GetProject(RepoName);
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
        var releaseNotes = this.Solution.GetReleaseNotes(releaseType, projectVersion);
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
        var project = this.Solution.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatTheProdReleaseNotesContainsPreviewReleaseSection)
            .LogRequirementTitle($"Checking if the 'production' release notes contains a preview releases section if required.");

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
            var releaseNotes = this.Solution.GetReleaseNotes(ReleaseType.Production, prodVersion);

            if (string.IsNullOrEmpty(releaseNotes))
            {
                const string notesDirPath = $"~/Documentation/ReleaseNotes/ProductionReleases";
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
        var project = this.Solution.GetProject(RepoName);
        var errors = new List<string>();

        nameof(ThatTheProdReleaseNotesContainsPreviewReleaseSection)
            .LogRequirementTitle($"Checking if the 'production' release notes contains a preview releases section if required.");

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
            var releaseNotes = this.Solution.GetReleaseNotes(ReleaseType.Production, prodVersion);

            if (string.IsNullOrEmpty(releaseNotes))
            {
                const string notesDirPath = $"~/Documentation/ReleaseNotes/ProductionReleases";
                var errorMsg = $"The production release notes do not exist for version {prodVersion}";
                var notesFileName = $"Release-Notes-{prodVersion}.md";
                errorMsg += $"{Environment.NewLine}{ConsoleTab}The production release notes go in the directory '{notesDirPath}'";
                errorMsg += $"{Environment.NewLine}{ConsoleTab}The production release notes file name should be '{notesFileName}'.";
                errors.Add(errorMsg);
            }
            else
            {
                var milestoneRequest = new MilestoneRequest();
                milestoneRequest.State = ItemStateFilter.All;

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
        var project = this.Solution.GetProject(RepoName);
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

    private bool ThatTheBuildSettingsDirPathIsValid()
    {
        var switchStr = $"--{nameof(BuildSettingsDirPath).ToKebabCase()}";
        var example = $"Example: {switchStr} \"C:/my-destination-dir-path\"";
        if (BuildSettingsDirPath == null)
        {
            var errorMsg = $"The switch '{switchStr}' must be used and contain a value.";
            errorMsg += $"{Environment.NewLine}{ConsoleTab}{example}";
            Log.Error(errorMsg);
        }
        else if (BuildSettingsDirPath == string.Empty)
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
}
