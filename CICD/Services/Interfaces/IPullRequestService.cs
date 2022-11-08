// <copyright file="IPullRequestService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Provides information about the current build pull request.
/// </summary>
public interface IPullRequestService
{
    /// <summary>
    /// Gets the source branch of a pull request.
    /// </summary>
    /// <remarks>This is the branch that is being merged into the <see cref="TargetBranch"/>.</remarks>
    string SourceBranch { get; }

    /// <summary>
    /// Gets the target branch of a pull request.
    /// </summary>
    /// <remarks>This is the branch where a pull request <see cref="SourceBranch"/> merges.</remarks>
    string TargetBranch { get; }

    /// <summary>
    /// Gets the pull request number of the current build.
    /// </summary>
    /// <returns>The pull request number.</returns>
    /// <remarks>
    ///     Returns the value -1 if no pull request number exists.
    /// </remarks>
    int PullRequestNumber { get; }
}
