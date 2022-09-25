// <copyright file="TokenFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Guards;
using CICDSystem.Services;
using Nuke.Common.CI.GitHubActions;

// ReSharper disable InconsistentNaming
namespace CICDSystem.Factories;

/// <inheritdoc/>
public class TokenFactory : ITokenFactory
{
    private readonly ILoadSecretsService secretsService;
    private readonly IExecutionContextService executionContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenFactory"/> class.
    /// </summary>
    /// <param name="secretsService">Loads secrets locally.</param>
    /// <param name="executionContextService">Determines the execution context.</param>
    public TokenFactory(
        ILoadSecretsService secretsService,
        IExecutionContextService executionContextService)
    {
        EnsureThat.ParamIsNotNull(secretsService, nameof(secretsService));
        EnsureThat.ParamIsNotNull(executionContextService, nameof(executionContextService));

        this.secretsService = secretsService;
        this.executionContextService = executionContextService;
    }

    /// <inheritdoc/>
    public string GetToken() =>
        this.executionContextService.IsServerBuild
            ? GitHubActions.Instance.Token // NOTE: This branch is not to be tested
            : this.secretsService.LoadSecret("GithubAPIToken");
}
