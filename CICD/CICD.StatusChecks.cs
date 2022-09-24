// <copyright file="CICD.StatusChecks.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem;

using System;
using System.Threading.Tasks;
using Nuke.Common;
using Serilog;

/// <summary>
/// Contains all of the status check targets and related methods.
/// </summary>
public partial class CICD // StatusChecks
{
    // ReSharper disable UnusedMember.Local

    /// <summary>
    /// Gets a target to perform a build status check.
    /// </summary>
    private Target BuildStatusCheck => _ => _
        .Before(BuildAllProjects)
        .Triggers(BuildAllProjects)
        .Executes(async () =>
        {
            Log.Information("💡Purpose: Verifies that all projects build for the solution.");
            Log.Information("✅Starting Status Check . . .");

            PrintPullRequestInfo();
            await ValidateBranch();

            Log.Information("Branch Is Valid!!");
        });

    /// <summary>
    /// Gets a target to perform a unit test status check.
    /// </summary>
    private Target UnitTestStatusCheck => _ => _
        .Before(RunAllUnitTests)
        .Triggers(RunAllUnitTests)
        .Executes(async () =>
        {
            Log.Information("💡Purpose: Verifies that all unit tests for all of the solution projects pass.");
            Console.WriteLine();
            Log.Information("✅Starting Status Check . . .");

            PrintPullRequestInfo();
            await ValidateBranch();

            Log.Information("Branch Is Valid!!");
        });

    /// <summary>
    /// Gets a target to perform a feature pull request status check.
    /// </summary>
    private Target FeaturePRStatusCheck => _ => _
        .Requires(
            () => ThatThisIsExecutedFromPullRequest(BranchType.Develop),
            () => ThatThePRSourceBranchIsValid(BranchType.Feature),
            () => ThatFeaturePRIssueNumberExists(),
            () => ThatFeaturePRIssueHasLabel(BranchType.Feature),
            () => ThatThePRTargetBranchIsValid(BranchType.Develop),
            () => ThatThePRHasBeenAssigned(),
            () => ThatPRHasLabels());

    /// <summary>
    /// Gets a target to perform a preview feature pull request status check.
    /// </summary>
    private Target PreviewFeaturePRStatusCheck => _ => _
        .Requires(
            () => ThatThisIsExecutedFromPullRequest(BranchType.PreviewFeature),
            () => ThatThePRSourceBranchIsValid(BranchType.PreviewFeature),
            () => ThatPreviewFeaturePRIssueNumberExists(),
            () => ThatFeaturePRIssueHasLabel(BranchType.PreviewFeature),
            () => ThatThePRTargetBranchIsValid(BranchType.Preview),
            () => ThatThePRHasBeenAssigned(),
            () => ThatPRHasLabels());

    private Target HotFixPRStatusCheck => _ => _
        .Requires(
            () => ThatThisIsExecutedFromPullRequest(BranchType.Master),
            () => ThatThePRSourceBranchIsValid(BranchType.HotFix),
            () => ThatPreviewFeaturePRIssueNumberExists(),
            () => ThatFeaturePRIssueHasLabel(BranchType.HotFix),
            () => ThatThePRTargetBranchIsValid(BranchType.Master),
            () => ThatThePRHasBeenAssigned(),
            () => ThatPRHasLabels());

    private Target PrevReleasePRStatusCheck => _ => _
        .Requires(
            () => ThatThisIsExecutedFromPullRequest(BranchType.Release),
            () => ThatThePRSourceBranchIsValid(BranchType.Preview),
            () => ThatThePRTargetBranchIsValid(BranchType.Release),
            () => ThatThePRHasBeenAssigned(),
            () => ThatThePRHasTheLabel("🚀Preview Release"),
            () => ThatTheProjectVersionsAreValid(ReleaseType.Preview),
            () => ThatThePreviewPRBranchVersionsMatch(ReleaseType.Preview),
            () => ThatThePRSourceBranchVersionSectionMatchesProjectVersion(ReleaseType.Preview),
            () => ThatTheReleaseMilestoneExists(),
            () => ThatTheReleaseMilestoneContainsIssues(),
            () => ThatTheReleaseTagDoesNotAlreadyExist(ReleaseType.Preview),
            () => ThatAllMilestoneIssuesHaveLabels(),
            () => ThatAllOfTheReleaseMilestoneIssuesAreClosed(ReleaseType.Preview, true),
            () => ThatAllOfTheReleaseMilestonePullRequestsAreClosed(ReleaseType.Preview, true),
            () => ThatTheReleaseMilestoneOnlyContainsSingle(ReleaseType.Preview, ItemType.Issue),
            () => ThatTheReleaseMilestoneOnlyContainsSingle(ReleaseType.Preview, ItemType.PullRequest),
            () => ThatTheReleaseNotesExist(ReleaseType.Preview),
            () => ThatTheReleaseNotesTitleIsCorrect(ReleaseType.Preview),
            () => ThatMilestoneIssuesExistInReleaseNotes(ReleaseType.Preview),
            () => ThatGitHubReleaseDoesNotExist(ReleaseType.Preview),
            () => NugetPackageDoesNotExist());

    private Target ProdReleasePRStatusCheck => _ => _
        .Requires(
            () => ThatThisIsExecutedFromPullRequest(BranchType.Master, BranchType.Develop),
            () => ThatThePRSourceBranchIsValid(BranchType.Release),
            () => ThatThePRTargetBranchIsValid(BranchType.Master),
            () => ThatThePRHasBeenAssigned(),
            () => ThatThePRHasTheLabel("🚀Production Release"),
            () => ThatTheProjectVersionsAreValid(ReleaseType.Production),
            () => ThatThePreviewPRBranchVersionsMatch(ReleaseType.Production),
            () => ThatThePRSourceBranchVersionSectionMatchesProjectVersion(ReleaseType.Production),
            () => ThatTheReleaseMilestoneExists(),
            () => ThatTheReleaseMilestoneContainsIssues(),
            () => ThatTheReleaseTagDoesNotAlreadyExist(ReleaseType.Production),
            () => ThatAllMilestoneIssuesHaveLabels(),
            () => ThatAllOfTheReleaseMilestoneIssuesAreClosed(ReleaseType.Production, true),
            () => ThatAllOfTheReleaseMilestonePullRequestsAreClosed(ReleaseType.Production, true),
            () => ThatTheReleaseMilestoneOnlyContainsSingle(ReleaseType.Production, ItemType.Issue),
            () => ThatTheReleaseMilestoneOnlyContainsSingle(ReleaseType.Production, ItemType.PullRequest),
            () => ThatTheReleaseNotesExist(ReleaseType.Production),
            () => ThatTheReleaseNotesTitleIsCorrect(ReleaseType.Production),
            () => ThatTheProdReleaseNotesContainsPreviewReleaseSection(),
            () => ThatTheProdReleaseNotesContainsPreviewReleaseItems(),
            () => ThatMilestoneIssuesExistInReleaseNotes(ReleaseType.Production),
            () => ThatGitHubReleaseDoesNotExist(ReleaseType.Production),
            () => NugetPackageDoesNotExist());

    private Target DebugTask => _ => _
        .Unlisted()
        .Executes(() =>
        {
        });

    // ReSharper restore UnusedMember.Local

    /// <summary>
    /// Parses the issue number out of the given <paramref name="branch"/> name.
    /// </summary>
    /// <param name="branch">The branch that might contain the issue number.</param>
    /// <returns>The issue number that might of been contained in the branch.</returns>
    /// <remarks>
    ///     If the branch does not contain an issue number, the value of '0' will be returned.
    /// </remarks>
    private static int ParseIssueNumber(string branch)
    {
        if (string.IsNullOrEmpty(branch))
        {
            return 0;
        }

        if (branch.IsFeatureBranch())
        {
            // feature/123-my-branch
            var mainSections = branch.Split("/");
            var number = mainSections[1].Split('-')[0];
            return int.Parse(number);
        }

        if (branch.IsPreviewFeatureBranch())
        {
            // preview/feature/123-my-preview-branch
            var mainSections = branch.Split("/");
            var number = mainSections[2].Split('-')[0];
            return int.Parse(number);
        }

        if (branch.IsHotFixBranch())
        {
            // hotfix/123-my-hotfix
            var mainSections = branch.Split("/");
            var number = mainSections[1].Split('-')[0];
            return int.Parse(number);
        }

        return 0;
    }

    /// <summary>
    /// Returns a value indicating if the issue number in the given <paramref name="branch"/> is a valid issue number.
    /// </summary>
    /// <param name="branch">The branch to validate.</param>
    /// <returns>
    /// True if the issue number is an issue number that exists.
    ///
    /// <para>The branches that contain issue numbers are:
    ///     <list type="bullet">
    ///         <item><see cref="BranchType.Feature"/></item>
    ///         <item><see cref="BranchType.PreviewFeature"/></item>
    ///         <item><see cref="BranchType.HotFix"/></item>
    ///     </list>
    /// </para>
    /// </returns>
    private async Task<bool> ValidBranchIssueNumber(string branch)
    {
        // If the branch is not a branch with an issue number, return as valid
        if (!branch.IsFeatureBranch() && !branch.IsPreviewFeatureBranch() && !branch.IsHotFixBranch())
        {
            return true;
        }

        var issueNumber = ParseIssueNumber(branch);

        var issueClient = GitHubClient.Issue;

        return await issueClient.IssueExists(RepoOwner, RepoName, issueNumber);
    }

    /// <summary>
    /// Validates the current branch.
    /// </summary>
    private async Task ValidateBranch()
    {
        /*
         * TODO: Refactor this to simply return a Task<bool> result. The logging and failure code inside of
         * this method should be performed by the targets that are consuming it.
         */

        var validBranch = false;
        var branch = string.Empty;

        // This is if the workflow is execution locally or manually in GitHub using workflow_dispatch
        bool ValidBranchForManualExecution()
        {
            return (this.Repo.Branch?.IsMasterBranch() ?? false) ||
                   (this.Repo.Branch?.IsDevelopBranch() ?? false) ||
                   (this.Repo.Branch?.IsFeatureBranch() ?? false) ||
                   (this.Repo.Branch?.IsPreviewFeatureBranch() ?? false) ||
                   (this.Repo.Branch?.IsPreviewBranch() ?? false) ||
                   (this.Repo.Branch?.IsReleaseBranch() ?? false) ||
                   (this.Repo.Branch?.IsHotFixBranch() ?? false);
        }

        // If the build is on the server and the GitHubActions object exists
        if (IsServerBuild && GitHubActions is not null)
        {
            validBranch = IsPullRequest()
                ? GitHubActions.BaseRef.IsPreviewBranch() || GitHubActions.BaseRef.IsReleaseBranch() ||
                  GitHubActions.BaseRef.IsDevelopBranch() || GitHubActions.BaseRef.IsMasterBranch()
                : ValidBranchForManualExecution(); // Manual execution

            branch = IsPullRequest() ? GitHubActions.BaseRef : this.Repo.Branch;
        }
        else if (IsLocalBuild || GitHubActions is null)
        {
            validBranch = ValidBranchForManualExecution();
            branch = this.Repo.Branch;
        }

        if (validBranch)
        {
            var validIssueNumber = await ValidBranchIssueNumber(branch);

            validBranch = validIssueNumber;

            if (validIssueNumber is false)
            {
                Log.Error($"The issue number '{ParseIssueNumber(branch)}' in branch '{branch}' does not exist.");
            }
        }

        if (validBranch is false)
        {
            Assert.Fail($"The branch '{branch}' is invalid.");
        }
    }
}
