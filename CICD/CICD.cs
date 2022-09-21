using System;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Octokit;
using Octokit.Internal;
using Services;
using NukeParameter = Nuke.Common.ParameterAttribute;

// TODO: Add editorconfig to build project and tweak until it fits

public partial class CICD : NukeBuild
{
    const string ProjFileExt = "csproj";
    const string NugetOrgSource = "https://api.nuget.org/v3/index.json";
    const string ConsoleTab = "\t       ";
    private static BuildSettings buildSettings;

    public static int Main(string[] args)
    {
        // If the generate settings file command was invoked
        if (args.Length > 0 && args[0] == nameof(GenerateSettingsFile))
        {
            return Execute<CICD>(x => x.DebugTask);
        }

        var buildSettingsService = new BuildSettingsService();

        var loadResult = buildSettingsService.LoadBuildSettings();

        if (loadResult.loadSuccessful)
        {
            buildSettings = loadResult.settings ??
                            throw new Exception("The build settings are null.  Build canceled!!");

            Owner = buildSettings.Owner ?? string.Empty;
            MainProjName = buildSettings.MainProjectName ?? string.Empty;

            // Make sure mandatory settings are not null or empty
            if (string.IsNullOrEmpty(Owner) || string.IsNullOrEmpty(MainProjName))
            {
                const string mandatorySettingsErrorMsg = $"The '{nameof(BuildSettings.Owner)}' and '{nameof(BuildSettings.MainProjectName)}' settings must not be null or empty.";
                LogError(mandatorySettingsErrorMsg);
                return -1;
            }

            MainProjFileName = string.IsNullOrEmpty(buildSettings.MainProjectFileName)
                ? MainProjFileName
                : buildSettings.MainProjectFileName;
            DocumentationDirName = string.IsNullOrEmpty(buildSettings.DocumentationDirName)
                ? DocumentationDirName
                : buildSettings.DocumentationDirName;
            ReleaseNotesDirName = string.IsNullOrEmpty(buildSettings.ReleaseNotesDirName)
                ? ReleaseNotesDirName
                : buildSettings.ReleaseNotesDirName;
            AnnounceOnTwitter = buildSettings.AnnounceOnTwitter;

            GitHubClient = GetGitHubClient();
        }
        else
        {
            Console.WriteLine();
            var loadErrorMsg = loadResult.errorMsg;
            loadErrorMsg += $"To create an empty build settings file, run the '{nameof(GenerateSettingsFile)}' command";
            LogError(loadErrorMsg);
            return -1;
        }

        return Execute<CICD>(x => x.BuildAllProjects, x => x.RunAllUnitTests);
    }

    GitHubActions? GitHubActions => GitHubActions.Instance;
    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository Repo;

    [NukeParameter] static GitHubClient GitHubClient;

    [NukeParameter(List = false)] static readonly Configuration Configuration = GetBuildConfig();

    [NukeParameter] [Secret] private string NugetOrgApiKey { get; set; } = string.Empty;
    [NukeParameter] [Secret] private string TwitterConsumerApiKey { get; set; } = string.Empty;
    [NukeParameter] [Secret] private string TwitterConsumerApiSecret { get; set; } = string.Empty;
    [NukeParameter] [Secret] private string TwitterAccessToken { get; set; } = string.Empty;
    [NukeParameter] [Secret] private string TwitterAccessTokenSecret { get; set; } = string.Empty;

    static string Owner = string.Empty;
    static string MainProjName = string.Empty;
    static string MainProjFileName = $"{MainProjName}.{ProjFileExt}";
    static string DocumentationDirName = "Documentation";
    static string ReleaseNotesDirName = "ReleaseNotes";

    static AbsolutePath DocumentationPath => RootDirectory / DocumentationDirName;
    static AbsolutePath ReleaseNotesBaseDirPath => DocumentationPath / ReleaseNotesDirName;
    static AbsolutePath MainProjPath => RootDirectory / MainProjName / MainProjFileName;
    static AbsolutePath NugetOutputPath => RootDirectory / "Artifacts";
    static AbsolutePath PreviewReleaseNotesDirPath => ReleaseNotesBaseDirPath / "PreviewReleases";
    static AbsolutePath ProductionReleaseNotesDirPath => ReleaseNotesBaseDirPath / "ProductionReleases";

    static bool AnnounceOnTwitter
    {
        get => buildSettings.AnnounceOnTwitter;
        set => buildSettings.AnnounceOnTwitter = value;
    }

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
            client = new GitHubClient(new ProductHeaderValue(MainProjName),
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
                client = new GitHubClient(new ProductHeaderValue(MainProjName));
            }
            else
            {
                client = new GitHubClient(new ProductHeaderValue(MainProjName),
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
