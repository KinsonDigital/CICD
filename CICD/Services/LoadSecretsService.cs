// <copyright file="LoadSecretsService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
namespace CICDSystem.Services;

using System.IO.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

/// <inheritdoc/>
public class LoadSecretsService : ILoadSecretsService
{
    private const string SecretFileName = "local-secrets.json";
    private readonly string executionPath;
    private readonly string rootRepoDirPath;
    private readonly IDirectory directory;
    private readonly IFile file;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadSecretsService"/> class.
    /// </summary>
    /// <param name="directory">Manages directories.</param>
    /// <param name="file">Manages files.</param>
    /// <param name="path">Manages paths.</param>
    /// <exception cref="Exception"></exception>
    // TODO: Add exceptions for the null param checks
    // TODO: Add an exception doc if the root of the repository does not exist.
    public LoadSecretsService(IDirectory directory, IFile file, IPath path)
    {
        // TODO: Create unit tests to check null for all params.  Create Ensure guard pattern

        this.directory = directory;
        this.file = file;

        this.executionPath = @$"{path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";
        this.executionPath = this.executionPath.Replace('\\', '/').TrimEnd('/');
        this.rootRepoDirPath = GetRepoRootDirPath().TrimEnd('/');

        if (string.IsNullOrEmpty(this.rootRepoDirPath))
        {
            throw new Exception("The root repository directory path could not be found.  Could not load local secrets.");
        }

        var secretFilePath = $"{this.rootRepoDirPath}/.github/{SecretFileName}";

        if (this.file.Exists(secretFilePath))
        {
            return;
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        var emptyData = new KeyValuePair<string, string>[] { new (string.Empty, string.Empty) };
        var emptyJsonData = JsonSerializer.Serialize(emptyData, options);

        this.file.WriteAllText(secretFilePath, emptyJsonData);
    }

    /// <inheritdoc/>
    public string LoadSecret(string secretName)
    {
        var secretFilePath = $"{this.rootRepoDirPath}/.github/{SecretFileName}";
        var jsonData = this.file.ReadAllText(secretFilePath);

        var secrets = JsonSerializer.Deserialize<KeyValuePair<string, string>[]>(jsonData);

        var foundSecret = (from s in secrets
            where s.Key == secretName
            select s.Value).FirstOrDefault();

        return foundSecret ?? string.Empty;
    }

    /// <summary>
    /// Gets the directory path of the root of the repository.
    /// </summary>
    /// <returns>The directory path to the .github directory.</returns>
    private string GetRepoRootDirPath()
    {
        var pathSections = this.executionPath.Split('/').ToList();

        bool IsRoot(string[] pathSections)
        {
            var pathToCheck = string.Join('/', pathSections);

            var directories = this.directory.GetDirectories(pathToCheck);

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
