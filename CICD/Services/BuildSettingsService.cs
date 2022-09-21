using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace CICD.Services;

public class BuildSettingsService
{
    private const string BuildSettingsFileName = "build-settings.json";
    private string ExecutionPath = @$"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/";
    private string rootRepoDirPath = string.Empty;
    private readonly string settingsFilePath;

    public BuildSettingsService()
    {
        ExecutionPath = ExecutionPath.Replace('\\', '/').TrimEnd('/');
        this.rootRepoDirPath = GetRepoRootDirPath().TrimEnd('/');

        if (string.IsNullOrEmpty(this.rootRepoDirPath))
        {
            throw new Exception("The root repository directory path could not be found.  Could not load local secrets.");
        }

        this.settingsFilePath = $"{this.rootRepoDirPath}/.github/{BuildSettingsFileName}";
    }

    public (bool loadSuccessful, BuildSettings? settings, string errorMsg) LoadBuildSettings()
    {
        if (File.Exists(this.settingsFilePath) is false)
        {
            return (false, null, $"The build settings at path '{this.settingsFilePath}' does not exist.");
        }

        var jsonData = File.ReadAllText(this.settingsFilePath);
        var buildSettings = JsonSerializer.Deserialize<BuildSettings>(jsonData);

        return buildSettings is null
            ? (false, null, "Could not load the build settings file.")
            : (true, buildSettings, string.Empty);
    }

    public void CreateDefaultBuildSettingsFile()
    {
        if (File.Exists(this.settingsFilePath))
        {
            return;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        var emptyData = new BuildSettings()
        {
            Owner = string.Empty,
            MainProjectName = string.Empty,
            MainProjectFileName = null,
            DocumentationDirName = null,
            ReleaseNotesDirName = null,
        };

        var emptyJsonData = JsonSerializer.Serialize(emptyData, options);

        File.WriteAllText(this.settingsFilePath, emptyJsonData);
    }

    private string GetRepoRootDirPath()
    {
        var pathSections = ExecutionPath.Split('/').ToList();

        bool IsRoot(string[] pathSections)
        {
            var pathToCheck = string.Join('/', pathSections);

            var directories = Directory.GetDirectories(pathToCheck);

            var containsGitHubDir = directories.Any(d => d.EndsWith(".git"));

            return containsGitHubDir;
        }

        for (var i = pathSections.Count - 1; i > 0; i--)
        {
            var isRoot = IsRoot(pathSections.ToArray());

            if (isRoot)
            {
                return string.Join('/', pathSections);
            }

            pathSections.RemoveAt(i);
        }

        return string.Empty;
    }
}
