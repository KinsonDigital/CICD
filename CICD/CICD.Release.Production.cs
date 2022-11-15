// <copyright file="CICD.Release.Production.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Services.Interfaces;

namespace CICDSystem;

using System;
using Nuke.Common;
using Serilog;

/// <summary>
/// Contains all of the functionality for production releases.
/// </summary>
public partial class CICD // Release.Production
{
    // ReSharper disable UnusedMember.Global

    /// <summary>
    /// Gets a target that performs a production release.
    /// </summary>
    public Target ProductionRelease => _ => _
        .Requires(
            () => ThatTheCurrentBranchIsCorrect(BranchType.Master),
            () => ThatTheProjectVersionsAreValid(ReleaseType.Production),
            () => ThatTheReleaseTagDoesNotAlreadyExist(ReleaseType.Production),
            () => ThatTheReleaseMilestoneExists(),
            () => ThatTheReleaseMilestoneContainsIssues(),
            () => ThatAllMilestoneIssuesHaveLabels(),
            () => ThatAllMilestonePullRequestsHaveLabels(),
            () => ThatAllMilestoneItemsAreAssigned(ItemType.Issue),
            () => ThatAllMilestoneItemsAreAssigned(ItemType.PullRequest),
            () => ThatAllOfTheReleaseMilestoneIssuesAreClosed(ReleaseType.Production, false),
            () => ThatAllOfTheReleaseMilestonePullRequestsAreClosed(ReleaseType.Production, false),
            () => ThatTheReleaseNotesExist(ReleaseType.Production),
            () => ThatTheReleaseNotesTitleIsCorrect(ReleaseType.Production),
            () => ThatTheProdReleaseNotesContainsPreviewReleaseSection(),
            () => ThatTheProdReleaseNotesContainsPreviewReleaseItems(),
            () => ThatMilestoneIssuesExistInReleaseNotes(ReleaseType.Production),
            () => ThatGitHubReleaseDoesNotExist(ReleaseType.Production),
            () => ThatTheNugetPackageDoesNotExist()).After(BuildAllProjects, RunAllUnitTests)
        .DependsOn(BuildAllProjects, RunAllUnitTests)
        .Executes(async () =>
        {
            var projectService = App.Container.GetInstance<IProjectService>();
            var version = projectService.GetVersion();

            if (string.IsNullOrEmpty(version))
            {
                Assert.Fail("Release failed.  Could not get version information.");
            }

            Log.Information($"🚀 Starting production release process for version '{version}' 🚀");

            try
            {
                // Create a GitHub release
                Log.Information("✅Creating new GitHub release . . .");
                var releaseUrl = await CreateNewGitHubRelease(ReleaseType.Production, version);
                var githubReleaseLog = $"The GitHub production release for version '{version}' was successful!!🚀";
                githubReleaseLog += $"{Environment.NewLine}{ConsoleTab}To view the release, go here 👉🏼 {releaseUrl}{Environment.NewLine}";
                Log.Information(githubReleaseLog);

                // Close the milestone
                Log.Information($"✅Closing GitHub milestone '{version}' . . .");
                var milestoneClient = GitHubClient.Issue.Milestone;
                var milestoneResult = await milestoneClient.CloseMilestone(RepoOwner, RepoName, version);
                var milestoneMsg = $"The GitHub milestone '{version}' as been closed.";
                milestoneMsg += $"{Environment.NewLine}{ConsoleTab}To view the milestone, go here 👉🏼 {milestoneResult.HtmlUrl}{Environment.NewLine}";
                Log.Information(milestoneMsg);

                // Update the milestone description
                Log.Information($"✅Updating description for milestone '{version}' . . .");
                var description = await CreateMilestoneDescription(version, ReleaseType.Production);
                var updatedMilestone = await milestoneClient.UpdateMilestoneDescription(RepoOwner, RepoName, version, description);
                var updateMsg = $"The GitHub Milestone '{version}' description has been updated.";
                updateMsg += $"{Environment.NewLine}{ConsoleTab}To view the milestone, go here 👉🏼 {updatedMilestone.HtmlUrl}{Environment.NewLine}";
                Log.Information(updateMsg);

                // If README pre-processing should be performed, process readme before packaging
                if (PreProcessReadMe)
                {
                    var readmeService = App.Container.GetInstance<IReadmeService>();

                    readmeService.RunPreProcessing();
                }

                // Create the nuget package to deploy
                var fileName = $"{RepoName}.{version.TrimStart('v')}.nupkg";
                var nugetPath = $"{NugetOutputPath}/{fileName}"
                    .Replace(RootDirectory, "./")
                    .Replace(@"\", "/");
                Log.Information("✅Creating a nuget package . . .");
                CreateNugetPackage();
                Log.Information($"Nuget package created at location '{nugetPath}'{Environment.NewLine}");

                // Publish nuget package to nuget.org
                Log.Information("✅Publishing nuget package to nuget.org . . .");
                var nugetUrl = $"https://www.nuget.org/packages/{RepoOwner}.{RepoName}/{version.TrimStart('v')}";
                PublishNugetPackage();
                var nugetReleaseLog = "Nuget package published!!🚀";
                nugetReleaseLog += $"To view the nuget package, go here 👉🏼 {nugetUrl}";
                Log.Information(nugetReleaseLog);

                // Tweet about release
                var releaseTweetService = App.Container.GetInstance<IReleaseTweetService>();

                Log.Information("✅Announcing release on Twitter . . .");
                releaseTweetService.SendReleaseTweet();
                Log.Information($"Twitter announcement complete!!{Environment.NewLine}");

                // Merge the master branch into the develop branch
                Log.Information("✅Merging 'master' branch into the 'develop' branch . . .");
                var mergeResultUrl = await MergeBranch("master", "develop");
                string mergeLog;

                // If the merge result URL is null or empty, something went wrong like a merge conflict
                if (string.IsNullOrEmpty(mergeResultUrl))
                {
                    mergeLog = "Something went wrong merging the 'master' branch into the 'develop' branch.";
                    mergeLog += $"{Environment.NewLine}{ConsoleTab}There most likely was a merge conflict.";
                    mergeLog += $"{Environment.NewLine}{ConsoleTab}Manually resolve the merge conflict and merge the 'master' branch into the 'develop' branch.";
                    Log.Warning(mergeLog);
                }
                else
                {
                    mergeLog = $"The 'master' branch has been merged into the 'develop' branch.";
                    mergeLog += $"{Environment.NewLine}{ConsoleTab}To view the merge result, go here 👉🏼 {mergeResultUrl}";
                    Log.Information(mergeLog);
                }
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        });

    // ReSharper restore UnusedMember.Global
}
