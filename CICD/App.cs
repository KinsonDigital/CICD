// <copyright file="App.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using CICDSystem.Factories;
using CICDSystem.Reactables;
using CICDSystem.Reactables.Core;
using CICDSystem.Services;
using CICDSystem.Services.Interfaces;
using Nuke.Common.ProjectModel;
using SimpleInjector;

namespace CICDSystem;

/// <summary>
/// Provides dependency injection for the application.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class App
{
    private static readonly FileSystem FileSystem = new ();
    private static readonly Container IoCContainer = new ();
    private static bool isInitialized;

    /// <summary>
    /// Gets the inversion of control container used to get instances of objects.
    /// </summary>
    public static Container Container
    {
        get
        {
            if (!isInitialized)
            {
                SetupContainer();
            }

            return IoCContainer;
        }
    }

    /// <summary>
    /// Sets up the IoC container.
    /// </summary>
    private static void SetupContainer()
    {
        IoCContainer.Register(() => FileSystem.File, Lifestyle.Singleton);
        IoCContainer.Register(() => FileSystem.Directory, Lifestyle.Singleton);
        IoCContainer.Register(() => FileSystem.Path, Lifestyle.Singleton);
        IoCContainer.Register<IReactable<(string, string)>, RepoInfoReactable>(Lifestyle.Singleton);
        IoCContainer.Register<IReactable<string>, ProductNameReactable>(Lifestyle.Singleton);
        IoCContainer.Register<IReactable<int>, PRNumberReactable>(Lifestyle.Singleton);
        IoCContainer.Register<IReactable<Solution>, SolutionReactable>(Lifestyle.Singleton);
        IoCContainer.Register<ISolutionWrapper, SolutionWrapper>(Lifestyle.Singleton);
        IoCContainer.Register<ISolutionService, SolutionService>(Lifestyle.Singleton);
        IoCContainer.Register<IGitRepoService, GitRepoService>(Lifestyle.Singleton);
        IoCContainer.Register<IPullRequestService, PullRequestService>(Lifestyle.Singleton);
        IoCContainer.Register<IExecutionContextService, ExecutionContextService>(Lifestyle.Singleton);
        IoCContainer.Register<IGitHubTokenService, GitHubTokenService>(Lifestyle.Singleton);
        IoCContainer.Register<IHttpClientFactory, HttpClientFactory>(Lifestyle.Singleton);
        IoCContainer.Register<IWorkflowService, WorkflowService>(Lifestyle.Singleton);
        IoCContainer.Register<ISecretService, SecretService>(Lifestyle.Singleton);
        IoCContainer.Register<IJsonService, JsonService>(Lifestyle.Singleton);
        IoCContainer.Register<IBranchValidatorService, BranchValidatorService>(Lifestyle.Singleton);

        isInitialized = true;
    }
}
