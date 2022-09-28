// <copyright file="App.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
namespace CICDSystem;

using Factories;
using System.IO.Abstractions;
using Services;
using System.Diagnostics.CodeAnalysis;
using SimpleInjector;

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
        IoCContainer.Register<IGitRepoService, GitRepoService>(Lifestyle.Singleton);
        IoCContainer.Register<IExecutionContextService, ExecutionContextService>(Lifestyle.Singleton);
        IoCContainer.Register<IGitHubTokenService, GitHubTokenService>(Lifestyle.Singleton);
        IoCContainer.Register<IHttpClientFactory, HttpClientFactory>(Lifestyle.Singleton);
        IoCContainer.Register<IGitHubClientService, GitHubClientService>(Lifestyle.Singleton);
        IoCContainer.Register<IWorkflowService, WorkflowService>(Lifestyle.Singleton);
        IoCContainer.Register<ISecretService, SecretService>(Lifestyle.Singleton);
        IoCContainer.Register<IJsonService, JsonService>(Lifestyle.Singleton);
        IoCContainer.Register<ICurrentDirService, CurrentDirService>(Lifestyle.Singleton);

        isInitialized = true;
    }
}
