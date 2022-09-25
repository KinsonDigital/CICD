// <copyright file="GitHubClientService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using CICDSystem.Factories;
using CICDSystem.Guards;
using Octokit;

namespace CICDSystem.Services;

/// <inheritdoc/>
public class GitHubClientService : IGitHubClientService
{
    private readonly ITokenFactory tokenFactory;
    private readonly IHttpClientFactory clientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubClientService"/> class.
    /// </summary>
    /// <param name="tokenFactory">Loads the GitHub API token.</param>
    /// <param name="clientFactory">Creates HTTP clients.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the any of the parameters are null.
    /// </exception>
    public GitHubClientService(
        ITokenFactory tokenFactory,
        IHttpClientFactory clientFactory)
    {
        EnsureThat.ParamIsNotNull(tokenFactory, nameof(tokenFactory));
        EnsureThat.ParamIsNotNull(clientFactory, nameof(clientFactory));

        this.tokenFactory = tokenFactory;
        this.clientFactory = clientFactory;
    }

    /// <inheritdoc/>
    /// <remarks>
    ///     The client returned is always a reference to the same internal object
    ///     and is generated from the <see cref="IHttpClientFactory"/>.
    /// </remarks>
    public IGitHubClient GetClient(string productName)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(productName, nameof(productName));

        var token = this.tokenFactory.GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            return this.clientFactory.CreateGitHubClient(productName, token);
        }

        var exMsg = "The token could be loaded.  If running locally, check that a 'local-secrets.json' file exists in ";
        exMsg += "the '.github' folder with the correct 'GitHubApiToken' key value pair.";
        exMsg += $"{Environment.NewLine}If running on the server, verify that the workflow is setting up and environment variable named ";
        exMsg += $"{Environment.NewLine}'GITHUB_TOKEN' with the token value.";

        throw new Exception(exMsg);
    }
}
