// <copyright file="HttpClientFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;
using System.Diagnostics.CodeAnalysis;
using CICDSystem.Guards;
using CICDSystem.Reactables.Core;
using CICDSystem.Services.Interfaces;
using Octokit;
using Octokit.Internal;

// ReSharper disable InconsistentNaming
namespace CICDSystem.Factories;

/// <inheritdoc/>
[ExcludeFromCodeCoverage]
internal sealed class HttpClientFactory : IHttpClientFactory
{
    private readonly IDisposable unsubscriber;
    private readonly IGitHubTokenService tokenService;
    private IGitHubClient? client;
    private string productName = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientFactory"/> class.
    /// </summary>
    /// <param name="productNamReactable">Provides push notifications about the pull request number.</param>
    /// <param name="tokenService">Provides access to tokens.</param>
    /// <exception cref="ArgumentNullException">
    ///     Occurs if any of the arguments are <c>null</c>.
    /// </exception>
    public HttpClientFactory(IReactable<string> productNamReactable, IGitHubTokenService tokenService)
    {
        EnsureThat.ParamIsNotNull(productNamReactable, nameof(productNamReactable));
        EnsureThat.ParamIsNotNull(tokenService, nameof(tokenService));

        this.tokenService = tokenService;

        this.unsubscriber = productNamReactable.Subscribe(new Reactor<string>(
            onNext: data =>
            {
                this.productName = data;
            },
            onCompleted: () => this.unsubscriber?.Dispose()));
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">
    /// Thrown for the following reasons:
    ///     <list type="bullet">
    ///         <item>The internal product name is null or empty.</item>
    ///         <item>The token is null or empty.</item>
    ///     </list>
    /// </exception>
    public IGitHubClient CreateGitHubClient()
    {
        if (this.client is not null)
        {
            return this.client;
        }

        if (string.IsNullOrEmpty(this.productName))
        {
            throw new InvalidOperationException("The internal product name cannot be null or empty.");
        }

        var token = this.tokenService.GetToken();

        if (string.IsNullOrEmpty(token))
        {
            throw new InvalidOperationException("The token must not be null or empty.");
        }

        var productHeaderValue = new ProductHeaderValue(this.productName);
        var credStore = new InMemoryCredentialStore(new Credentials(token));

        this.client = new GitHubClient(productHeaderValue, credStore);

        return this.client;
    }
}
