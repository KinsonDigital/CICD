// <copyright file="CICD.Release.Preview.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;
using CICDSystem.Services.Interfaces;
using Nuke.Common;
using Serilog;

namespace CICDSystem;

/// <summary>
/// Contains all of the functionality for preview releases.
/// </summary>
public partial class CICD // Release.Preview
{
    // ReSharper disable UnusedMember.Global

    /// <summary>
    /// Gets a target the performs a preview release.
    /// </summary>
    public Target PreviewRelease => _ => _
        .Requires(
            () => ThatTheCurrentBranchIsCorrect(BranchType.Release),
            () => ThatTheProjectVersionsAreValid(ReleaseType.Preview),
            () => ThatTheCurrentBranchVersionMatchesProjectVersion(BranchType.Release),
            () => ThatTheReleaseTagDoesNotAlreadyExist(ReleaseType.Preview),
            () => ThatTheReleaseMilestoneExists(),
            () => ThatTheReleaseMilestoneContainsIssues(),
            () => ThatAllMilestoneIssuesHaveLabels(),
            () => ThatAllMilestonePullRequestsHaveLabels(),
            () => ThatAllMilestoneItemsAreAssigned(ItemType.Issue),
            () => ThatAllMilestoneItemsAreAssigned(ItemType.PullRequest),
            () => ThatAllOfTheReleaseMilestoneIssuesAreClosed(ReleaseType.Preview, false),
            () => ThatAllOfTheReleaseMilestonePullRequestsAreClosed(ReleaseType.Preview, false),
            () => ThatTheReleaseNotesExist(ReleaseType.Preview),
            () => ThatTheReleaseNotesTitleIsCorrect(ReleaseType.Preview),
            () => ThatMilestoneIssuesExistInReleaseNotes(ReleaseType.Preview),
            () => ThatGitHubReleaseDoesNotExist(ReleaseType.Preview),
            () => ThatTheNugetPackageDoesNotExist()).After(BuildAllProjects, RunAllUnitTests)
        .DependsOn(BuildAllProjects, RunAllUnitTests)
        .Executes(async () =>
        {
            if (ReleaseChecksOnly)
            {
                var warningMsg = "Preview Release Skipped.";
                warningMsg += $"{Environment.NewLine}This is due to the `{{Value1}}` run being set to only execute the release requirement checks.";
                warningMsg += $"{Environment.NewLine}This means that all of the requirement checks have been executed but the release has not.";
                warningMsg += $"{Environment.NewLine}If you want to perform a release, do not use the `{{Value2}}` switch in the workflow,";
                warningMsg += $"{Environment.NewLine}and/or remove or set the '{{Value3}}' setting in the '{{Value4}}' file to a value of '{{Value5}}'.";

                Log.Warning(warningMsg, "PreviewRelease", "--release-checks-only", "ReleaseChecksOnly", "parameters.json", "false");

                return;
            }

            var projectService = App.Container.GetInstance<IProjectService>();
            var version = projectService.GetVersion();

            version = version.StartsWith('v') ? version : $"v{version}";

            if (string.IsNullOrEmpty(version))
            {
                Assert.Fail("Release failed.  Could not get version information.");
            }

            Log.Information($"🚀 Starting preview release process for version '{version}' 🚀");

            try
            {
                // Create a GitHub release
                var releaseUrl = await CreateNewGitHubRelease(ReleaseType.Preview, version);
                var githubReleaseLog = $"The GitHub preview release for version '{version}' was successful!!🚀";
                githubReleaseLog += $"{Environment.NewLine}{ConsoleTab}To view the release, go here 👉🏼 {releaseUrl}{Environment.NewLine}";
                Log.Information(githubReleaseLog);

                // Close the milestone
                Log.Information($"✅Closing GitHub milestone '{version}' . . .");
                var milestoneClient = GitHubClient.Issue.Milestone;
                var milestoneResult = await milestoneClient.CloseMilestone(RepoOwner, RepoName, version);
                var milestoneMsg = $"The GitHub milestone '{version}' has been closed.";
                milestoneMsg +=
                    $"{Environment.NewLine}{ConsoleTab}To view the milestone, go here 👉🏼 {milestoneResult.HtmlUrl}{Environment.NewLine}";
                Log.Information(milestoneMsg);

                // Update the milestone description
                Log.Information($"✅Updating description for milestone '{version}' . . .");
                var description = await CreateMilestoneDescription(version, ReleaseType.Preview);
                var updatedMilestone = await milestoneClient.UpdateMilestoneDescription(RepoOwner, RepoName, version, description);
                var updateMsg = $"The GitHub Milestone '{version}' description has been updated.";
                updateMsg +=
                    $"{Environment.NewLine}{ConsoleTab}To view the milestone, go here 👉🏼 {updatedMilestone.HtmlUrl}{Environment.NewLine}";
                Log.Information(updateMsg);

                // If README pre-processing should be performed, process README before packaging
                if (PreProcessReadMe)
                {
                    var readmeService = App.Container.GetInstance<IReadmeService>();

                    readmeService.RunPreProcessing();
                }

                // Create the NuGet package to deploy
                var fileName = $"{RepoName}.{version.TrimStart('v')}.nupkg";
                var nugetPath = $"{NugetOutputPath}/{fileName}"
                    .Replace(RootDirectory, "./")
                    .Replace(@"\", "/");
                Log.Information("✅Creating a NuGet package . . .");
                CreateNugetPackage();
                Log.Information($"Nuget package created at location '{nugetPath}'{Environment.NewLine}");

                // Publish NuGet package to nuget.org
                Log.Information("✅Publishing NuGet package to nuget.org . . .");
                var nugetUrl = $"https://www.nuget.org/packages/{RepoOwner}.{RepoName}/{version.TrimStart('v')}";
                PublishNugetPackage();
                var nugetReleaseLog = "Nuget package published!!🚀";
                nugetReleaseLog += $"To view the NuGet package, go here 👉🏼 {nugetUrl}";
                Log.Information(nugetReleaseLog);

                var releaseTweetService = App.Container.GetInstance<IReleaseTweetService>();

                Log.Information("✅Announcing release on Twitter . . .");
                releaseTweetService.SendReleaseTweet();
                Log.Information($"Twitter announcement complete!!{Environment.NewLine}");
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
            }
        });

    // ReSharper restore UnusedMember.Global
}
