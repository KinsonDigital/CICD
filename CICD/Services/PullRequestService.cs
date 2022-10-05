// <copyright file="PullRequestService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using CICDSystem.Factories;
using CICDSystem.Guards;
using CICDSystem.Reactables.Core;
using Octokit;

namespace CICDSystem.Services;

/// <inheritdoc/>
internal sealed class PullRequestService : IPullRequestService
{
    private readonly IDisposable repoInfoUnsubscriber;
    private readonly IDisposable prNumberUnsubscriber;
    private readonly IHttpClientFactory clientFactory;
    private string repoOwner = string.Empty;
    private string repoName = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="PullRequestService"/> class.
    /// </summary>
    /// <param name="repoInfoReactable">Provides push notifications of build information.</param>
    /// <param name="prNumberReactable">Provides push notifications of the pull request number.</param>
    /// <param name="clientFactory">Creates HTTP clients.</param>
    public PullRequestService(
        IReactable<(string, string)> repoInfoReactable,
        IReactable<int> prNumberReactable,
        IHttpClientFactory clientFactory)
    {
        EnsureThat.ParamIsNotNull(repoInfoReactable, nameof(repoInfoReactable));
        EnsureThat.ParamIsNotNull(prNumberReactable, nameof(prNumberReactable));
        EnsureThat.ParamIsNotNull(clientFactory, nameof(clientFactory));

        this.repoInfoUnsubscriber = repoInfoReactable.Subscribe(new Reactor<(string repoOwner, string repoName)>(
            onNext: data =>
            {
                this.repoOwner = data.repoOwner;
                this.repoName = data.repoName;
            },
            onCompleted: () => this.repoInfoUnsubscriber?.Dispose()));

        this.prNumberUnsubscriber = prNumberReactable.Subscribe(new Reactor<int>(
            onNext: data =>
            {
                PullRequestNumber = data;
            },
            onCompleted: () => this.prNumberUnsubscriber?.Dispose()));

        this.clientFactory = clientFactory;
    }

    /// <inheritdoc/>
    /// <exception cref="NotFoundException">Thrown if the pull request does not exist.</exception>
    public string SourceBranch
    {
        get
        {
            var prClient = this.clientFactory.CreateGitHubClient().PullRequest;

            var pullRequest = prClient.Get(this.repoOwner, this.repoName, PullRequestNumber).Result;

            return pullRequest.Head.Ref;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="NotFoundException">Thrown if the pull request does not exist.</exception>
    public string TargetBranch
    {
        get
        {
            var prClient = this.clientFactory.CreateGitHubClient().PullRequest;

            var pullRequest = prClient.Get(this.repoOwner, this.repoName, PullRequestNumber).Result;

            return pullRequest.Base.Ref;
        }
    }

    /// <inheritdoc/>
    public int PullRequestNumber { get; private set; }
}
