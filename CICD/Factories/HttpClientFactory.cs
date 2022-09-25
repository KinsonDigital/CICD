// <copyright file="HttpClientFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using CICDSystem.Guards;
using Octokit;
using Octokit.Internal;

// ReSharper disable InconsistentNaming
namespace CICDSystem.Factories;

/// <inheritdoc/>
[ExcludeFromCodeCoverage]
public sealed class HttpClientFactory : IHttpClientFactory
{
    private static IGitHubClient? client;
    private bool isDisposed;

    /// <inheritdoc/>
    public IGitHubClient CreateGitHubClient(string productName, string token)
    {
        if (client is not null)
        {
            return client;
        }

        EnsureThat.StringParamIsNotNullOrEmpty(productName, nameof(productName));
        EnsureThat.StringParamIsNotNullOrEmpty(token, nameof(token));

        var productHeaderValue = new ProductHeaderValue(productName);
        var credStore = new InMemoryCredentialStore(new Credentials(token));

        client = new GitHubClient(productHeaderValue, credStore);

        return client;
    }
}
