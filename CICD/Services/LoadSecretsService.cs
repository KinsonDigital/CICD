using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace Services;

public class LoadSecretsService
{
    private const string SecretFileName = "local-secrets.json";
    private string ExecutionPath = @$"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/";
    private string rootRepoDirPath = string.Empty;

    public LoadSecretsService()
    {
        this.ExecutionPath = this.ExecutionPath.Replace('\\', '/').TrimEnd('/');
        this.rootRepoDirPath = GetRepoRootDirPath().TrimEnd('/');

        if (string.IsNullOrEmpty(this.rootRepoDirPath))
        {
            throw new Exception("The root repository directory path could not be found.  Could not load local secrets.");
        }

        var secretFilePath = $"{this.rootRepoDirPath}/.github/{SecretFileName}";

        if (File.Exists(secretFilePath))
        {
            return;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        var emptyData = new KeyValuePair<string, string>[] { new (string.Empty, string.Empty) };
        var emptyJsonData = JsonSerializer.Serialize(emptyData, options);

        File.WriteAllText(secretFilePath, emptyJsonData);
    }

    public string LoadSecret(string secretName)
    {
        var secretFilePath = $"{this.rootRepoDirPath}/.github/{SecretFileName}";
        var jsonData = File.ReadAllText(secretFilePath);

        var secrets = JsonSerializer.Deserialize<KeyValuePair<string, string>[]>(jsonData);

        var foundSecret = (from s in secrets
            where s.Key == secretName
            select s.Value).FirstOrDefault();

        return foundSecret;
    }

    private string GetRepoRootDirPath()
    {
        var pathSections = this.ExecutionPath.Split('/').ToList();

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
