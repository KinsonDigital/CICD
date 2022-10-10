// <copyright file="SolutionWrapper.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CICDSystem.Guards;
using CICDSystem.Reactables.Core;
using Nuke.Common.ProjectModel;

namespace CICDSystem;

/// <inheritdoc/>
internal sealed class SolutionWrapper : ISolutionWrapper
{
    private readonly IDisposable? unsubscriber;
    private Solution? solution;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolutionWrapper"/> class.
    /// </summary>
    /// <param name="solutionReactable">Provides push notifications about the build solution.</param>
    public SolutionWrapper(IReactable<Solution> solutionReactable)
    {
        EnsureThat.ParamIsNotNull(solutionReactable, nameof(solutionReactable));

        this.unsubscriber = solutionReactable.Subscribe(new Reactor<Solution>(
            onNext: solutionData => this.solution = solutionData,
            () => this.unsubscriber?.Dispose()));
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public string Directory => this.solution?.Directory?.ToString().Replace('\\', '/').TrimEnd('/') ?? string.Empty;

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public IReadOnlyCollection<Project> AllProjects => this.solution?.AllProjects ?? new ReadOnlyCollection<Project>(Array.Empty<Project>());

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public Project? GetProject(string nameOrFullPath) => this.solution?.GetProject(nameOrFullPath);

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public IEnumerable<Project> GetProjects(string wildcardPattern) => this.solution?.GetProjects(wildcardPattern) ?? Array.Empty<Project>();
}
