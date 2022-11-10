// <copyright file="ProjectService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using System.Diagnostics.CodeAnalysis;
using CICDSystem.Guards;
using CICDSystem.Reactables.Core;
using CICDSystem.Services.Interfaces;

namespace CICDSystem.Services;

/// <inheritdoc/>
internal sealed class ProjectService : IProjectService
{
    private readonly ISolutionWrapper solutionWrapper;
    private readonly IDisposable unsubscriber;
    private string projectName = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectService"/> class.
    /// </summary>
    /// <param name="solutionWrapper">
    ///     Wraps the <see cref="Nuke.Common"/>.
    ///     <see cref="Nuke.Common.ProjectModel"/>.
    ///     <see cref="Nuke.Common.ProjectModel.Solution"/> functionality.
    /// </param>
    /// <param name="repoInfoReactable">Provides push notifications about repository information.</param>
    public ProjectService(ISolutionWrapper solutionWrapper, IReactable<(string, string)> repoInfoReactable)
    {
        EnsureThat.ParamIsNotNull(solutionWrapper, nameof(solutionWrapper));
        EnsureThat.ParamIsNotNull(repoInfoReactable, nameof(repoInfoReactable));

        this.solutionWrapper = solutionWrapper;

        this.unsubscriber = repoInfoReactable.Subscribe(new Reactor<(string repoOwner, string repoName)>(
            onNext: repoInfoData =>
            {
                this.projectName = repoInfoData.repoName;
            }, () => this.unsubscriber?.Dispose()));
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public string GetVersion()
    {
        var project = this.solutionWrapper.GetProject(this.projectName);

        if (project is null)
        {
            throw new Exception($"The project '{this.projectName}' could not be found.");
        }

        var version = project.GetVersion();

        version = version.StartsWith("v")
            ? version
            : $"v{version}";

        return version;
    }
}
