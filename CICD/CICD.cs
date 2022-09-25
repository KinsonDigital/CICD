// <copyright file="CICD.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
namespace CICDSystem;

using Services;
using System;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Octokit;
using NukeParameter = Nuke.Common.ParameterAttribute;

/// <summary>
/// Contains all of the base setup and init code for the build process.
/// </summary>
public partial class CICD : NukeBuild
{
    private const string NugetOrgSource = "https://api.nuget.org/v3/index.json";
    private const string ConsoleTab = "\t       ";
    [NukeParameter(List = false)]
    private static readonly Configuration Configuration = GetBuildConfig();
    [Solution]
    private readonly Solution? solution;
    [GitRepository]
    private readonly GitRepository repo;

    /// <summary>
    /// The main entry point of the build system.
    /// </summary>
    /// <returns>An <c>integer</c> value representing an error code or 0 for no errors.</returns>
    public static int Main() =>
        Execute<CICD>(x => x.BuildAllProjects, x => x.RunAllUnitTests);

    private GitHubActions? GitHubActions => GitHubActions.Instance;

    [NukeParameter]
    private static GitHubClient GitHubClient;

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

    private static AbsolutePath NugetOutputPath => RootDirectory / "Artifacts";

    private AbsolutePath PreviewReleaseNotesDirPath => ReleaseNotesBaseDirPath / PreviewReleaseNotesDirName;

    private AbsolutePath ProductionReleaseNotesDirPath => ReleaseNotesBaseDirPath / ProductionReleaseNotesDirName;

    private static Configuration GetBuildConfig()
    {
        var repo = GitRepository.FromLocalDirectory(RootDirectory);

        if (IsLocalBuild || GitHubActions.Instance is null)
        {
            return (repo.Branch ?? string.Empty).IsMasterBranch()
                ? Configuration.Release
                : Configuration.Debug;
        }

        return (GitHubActions.Instance?.BaseRef  ?? string.Empty).IsMasterBranch()
            ? Configuration.Release
            : Configuration.Debug;
    }

    static string GetGitHubToken()
    {
        if (IsServerBuild)
        {
            return GitHubActions.Instance.Token;
        }

        var localSecretService = new LoadSecretsService();

        const string tokenName = "GitHubApiToken";

        return localSecretService.LoadSecret(tokenName);
    }

    static void LogWarning(string warning)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠️{warning}");
        Console.ForegroundColor = color;
    }

    private static void LogError(string error)
    {
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error);
        Console.ForegroundColor = color;
    }
}
