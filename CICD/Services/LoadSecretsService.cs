// <copyright file="LoadSecretsService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
namespace CICDSystem.Services;

using Guards;
using System.IO.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

/// <inheritdoc/>
public class LoadSecretsService : ILoadSecretsService
{
    private const string SecretFileName = "local-secrets.json";
    private readonly string executionPath;
    private readonly string rootRepoDirPath;
    private readonly IDirectory directory;
    private readonly IFile file;
    private readonly IJsonService jsonService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadSecretsService"/> class.
    /// </summary>
    /// <param name="directory">Manages directories.</param>
    /// <param name="file">Manages files.</param>
    /// <param name="path">Manages paths.</param>
    /// <param name="jsonService">Serializes and deserializes JSON data.</param>
    /// <param name="currentDirService">Gets the current execution directory.</param>
    /// <exception cref="ArgumentNullException">
    /// Occurs when the following parameters are null:
    /// <list type="bullet">
    ///     <item><paramref name="directory"/></item>
    ///     <item><paramref name="file"/></item>
    ///     <item><paramref name="path"/></item>
    ///     <item><paramref name="jsonService"/></item>
    ///     <item><paramref name="currentDirService"/></item>
    /// </list>
    /// </exception>
    public LoadSecretsService(
        IDirectory directory,
        IFile file,
        IPath path,
        IJsonService jsonService,
        ICurrentDirService currentDirService)
    {
        EnsureThat.ParamIsNotNull(directory, nameof(directory));
        EnsureThat.ParamIsNotNull(file, nameof(file));
        EnsureThat.ParamIsNotNull(path, nameof(path));
        EnsureThat.ParamIsNotNull(jsonService, nameof(jsonService));
        EnsureThat.ParamIsNotNull(currentDirService, nameof(currentDirService));

        this.directory = directory;
        this.file = file;
        this.jsonService = jsonService;

        this.executionPath = @$"{path.GetDirectoryName(currentDirService.GetCurrentDirectory())}";
        this.executionPath = this.executionPath.Replace('\\', '/').TrimEnd('/');
        this.rootRepoDirPath = GetRepoRootDirPath().TrimEnd('/');

        if (string.IsNullOrEmpty(this.rootRepoDirPath))
        {
            throw new Exception("The root repository directory path could not be found.  Could not load local secrets.");
        }

        var secretFilePath = $"{this.rootRepoDirPath}/.github/{SecretFileName}".Replace('\\', '/');

        if (this.file.Exists(secretFilePath))
        {
            return;
        }

        var emptyData = new KeyValuePair<string, string>[] { new (string.Empty, string.Empty) };
        var emptyJsonData = this.jsonService.Serialize(emptyData);

        this.file.WriteAllText(secretFilePath, emptyJsonData);
    }

    /// <inheritdoc/>
    public string LoadSecret(string secretName)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(secretName, nameof(secretName));

        var secretFilePath = $"{this.rootRepoDirPath}/.github/{SecretFileName}";
        var jsonData = this.file.ReadAllText(secretFilePath);

        var secrets = this.jsonService.Deserialize<KeyValuePair<string, string>[]>(jsonData);

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
