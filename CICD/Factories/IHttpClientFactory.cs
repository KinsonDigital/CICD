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
    /// <param name="productName">The name of the product using the client.</param>
    /// <param name="token">The token to make authorized API requests.</param>
    /// <returns>The <see cref="IGitHubClient"/>.</returns>
    IGitHubClient CreateGitHubClient(string productName, string token);
}
