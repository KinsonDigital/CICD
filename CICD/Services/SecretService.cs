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
    private readonly IFile file;
    private readonly IJsonService jsonService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretService"/> class.
    /// </summary>
    /// <param name="directory">Manages directories.</param>
    /// <param name="file">Manages files.</param>
    /// <param name="path">Manages paths.</param>
    /// <param name="findDirService">Searches for directories.</param>
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
        IFindDirService findDirService,
        IJsonService jsonService)
    {
        EnsureThat.ParamIsNotNull(directory, nameof(directory));
        EnsureThat.ParamIsNotNull(file, nameof(file));
        EnsureThat.ParamIsNotNull(path, nameof(path));
        EnsureThat.ParamIsNotNull(findDirService, nameof(findDirService));
        EnsureThat.ParamIsNotNull(jsonService, nameof(jsonService));

        this.file = file;
        this.jsonService = jsonService;

        var startPath = directory.GetCurrentDirectory().Replace('\\', '/').TrimEnd('/');

        this.rootRepoDirPath = findDirService.FindDescendentDir(startPath, GitHubDirName);

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
}
