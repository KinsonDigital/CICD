// <copyright file="GitRepoService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System.IO;
using Nuke.Common.Git;

namespace CICDSystem.Services;

/// <inheritdoc/>
internal sealed class GitRepoService : IGitRepoService
{
    private readonly GitRepository gitRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitRepoService"/> class.
    /// </summary>
    public GitRepoService() => this.gitRepository = GitRepository.FromLocalDirectory(Directory.GetCurrentDirectory());

    /// <inheritdoc/>
    public string? Commit => this.gitRepository.Commit;

    /// <inheritdoc/>
    public string? Branch => this.gitRepository.Branch;
}
