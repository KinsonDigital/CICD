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
            () => ThatFeaturePRIssueNumberExists(),
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
            () => ThatPreviewFeaturePRIssueNumberExists(),
            () => ThatFeaturePRIssueHasLabel(BranchType.PreviewFeature),
            () => ThatThePRHasBeenAssigned(),
            () => ThatPRHasLabels());

    private Target HotFixPRStatusCheck => _ => _
        .Requires(
            () => ThatPullRequestNumberIsProvided(),
            () => ThatThePullRequestExists(),
            () => ThatThePRBranchIsValid(PRBranchContext.Source, BranchType.HotFix),
            () => ThatThePRBranchIsValid(PRBranchContext.Target, BranchType.Master),
            () => ThatPreviewFeaturePRIssueNumberExists(),
            () => ThatFeaturePRIssueHasLabel(BranchType.HotFix),
            () => ThatThePRHasBeenAssigned(),
            () => ThatPRHasLabels());

    private Target PrevReleasePRStatusCheck => _ => _
        .Requires(
            () => ThatPullRequestNumberIsProvided(),
            () => ThatThePullRequestExists(),
            () => ThatThePRBranchIsValid(PRBranchContext.Source, BranchType.Preview),
            () => ThatThePRBranchIsValid(PRBranchContext.Target, BranchType.Release),
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
            () => ThatPullRequestNumberIsProvided(),
            () => ThatThePullRequestExists(),
            () => ThatThePRBranchIsValid(PRBranchContext.Source, BranchType.Release),
            () => ThatThePRBranchIsValid(PRBranchContext.Target, BranchType.Master),
            () => ThatThePRHasBeenAssigned(),
            () => ThatThePRHasTheLabel("🚀Production Release"),
            () => ThatTheProjectVersionsAreValid(ReleaseType.Production),
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
}
