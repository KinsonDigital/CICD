// <copyright file="CICD.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Services;

namespace CICDSystem;

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

    /// <summary>
    /// The main entry point of the build system.
    /// </summary>
    /// <returns>An <c>integer</c> value representing an error code or 0 for no errors.</returns>
    public static int Main() =>
        Execute<CICD>(x => x.BuildAllProjects, x => x.RunAllUnitTests);

    GitHubActions? GitHubActions => GitHubActions.Instance;

    [Solution]
    readonly Solution Solution;

    [GitRepository]
    readonly GitRepository Repo;

    [NukeParameter]
    static GitHubClient GitHubClient;

    [NukeParameter(List = false)]
    static readonly Configuration Configuration = GetBuildConfig();

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

    static string DocumentationDirName = "Documentation";
    static string ReleaseNotesDirName = "ReleaseNotes";

    static AbsolutePath DocumentationPath => RootDirectory / DocumentationDirName;
    static AbsolutePath ReleaseNotesBaseDirPath => DocumentationPath / ReleaseNotesDirName;
    static AbsolutePath NugetOutputPath => RootDirectory / "Artifacts";
    static AbsolutePath PreviewReleaseNotesDirPath => ReleaseNotesBaseDirPath / "PreviewReleases";
    static AbsolutePath ProductionReleaseNotesDirPath => ReleaseNotesBaseDirPath / "ProductionReleases";

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
