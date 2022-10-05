// <copyright file="ServiceFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using CICDSystem.Reactables.Core;
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
    /// <returns>The service.</returns>
    /// <remarks>
    ///     The object returned is a singleton and the same reference is returned every time.
    /// </remarks>
    public static IGitHubActionsService CreateGitHubActionsService(int pullRequestNumber)
    {
        if (actionsService is not null)
        {
            return actionsService;
        }

        var secretService = App.Container.GetInstance<ISecretService>();
        var executionContextService = App.Container.GetInstance<IExecutionContextService>();
        var gitRepoService = App.Container.GetInstance<IGitRepoService>();
        var clientFactory = App.Container.GetInstance<IHttpClientFactory>();
        var repoInfoReactable = App.Container.GetInstance<IReactable<(string, string)>>();

        actionsService = new GitHubActionsService(
            pullRequestNumber,
            secretService,
            executionContextService,
            gitRepoService,
            clientFactory,
            repoInfoReactable);

        return actionsService;
    }
}
