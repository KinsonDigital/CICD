// <copyright file="IHttpClientFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using Octokit;

namespace CICDSystem.Factories;

/// <summary>
/// Creates new HTTP clients.
/// </summary>
internal interface IHttpClientFactory
{
    /// <summary>
    /// Creates a new instance of the <see cref="IGitHubClient"/> to communicate with the GitHub API.
    /// </summary>
    /// <returns>An <see cref="IGitHubClient"/>.</returns>
    IGitHubClient CreateGitHubClient();
}
