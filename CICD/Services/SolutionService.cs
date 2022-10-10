// <copyright file="SolutionService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using CICDSystem.Guards;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities;

namespace CICDSystem.Services;

/// <inheritdoc/>
internal sealed class SolutionService : ISolutionService
{
    private readonly ISolutionWrapper solutionWrapper;
    private readonly IFile file;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolutionService"/> class.
    /// </summary>
    /// <param name="solutionWrapper">
    ///     Wraps the <see cref="Nuke.Common"/>.
    ///     <see cref="Nuke.Common.ProjectModel"/>.
    ///     <see cref="Nuke.Common.ProjectModel.Solution"/> functionality.
    /// </param>
    /// <param name="file">Manages files.</param>
    public SolutionService(ISolutionWrapper solutionWrapper, IFile file)
    {
        EnsureThat.ParamIsNotNull(solutionWrapper, nameof(solutionWrapper));
        EnsureThat.ParamIsNotNull(file, nameof(file));

        this.solutionWrapper = solutionWrapper;
        this.file = file;
    }

    /// <inheritdoc/>
    public IReadOnlyCollection<Project> AllProjects => this.solutionWrapper.AllProjects;

    /// <inheritdoc/>
    public Project? GetProject(string nameOrFullPath) => this.solutionWrapper.GetProject(nameOrFullPath);

    /// <inheritdoc/>
    public IEnumerable<Project> GetProjects(string wildcardPattern) => this.solutionWrapper.GetProjects(wildcardPattern);

    /// <inheritdoc/>
    // TODO: Add exceptions
    public string BuildReleaseNotesFilePath(ReleaseType releaseType, string version)
    {
        const string relativeDir = "Documentation/ReleaseNotes";

        var notesDirPath = releaseType switch
        {
            ReleaseType.Preview => $"{this.solutionWrapper.Directory}/{relativeDir}/PreviewReleases",
            ReleaseType.Production => $"{this.solutionWrapper.Directory}/{relativeDir}/ProductionReleases",
            ReleaseType.HotFix => $"{this.solutionWrapper.Directory}/{relativeDir}/ProductionReleases",
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, "The enumeration is out of range.")
        };

        version = version.StartsWith("v")
            ? version.TrimStart("v")
            : version;

        var fileName = $"Release-Notes-v{version}.md";

        return $"{notesDirPath}/{fileName}";
    }

    /// <inheritdoc/>
    // TODO: Add exceptions
    public string GetReleaseNotes(ReleaseType releaseType, string version)
    {
        var fullFilePath = BuildReleaseNotesFilePath(releaseType, version);

        if (this.file.Exists(fullFilePath))
        {
            return this.file.ReadAllText(fullFilePath);
        }

        version = version.StartsWith("v")
            ? version.TrimStart("v")
            : version;

        var exceptionMsg = $"The {releaseType.ToString().ToLower()} release notes for version 'v{version}' could not be found.";

        throw new FileNotFoundException(exceptionMsg, fullFilePath);
    }

    /// <inheritdoc/>
    // TODO: Add exceptions
    public IEnumerable<string> GetReleaseNotesAsLines(ReleaseType releaseType, string version)
    {
        var fullFilePath = BuildReleaseNotesFilePath(releaseType, version);

        if (this.file.Exists(fullFilePath))
        {
            return this.file.ReadAllLines(fullFilePath);
        }

        version = version.StartsWith("v")
            ? version.TrimStart("v")
            : version;

        var exceptionMsg = $"The {releaseType.ToString().ToLower()} release notes for version 'v{version}' could not be found.";

        throw new FileNotFoundException(exceptionMsg, fullFilePath);
    }
}
