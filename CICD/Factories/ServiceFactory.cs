// <copyright file="ServiceFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using CICDSystem.Guards;
using CICDSystem.Services;

namespace CICDSystem.Factories;

/// <summary>
/// Creates various services.
/// </summary>
internal static class ServiceFactory
{
    private static IGitHubActionsService? actionsService;

    /// <summary>
    /// Creates an <see cref="IGitHubActionsService"/> instance.
    /// </summary>
    /// <param name="pullRequestNumber">The pull request number that might be used.</param>
    /// <param name="repoOwner">The owner of the repository/project.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <returns>The service.</returns>
    /// <remarks>
    ///     The object returned is a singleton and the same reference is returned every time.
    /// </remarks>
    public static IGitHubActionsService CreateGitHubActionsService(
        int pullRequestNumber,
        string repoOwner,
        string repoName)
    {
        if (actionsService is not null)
        {
            return actionsService;
        }

        EnsureThat.StringParamIsNotNullOrEmpty(repoOwner, nameof(repoOwner));
        EnsureThat.StringParamIsNotNullOrEmpty(repoName, nameof(repoName));

        var secretService = App.Container.GetInstance<ISecretService>();
        var executionContextService = App.Container.GetInstance<IExecutionContextService>();
        var gitRepoService = App.Container.GetInstance<IGitRepoService>();
        var clientFactory = App.Container.GetInstance<IHttpClientFactory>();

        actionsService = new GitHubActionsService(
            pullRequestNumber,
            repoOwner,
            repoName,
            secretService,
            executionContextService,
            gitRepoService,
            clientFactory,

        return actionsService;
    }
}
