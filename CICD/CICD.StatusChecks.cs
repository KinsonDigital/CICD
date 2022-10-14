// <copyright file="CICD.StatusChecks.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;
using Nuke.Common;
using Serilog;

namespace CICDSystem;

/// <summary>
/// Contains all of the status check targets and related methods.
/// </summary>
public partial class CICD // StatusChecks
{
    // ReSharper disable UnusedMember.Local

    /// <summary>
    /// Gets a target to perform a build status check.
    /// </summary>
    private Target PRBuildStatusCheck => _ => _
        .Before(BuildAllProjects)
        .Requires(
            () => ThatPullRequestNumberIsProvided(),
            () => ThatThePullRequestExists(),
            () => ThatThePRBranchIsValid(
                PRBranchContext.Source,
                BranchType.Feature,
                BranchType.PreviewFeature,
                BranchType.Release,
                BranchType.HotFix,
                BranchType.Preview),
            () => ThatThePRBranchIsValid(
                PRBranchContext.Target,
                BranchType.Develop,
                BranchType.Master,
                BranchType.Release,
                BranchType.Preview)).Before(BuildAllProjects)
        .Triggers(BuildAllProjects)
        .Executes(() =>
        {
            Log.Information("💡Purpose: Verifies that all projects build for the solution.");
            Log.Information("✅Starting Status Check . . .");
            Log.Information("Branch Is Valid!!");
        });

    /// <summary>
    /// Gets a target to perform a unit test status check.
    /// </summary>
    private Target PRUnitTestStatusCheck => _ => _
        .Before(RunAllUnitTests)
        .Requires(
            () => ThatPullRequestNumberIsProvided(),
            () => ThatThePullRequestExists(),
            () => ThatThePRBranchIsValid(
                PRBranchContext.Source,
                BranchType.Feature,
                BranchType.PreviewFeature,
                BranchType.Release,
                BranchType.HotFix,
                BranchType.Preview),
            () => ThatThePRBranchIsValid(
                PRBranchContext.Target,
                BranchType.Develop,
                BranchType.Master,
                BranchType.Release,
                BranchType.Preview)).Before(RunAllUnitTests)
        .Triggers(RunAllUnitTests)
        .Executes(() =>
        {
            Log.Information("💡Purpose: Verifies that all unit tests for all of the solution projects pass.");
            Console.WriteLine();
            Log.Information("✅Starting Status Check . . .");
            Log.Information("Branch Is Valid!!");
        });

    /// <summary>
    /// Gets a target to perform a feature pull request status check.
    /// </summary>
    private Target FeaturePRStatusCheck => _ => _
        .Requires(
            () => ThatPullRequestNumberIsProvided(),
            () => ThatThePullRequestExists(),
            () => ThatThePRBranchIsValid(PRBranchContext.Source, BranchType.Feature),
            () => ThatThePRBranchIsValid(PRBranchContext.Target, BranchType.Develop),
            () => ThatThePRIsAssignedToMilestone(),
            () => ThatThePRSourceBranchIssueNumberExists(),
            () => ThatFeaturePRIssueHasLabel(BranchType.Feature),
            () => ThatThePRHasBeenAssigned(),
            () => ThatPRHasLabels());

    /// <summary>
    /// Gets a target to perform a preview feature pull request status check.
    /// </summary>
    private Target PreviewFeaturePRStatusCheck => _ => _
        .Requires(
            () => ThatPullRequestNumberIsProvided(),
            () => ThatThePullRequestExists(),
            () => ThatThePRBranchIsValid(PRBranchContext.Source, BranchType.PreviewFeature),
            () => ThatThePRBranchIsValid(PRBranchContext.Target, BranchType.Preview),
            () => ThatThePRIsAssignedToMilestone(),
            () => ThatThePRSourceBranchIssueNumberExists(),
            () => ThatFeaturePRIssueHasLabel(BranchType.PreviewFeature),
            () => ThatThePRHasBeenAssigned(),
            () => ThatPRHasLabels());

    private Target HotFixPRStatusCheck => _ => _
        .Requires(
            () => ThatPullRequestNumberIsProvided(),
            () => ThatThePullRequestExists(),
            () => ThatThePRBranchIsValid(PRBranchContext.Source, BranchType.HotFix),
            () => ThatThePRBranchIsValid(PRBranchContext.Target, BranchType.Master),
            () => ThatThePRSourceBranchIssueNumberExists(),
            () => ThatThePRHasBeenAssigned(),
            () => ThatThePRIsAssignedToMilestone(),
            () => ThatThePRHasTheLabel("🔥Hot Fix Release🚀", "🐛bug"),
            () => ThatTheProjectVersionsAreValid(ReleaseType.Production),
            () => ThatThePRSourceBranchVersionSectionMatchesProjectVersion(ReleaseType.Production),
            () => ThatTheReleaseTagDoesNotAlreadyExist(ReleaseType.Production),
            () => ThatTheReleaseMilestoneExists(),
            () => ThatTheMilestoneContainsOnlySingleItemOfType(ItemType.Issue),
            () => ThatTheMilestoneContainsOnlySingleItemOfType(ItemType.PullRequest),
            () => ThatAllMilestoneIssuesHaveLabels(),
            () => ThatAllMilestoneItemsAreAssigned(ItemType.Issue),
            () => ThatAllMilestoneItemsAreAssigned(ItemType.PullRequest),
            () => ThatTheReleaseNotesExist(ReleaseType.Production),
            () => ThatTheReleaseNotesTitleIsCorrect(ReleaseType.Production),
            () => ThatMilestoneIssuesExistInReleaseNotes(ReleaseType.Production),
            () => ThatGitHubReleaseDoesNotExist(ReleaseType.Production),
            () => ThatTheNugetPackageDoesNotExist());

    private Target PrevReleasePRStatusCheck => _ => _
        .Requires(
            () => ThatPullRequestNumberIsProvided(),
            () => ThatThePullRequestExists(),
            () => ThatThePRBranchIsValid(PRBranchContext.Source, BranchType.Preview),
            () => ThatThePRBranchIsValid(PRBranchContext.Target, BranchType.Release),
            () => ThatThePRHasBeenAssigned(),
            () => ThatThePRHasTheLabel("🚀Preview Release"),
            () => ThatThePRIsAssignedToMilestone(),
            () => ThatTheProjectVersionsAreValid(ReleaseType.Preview),
            () => ThatThePreviewPRBranchVersionsMatch(ReleaseType.Preview),
            () => ThatThePRSourceBranchVersionSectionMatchesProjectVersion(ReleaseType.Preview),
            () => ThatTheReleaseMilestoneExists(),
            () => ThatTheReleaseMilestoneContainsIssues(),
            () => ThatTheReleaseTagDoesNotAlreadyExist(ReleaseType.Preview),
            () => ThatAllMilestoneIssuesHaveLabels(),
            () => ThatAllOfTheReleaseMilestoneIssuesAreClosed(ReleaseType.Preview, true),
            () => ThatAllOfTheReleaseMilestonePullRequestsAreClosed(ReleaseType.Preview, true),
            () => ThatTheReleaseMilestoneOnlyContainsSingleReleaseItem(ReleaseType.Preview, ItemType.Issue),
            () => ThatTheReleaseMilestoneOnlyContainsSingleReleaseItem(ReleaseType.Preview, ItemType.PullRequest),
            () => ThatTheReleaseNotesExist(ReleaseType.Preview),
            () => ThatTheReleaseNotesTitleIsCorrect(ReleaseType.Preview),
            () => ThatMilestoneIssuesExistInReleaseNotes(ReleaseType.Preview),
            () => ThatGitHubReleaseDoesNotExist(ReleaseType.Preview),
            () => ThatTheNugetPackageDoesNotExist());

    private Target ProdReleasePRStatusCheck => _ => _
        .Requires(
            () => ThatPullRequestNumberIsProvided(),
            () => ThatThePullRequestExists(),
            () => ThatThePRBranchIsValid(PRBranchContext.Source, BranchType.Release),
            () => ThatThePRBranchIsValid(PRBranchContext.Target, BranchType.Master),
            () => ThatThePRHasBeenAssigned(),
            () => ThatThePRHasTheLabel("🚀Production Release"),
            () => ThatThePRIsAssignedToMilestone(),
            () => ThatTheProjectVersionsAreValid(ReleaseType.Production),
            () => ThatThePRSourceBranchVersionSectionMatchesProjectVersion(ReleaseType.Production),
            () => ThatTheReleaseMilestoneExists(),
            () => ThatTheReleaseMilestoneContainsIssues(),
            () => ThatTheReleaseTagDoesNotAlreadyExist(ReleaseType.Production),
            () => ThatAllMilestoneIssuesHaveLabels(),
            () => ThatAllOfTheReleaseMilestoneIssuesAreClosed(ReleaseType.Production, true),
            () => ThatAllOfTheReleaseMilestonePullRequestsAreClosed(ReleaseType.Production, true),
            () => ThatTheReleaseMilestoneOnlyContainsSingleReleaseItem(ReleaseType.Production, ItemType.Issue),
            () => ThatTheReleaseMilestoneOnlyContainsSingleReleaseItem(ReleaseType.Production, ItemType.PullRequest),
            () => ThatTheReleaseNotesExist(ReleaseType.Production),
            () => ThatTheReleaseNotesTitleIsCorrect(ReleaseType.Production),
            () => ThatTheProdReleaseNotesContainsPreviewReleaseSection(),
            () => ThatTheProdReleaseNotesContainsPreviewReleaseItems(),
            () => ThatMilestoneIssuesExistInReleaseNotes(ReleaseType.Production),
            () => ThatGitHubReleaseDoesNotExist(ReleaseType.Production),
            () => ThatTheNugetPackageDoesNotExist());

    private Target DebugTask => _ => _
        .Unlisted()
        .Executes(() =>
        {
        });

    // ReSharper restore UnusedMember.Local
}
