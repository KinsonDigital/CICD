// <copyright file="IGitHubClientService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using Octokit;

namespace CICDSystem.Services;

/// <summary>
/// Creates a GitHub API client for communicating with the GitHub API.
/// </summary>
public interface IGitHubClientService
{
    /// <summary>
    /// Gets the GitHub API HTTP Client.
    /// </summary>
    /// <param name="productName">The name of the product communicating with the GitHub API.</param>
    /// <returns>The GitHub API HTTP client.</returns>
    IGitHubClient GetClient(string productName);
}
