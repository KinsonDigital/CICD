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
using Octokit.Internal;
using NukeParameter = Nuke.Common.ParameterAttribute;

// TODO: Add editorconfig to build project and tweak until it fits

public partial class CICD : NukeBuild
{
    private const string ProjFileExt = "csproj";
    private const string NugetOrgSource = "https://api.nuget.org/v3/index.json";
    private const string ConsoleTab = "\t       ";

    public static int Main() =>
        Execute<CICD>(x => x.BuildAllProjects, x => x.RunAllUnitTests);

    GitHubActions? GitHubActions => GitHubActions.Instance;
    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository Repo;

    [NukeParameter] static GitHubClient GitHubClient;

    [NukeParameter(List = false)] static readonly Configuration Configuration = GetBuildConfig();

    [NukeParameter] private static string? BuildSettingsDirPath { get; set; }

    [NukeParameter] private static bool SkipTwitterAnnouncement { get; set; }

    [NukeParameter] private string RepoOwner { get; set; }
    [NukeParameter] private string RepoName { get; set; }
    [NukeParameter] private string ProjectName { get; set; }
    [NukeParameter] [Secret] private string NugetOrgApiKey { get; set; } = string.Empty;
    [NukeParameter] [Secret] private string TwitterConsumerApiKey { get; set; } = string.Empty;
    [NukeParameter] [Secret] private string TwitterConsumerApiSecret { get; set; } = string.Empty;
    [NukeParameter] [Secret] private string TwitterAccessToken { get; set; } = string.Empty;
    [NukeParameter] [Secret] private string TwitterAccessTokenSecret { get; set; } = string.Empty;

    static string DocumentationDirName = "Documentation";
    static string ReleaseNotesDirName = "ReleaseNotes";

    static AbsolutePath DocumentationPath => RootDirectory / DocumentationDirName;
    static AbsolutePath ReleaseNotesBaseDirPath => DocumentationPath / ReleaseNotesDirName;
    static AbsolutePath NugetOutputPath => RootDirectory / "Artifacts";
    static AbsolutePath PreviewReleaseNotesDirPath => ReleaseNotesBaseDirPath / "PreviewReleases";
    static AbsolutePath ProductionReleaseNotesDirPath => ReleaseNotesBaseDirPath / "ProductionReleases";

    static Configuration GetBuildConfig()
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

    static GitHubClient GetGitHubClient()
    {
        var token = GetGitHubToken();
        GitHubClient client;

        if (IsServerBuild)
        {
            client = new GitHubClient(new ProductHeaderValue(RepoName),
                new InMemoryCredentialStore(new Credentials(token)));
        }
        else
        {
            if (string.IsNullOrEmpty(token))
            {
                var warning = "No token has been loaded from the local 'local-secrets.json' file.";
                warning += $"{Environment.NewLine}GitHub API requests will be unauthorized and you may run into API request limits.";
                Console.WriteLine();
                LogWarning(warning);
                client = new GitHubClient(new ProductHeaderValue(RepoName));
            }
            else
            {
                client = new GitHubClient(new ProductHeaderValue(RepoName),
                    new InMemoryCredentialStore(new Credentials(token)));
            }
        }

        return client;
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
