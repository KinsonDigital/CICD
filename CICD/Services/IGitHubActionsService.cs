﻿// <copyright file="IGitHubActionsService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services;

/// <summary>
/// Provides GitHub Action related services.
/// </summary>
public interface IGitHubActionsService
{
    /// <summary>
    /// Gets a value indicating whether or not the GitHub actions functionality is available.
    /// </summary>
    bool NotAvailable { get; }

    /// <summary>
    /// Gets a value indicating whether or not the current workflow run is a pull request.
    /// </summary>
    bool IsPullRequest { get; }

    /// <summary>
    /// Gets the GitHub token.
    /// </summary>
    string Token { get; }

    /// <summary>
    /// Gets the the name of the person or app that initiated the workflow.
    /// </summary>
    /// <remarks>
    /// If a local build, the user name currently on the machine that initiated the build.
    /// </remarks>
    string Actor { get; }

    /// <summary>
    /// Gets the branch or tag ref that triggered the workflow.
    /// </summary>
    string? Ref { get; }

    /// <summary>
    /// Gets the branch of the base repository.
    /// </summary>
    string? BaseRef { get; }

    /// <summary>
    /// Gets the branch of the head repository.
    /// </summary>
    string? HeadRef { get; }

    /// <summary>
    /// Gets the unique id of the pull request for the current run.
    /// </summary>
    int? PullRequestNumber { get; }
}
