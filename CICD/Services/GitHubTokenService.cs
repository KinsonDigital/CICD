// <copyright file="GitHubTokenService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Guards;
using Nuke.Common.CI.GitHubActions;

// ReSharper disable InconsistentNaming
namespace CICDSystem.Services;

/// <inheritdoc/>
internal sealed class GitHubTokenService : IGitHubTokenService
{
    private const string TokenKey = "GitHubApiToken";
    private readonly ISecretService secretService;
    private readonly IExecutionContextService executionContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubTokenService"/> class.
    /// </summary>
    /// <param name="secretService">Loads secrets locally.</param>
    /// <param name="executionContextService">Determines the execution context.</param>
    public GitHubTokenService(
        ISecretService secretService,
        IExecutionContextService executionContextService)
    {
        EnsureThat.ParamIsNotNull(secretService, nameof(secretService));
        EnsureThat.ParamIsNotNull(executionContextService, nameof(executionContextService));

        this.secretService = secretService;
        this.executionContextService = executionContextService;
    }

    /// <inheritdoc/>
    /// <remarks>
    /// If a local build, this is pulled from a local secrets file.
    /// </remarks>
    public string GetToken() =>
        this.executionContextService.IsServerBuild
            ? GitHubActions.Instance.Token // NOTE: This code branch is not to be tested
            : this.secretService.LoadSecret(TokenKey);
}
