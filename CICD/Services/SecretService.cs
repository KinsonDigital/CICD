// <copyright file="SecretService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using CICDSystem.Guards;
using CICDSystem.Services.Interfaces;

namespace CICDSystem.Services;

/// <inheritdoc/>
internal sealed class SecretService : ISecretService
{
    private const string GitHubDirName = ".github";
    private const string SecretFileName = "local-secrets.json";
    private readonly string rootRepoDirPath;
    private readonly IDirectory directory;
    private readonly IFile file;
    private readonly IPath path;
    private readonly IJsonService jsonService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretService"/> class.
    /// </summary>
    /// <param name="directory">Manages directories.</param>
    /// <param name="file">Manages files.</param>
    /// <param name="path">Manages paths.</param>
    /// <param name="jsonService">Serializes and deserializes JSON data.</param>
    /// <exception cref="ArgumentNullException">
    /// Occurs when the following parameters are null:
    /// <list type="bullet">
    ///     <item><paramref name="directory"/></item>
    ///     <item><paramref name="file"/></item>
    ///     <item><paramref name="path"/></item>
    ///     <item><paramref name="jsonService"/></item>
    /// </list>
    /// </exception>
    public SecretService(
        IDirectory directory,
        IFile file,
        IPath path,
        IJsonService jsonService)
    {
        EnsureThat.ParamIsNotNull(directory, nameof(directory));
        EnsureThat.ParamIsNotNull(file, nameof(file));
        EnsureThat.ParamIsNotNull(path, nameof(path));
        EnsureThat.ParamIsNotNull(jsonService, nameof(jsonService));

        this.directory = directory;
        this.file = file;
        this.path = path;
        this.jsonService = jsonService;

        var startPath = directory.GetCurrentDirectory().Replace('\\', '/').TrimEnd('/');

        this.rootRepoDirPath = FindDescendentDir(startPath, GitHubDirName);

        if (string.IsNullOrEmpty(this.rootRepoDirPath))
        {
            throw new Exception("The root repository directory path could not be found.  Could not load local secrets.");
        }

        var secretFilePath = $"{this.rootRepoDirPath}/{SecretFileName}";

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

        var secretFilePath = $"{this.rootRepoDirPath}/{SecretFileName}";

        if (this.file.Exists(secretFilePath) is false)
        {
            return string.Empty;
        }

        var jsonData = this.file.ReadAllText(secretFilePath);

        var secrets = this.jsonService.Deserialize<KeyValuePair<string, string>[]>(jsonData);

        var foundSecret = (from s in secrets
            where s.Key == secretName
            select s.Value).FirstOrDefault();

        return foundSecret ?? string.Empty;
    }

    private string FindDescendentDir(string? startPath, string dirNameToFind)
    {
        if (string.IsNullOrEmpty(startPath))
        {
            return string.Empty;
        }

        var dirsToSearch = new List<string>();
        var siblingDirs = this.directory.GetDirectories(startPath);

        dirsToSearch.AddRange(siblingDirs);

        var foundPath = string.Empty;

        while (string.IsNullOrEmpty(foundPath))
        {
            foundPath = dirsToSearch.FirstOrDefault(d => d.EndsWith(dirNameToFind));

            if (string.IsNullOrEmpty(foundPath) is false)
            {
                break;
            }

            // Remove the directory from the end
            startPath = this.path.GetDirectoryName(startPath);

            // If the string is null or empty, every descendant has been checked.
            if (string.IsNullOrEmpty(startPath))
            {
                break;
            }

            var moreDirs = this.directory.GetDirectories(startPath);

            dirsToSearch.Clear();
            dirsToSearch.AddRange(moreDirs);
        }

        return foundPath is null ? string.Empty : foundPath.Replace('\\', '/');
    }
}
