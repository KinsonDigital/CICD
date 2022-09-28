﻿// <copyright file="GitHubActionsService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using System.Diagnostics.CodeAnalysis;
using CICDSystem.Guards;
using Nuke.Common.CI.GitHubActions;
using Octokit;

namespace CICDSystem.Services;

/// <inheritdoc/>
public class GitHubActionsService : IGitHubActionsService
{
    private const string TokenKey = "GitHubApiToken";
    private readonly ISecretService secretService;
    private readonly IExecutionContextService executionContextService;
    private readonly IGitRepoService repoService;
    private readonly IGitHubClient githubClient;
    private readonly int pullRequestNumber;
    private readonly string repoOwner;
    private readonly string repoName;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubActionsService"/> class.
    /// </summary>
    /// <param name="pullRequestNumber">The pull request number that might be used.</param>
    /// <param name="repoOwner">The owner of the repository/project.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="secretService">Manages local secrets for local builds.</param>
    /// <param name="executionContextService">Provides information about the current build execution.</param>
    /// <param name="repoService">Provides repository related services.</param>
    /// <param name="githubClient">Provides GitHub API communication.</param>
    public GitHubActionsService(
        int pullRequestNumber,
        string repoOwner,
        string repoName,
        ISecretService secretService,
        IExecutionContextService executionContextService,
        IGitRepoService repoService,
        IGitHubClient githubClient)
    {
        // TODO: Unit test this
        EnsureThat.StringParamIsNotNullOrEmpty(repoOwner, nameof(repoOwner));
        EnsureThat.StringParamIsNotNullOrEmpty(repoName, nameof(repoName));
        EnsureThat.ParamIsNotNull(secretService, nameof(secretService));
        EnsureThat.ParamIsNotNull(executionContextService, nameof(executionContextService));
        EnsureThat.ParamIsNotNull(repoService, nameof(repoService));
        EnsureThat.ParamIsNotNull(githubClient, nameof(githubClient));

        this.repoOwner = repoOwner;
        this.repoName = repoName;
        this.pullRequestNumber = pullRequestNumber;
        this.secretService = secretService;
        this.executionContextService = executionContextService;
        this.repoService = repoService;
        this.githubClient = githubClient;
    }

    /// <inheritdoc/>
    /// <remarks>
    ///     This could be true due to the build running locally instead of the server(in GitHub Actions).
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public bool NotAvailable => GitHubActions.Instance is null;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public bool IsPullRequest => this.executionContextService.IsServerBuild && GitHubActions.Instance.IsPullRequest;

    /// <inheritdoc/>
    /// <remarks>
    /// if a local build, this comes from a local secrets file.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public string Token => this.executionContextService.IsServerBuild
        ? GitHubActions.Instance.Token
        : this.secretService.LoadSecret(TokenKey);

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public string Actor => this.executionContextService.IsServerBuild ? GitHubActions.Instance.Actor : Environment.UserName;

    /// <inheritdoc/>
    /// <remarks>
    /// If a local build, this is the current branch checked out during the run.
    /// This would be in the form of <c>refs/heads/&lt;branch-name&gt;</c>.
    /// </remarks>
    /// <example>
    /// Local Build: <c>refs/heads/my-branch</c>.
    /// </example>
    [ExcludeFromCodeCoverage]
    public string? Ref => this.executionContextService.IsServerBuild
        ? GitHubActions.Instance.Ref
        : $"refs/heads/{this.repoService.Branch}";

    /// <inheritdoc/>
    /// <remarks>
    /// This is also the destination branch that the pull request source branch is merging into.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public string? BaseRef
    {
        get
        {
            if (this.executionContextService.IsServerBuild)
            {
                return GitHubActions.Instance.IsPullRequest ? GitHubActions.Instance.BaseRef : null;
            }

            if (this.pullRequestNumber <= 0)
            {
                return null;
            }

            // If this is a local build and the pull request number has been set
            var pullRequest = this.githubClient.PullRequest.Get(this.repoOwner, this.repoName, this.pullRequestNumber).Result;

            var targetBranch = pullRequest.Base.Ref;

            return targetBranch;
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// If a local build, this is the currently checked out branch that the build is running against.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    public string? HeadRef
    {
        get
        {
            if (this.executionContextService.IsServerBuild)
            {
                return GitHubActions.Instance.IsPullRequest ? GitHubActions.Instance.HeadRef : null;
            }

            return this.repoService.Branch;
        }
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public int? PullRequestNumber
    {
        get
        {
            if (this.executionContextService.IsServerBuild)
            {
                return GitHubActions.Instance.PullRequestNumber;
            }

            return this.pullRequestNumber <= 0 ? null : this.pullRequestNumber;
        }
    }
}
