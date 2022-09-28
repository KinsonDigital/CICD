// <copyright file="IGitRepoService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services;

/// <summary>
/// Provides GIT repository related services.
/// </summary>
internal interface IGitRepoService
{
    /// <summary>
    /// Gets the current commit.
    /// </summary>
    string? Commit { get; }

    /// <summary>
    /// Gets the current branch. <c>null</c> if the head is detached.
    /// </summary>
    string? Branch { get; }
}
