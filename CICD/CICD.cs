// <copyright file="CICD.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using CICDSystem.Factories;
using CICDSystem.Reactables.Core;
using CICDSystem.Services;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Octokit;

namespace CICDSystem;

/// <summary>
/// Contains all of the base setup and init code for the build process.
/// </summary>
public partial class CICD : NukeBuild
{
    private const string NugetOrgSource = "https://api.nuget.org/v3/index.json";
    private const string ConsoleTab = "\t       ";
    private Solution? solution;
    private string repoOwner = string.Empty;
    private string repoName = string.Empty;
    private string projectName = string.Empty;
    private int pullRequestNumber;

    /// <summary>
    /// The main entry point of the build system.
    /// </summary>
    /// <returns>An <c>integer</c> value representing an error code or 0 for no errors.</returns>
    public static int Main() => Execute<CICD>(x => x.BuildAllProjects, x => x.RunAllUnitTests);

#pragma warning disable SA1201 - A property should not follow a method
    /// <summary>
    /// Gets or sets the solution for the build.
    /// </summary>
    [Solution]
    private Solution? Solution
    {
        get => this.solution;
        set
        {
            this.solution = value;

            var solutionReactable = App.Container.GetInstance<IReactable<Solution>>();

            if (solutionReactable.NotificationsEnded || this.solution is null)
            {
                return;
            }

            solutionReactable.PushNotification(this.solution);
            solutionReactable.EndNotifications();
        }
    }

    private ISolutionService SolutionService => App.Container.GetInstance<ISolutionService>();

    private IExecutionContextService ExecutionContext => App.Container.GetInstance<IExecutionContextService>();

    private IBranchValidatorService BranchValidator => App.Container.GetInstance<IBranchValidatorService>();

    private IPullRequestService PullRequestService => App.Container.GetInstance<IPullRequestService>();

    private Configuration Configuration => GetBuildConfig();

    private IGitRepoService Repo => App.Container.GetInstance<IGitRepoService>();

    [Parameter("The output directory of where the template workflows should be generated.")]
    private string? WorkflowTemplateOutput { get; set; }

    [Parameter("If true, will skip the twitter announcement of a release.")]
    private bool SkipTwitterAnnouncement { get; set; }

    [Parameter("The owner of the GitHub repo.  This can also be the GitHub organization that owns the repository.")]
    private string RepoOwner
    {
        get => this.repoOwner;
        set
        {
            this.repoOwner = value;

            var repoInfoReactable = App.Container.GetInstance<IReactable<(string repoOwner, string repoName)>>();

            var dataIsNotReady = string.IsNullOrEmpty(this.repoOwner) || string.IsNullOrEmpty(this.repoName);

            if (dataIsNotReady || repoInfoReactable.NotificationsEnded)
            {
                return;
            }

            repoInfoReactable.PushNotification((this.repoOwner, this.repoName));
            repoInfoReactable.EndNotifications();
        }
    }

    [Parameter("The name of the GitHub repository.")]
    private string RepoName
    {
        get => this.repoName;
        set
        {
            this.repoName = value;

            var repoInfoReactable = App.Container.GetInstance<IReactable<(string repoOwner, string repoName)>>();

            var dataIsNotReady = string.IsNullOrEmpty(this.repoOwner) || string.IsNullOrEmpty(this.repoName);

            if (dataIsNotReady || repoInfoReactable.NotificationsEnded)
            {
                return;
            }

            repoInfoReactable.PushNotification((this.repoOwner, this.repoName));
            repoInfoReactable.EndNotifications();
        }
    }

    [Parameter("The name of the C# project.")]
    private string ProjectName
    {
        get => this.projectName;
        set
        {
            this.projectName = value;

            var productInfoReactable = App.Container.GetInstance<IReactable<string>>();

            if (productInfoReactable.NotificationsEnded)
            {
                return;
            }

            productInfoReactable.PushNotification(this.projectName);
            productInfoReactable.EndNotifications();
        }
    }

    [Parameter("The unique number/id of the GItHub pull request.  Used for pull request status checks when running locally.")]
    private int PullRequestNumber
    {
        get => this.pullRequestNumber;
        set
        {
            this.pullRequestNumber = value;

            var prNumReactable = App.Container.GetInstance<IReactable<int>>();

            if (prNumReactable.NotificationsEnded)
            {
                return;
            }

            prNumReactable.PushNotification(this.pullRequestNumber);
            prNumReactable.EndNotifications();
        }
    }

    [Parameter($"The name of the preview release notes directory name.  This will be located in the '{nameof(ReleaseNotesBaseDirPath)}'.")]
    private string PreviewReleaseNotesDirName { get; set; } = "PreviewReleases";

    [Parameter($"The name of the production release notes directory name.  This will be located in the '{nameof(ReleaseNotesBaseDirPath)}'.")]
    private string ProductionReleaseNotesDirName { get; set; } = "ProductionReleases";

    [Parameter("The directory path of the location of the preview and production release notes.")]
    private AbsolutePath ReleaseNotesBaseDirPath { get; set; } = RootDirectory / "Documentation" / "ReleaseNotes";

    [Parameter("The API key for releasing NuGet packages to nuget.org.")]
    [Secret]
    private string NugetOrgApiKey { get; set; } = string.Empty;

    [Parameter("The Twitter consumer API key.  Essentially the Twitter username.")]
    [Secret]
    private string TwitterConsumerApiKey { get; set; } = string.Empty;

    [Parameter("The Twitter consumer API secret.  Essentially the Twitter password.")]
    [Secret]
    private string TwitterConsumerApiSecret { get; set; } = string.Empty;

    [Parameter("The Twitter access token.")]
    [Secret]
    private string TwitterAccessToken { get; set; } = string.Empty;

    [Parameter("The Twitter access token secret.")]
    [Secret]
    private string TwitterAccessTokenSecret { get; set; } = string.Empty;

    private IGitHubClient GitHubClient => App.Container.GetInstance<IHttpClientFactory>().CreateGitHubClient();

    private AbsolutePath NugetOutputPath => RootDirectory / "Artifacts";

    private AbsolutePath PreviewReleaseNotesDirPath => ReleaseNotesBaseDirPath / PreviewReleaseNotesDirName;

    private AbsolutePath ProductionReleaseNotesDirPath => ReleaseNotesBaseDirPath / ProductionReleaseNotesDirName;
#pragma warning restore SA1201 - A property should not follow a method

    private Configuration GetBuildConfig()
    {
        if (ExecutionContext.IsLocalBuild)
        {
            return (Repo.Branch ?? string.Empty).IsMasterBranch()
                ? Configuration.Release
                : Configuration.Debug;
        }

        return PullRequestService.TargetBranch.IsMasterBranch()
            ? Configuration.Release
            : Configuration.Debug;
    }
}
