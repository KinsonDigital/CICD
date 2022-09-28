// <copyright file="CICD.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming

using CICDSystem.Factories;
using CICDSystem.Services;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Octokit;
using NukeParameter = Nuke.Common.ParameterAttribute;

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

    [NukeParameter]
    private static string? BuildSettingsDirPath { get; set; }

    [NukeParameter]
    private static bool SkipTwitterAnnouncement { get; set; }

    [NukeParameter]
    private string RepoOwner { get; set; } = string.Empty;

    [NukeParameter]
    private string RepoName { get; set; } = string.Empty;

    [NukeParameter]
    private string ProjectName { get; set; } = string.Empty;

    [NukeParameter]
    private int PullRequestNumber { get; set; }

    [NukeParameter]
    private string PreviewReleaseNotesDirName { get; set; } = "PreviewReleases";

    [NukeParameter]
    private string ProductionReleaseNotesDirName { get; set; } = "ProductionReleases";

    [NukeParameter]
    private AbsolutePath ReleaseNotesBaseDirPath { get; set; } = RootDirectory / "Documentation" / "ReleaseNotes";

    [NukeParameter]
    [Secret]
    private string NugetOrgApiKey { get; set; } = string.Empty;

    [NukeParameter]
    [Secret]
    private string TwitterConsumerApiKey { get; set; } = string.Empty;

    [NukeParameter]
    [Secret]
    private string TwitterConsumerApiSecret { get; set; } = string.Empty;

    [NukeParameter]
    [Secret]
    private string TwitterAccessToken { get; set; } = string.Empty;

    [NukeParameter]
    [Secret]
    private string TwitterAccessTokenSecret { get; set; } = string.Empty;

    private IGitHubClient GitHubClient => App.Container.GetInstance<IGitHubClientService>().GetClient(RepoName);

    private static AbsolutePath NugetOutputPath => RootDirectory / "Artifacts";

    private AbsolutePath PreviewReleaseNotesDirPath => ReleaseNotesBaseDirPath / PreviewReleaseNotesDirName;

    private AbsolutePath ProductionReleaseNotesDirPath => ReleaseNotesBaseDirPath / ProductionReleaseNotesDirName;

    private Configuration GetBuildConfig()
    {
        var repo = GitRepository.FromLocalDirectory(RootDirectory);

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
