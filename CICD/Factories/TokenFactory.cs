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
    private readonly ISecretService secretService;
    private readonly IExecutionContextService executionContextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenFactory"/> class.
    /// </summary>
    /// <param name="secretService">Loads secrets locally.</param>
    /// <param name="executionContextService">Determines the execution context.</param>
    public TokenFactory(
        ISecretService secretService,
        IExecutionContextService executionContextService)
    {
        EnsureThat.ParamIsNotNull(secretService, nameof(secretService));
        EnsureThat.ParamIsNotNull(executionContextService, nameof(executionContextService));

        this.secretService = secretService;
        this.executionContextService = executionContextService;
    }

    /// <inheritdoc/>
    public string GetToken() =>
        this.executionContextService.IsServerBuild
            ? GitHubActions.Instance.Token // NOTE: This code branch is not to be tested
            : this.secretService.LoadSecret("GithubAPIToken");
}
