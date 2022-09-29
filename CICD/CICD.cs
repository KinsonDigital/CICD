// <copyright file="CICD.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using CICDSystem.Factories;
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
    [Solution]
    private readonly Solution? solution;

    /// <summary>
    /// The main entry point of the build system.
    /// </summary>
    /// <returns>An <c>integer</c> value representing an error code or 0 for no errors.</returns>
    public static int Main() =>
        Execute<CICD>(x => x.BuildAllProjects, x => x.RunAllUnitTests);

    private IGitHubActionsService GitHubActionsService =>
        ServiceFactory.CreateGitHubActionsService(PullRequestNumber, RepoOwner, RepoName);

    private IExecutionContextService ExecutionContext => App.Container.GetInstance<IExecutionContextService>();

    private Configuration Configuration => GetBuildConfig();

    private IGitRepoService repo => App.Container.GetInstance<IGitRepoService>();

    [Parameter("The output directory of where the template workflows should be generated.")]
    private string? WorkflowTemplateOutput { get; set; }

    [Parameter("If true, will skip the twitter announcement of a release.")]
    private bool SkipTwitterAnnouncement { get; set; }

    [Parameter("The owner of the GitHub repo.  This can also be the GitHub organization that owns the repository.")]
    private string RepoOwner { get; set; } = string.Empty;

    [Parameter("The name of the GitHub repository.")]
    private string RepoName { get; set; } = string.Empty;

    [Parameter("The name of the C# project.")]
    private string ProjectName { get; set; } = string.Empty;

    [Parameter("The unique number/id of the GItHub pull request.  Used for pull request status checks when running locally.")]
    private int PullRequestNumber { get; set; }

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

    private IGitHubClient GitHubClient => App.Container.GetInstance<IGitHubClientService>().GetClient(RepoName);

    private static AbsolutePath NugetOutputPath => RootDirectory / "Artifacts";

    private AbsolutePath PreviewReleaseNotesDirPath => ReleaseNotesBaseDirPath / PreviewReleaseNotesDirName;

    private AbsolutePath ProductionReleaseNotesDirPath => ReleaseNotesBaseDirPath / ProductionReleaseNotesDirName;

    private Configuration GetBuildConfig()
    {
        if (ExecutionContext.IsLocalBuild)
        {
            return (repo.Branch ?? string.Empty).IsMasterBranch()
                ? Configuration.Release
                : Configuration.Debug;
        }

        return (GitHubActionsService.BaseRef ?? string.Empty).IsMasterBranch()
            ? Configuration.Release
            : Configuration.Debug;
    }
}
