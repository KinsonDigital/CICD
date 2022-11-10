// <copyright file="GitRepoWrapper.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using System.IO;
using CICDSystem.Services.Interfaces;
using Nuke.Common.Git;

namespace CICDSystem.Services;

/// <inheritdoc/>
[ExcludeFromCodeCoverage]
internal sealed class GitRepoWrapper : IGitRepoWrapper
{
    private readonly GitRepository gitRepository;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitRepoWrapper"/> class.
    /// </summary>
    public GitRepoWrapper() => this.gitRepository = GitRepository.FromLocalDirectory(Directory.GetCurrentDirectory());

    /// <inheritdoc/>
    public string? Commit => this.gitRepository.Commit;

    /// <inheritdoc/>
    public string? Branch => this.gitRepository.Branch;
}
