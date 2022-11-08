// <copyright file="ISolutionService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Nuke.Common.ProjectModel;

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Provides functionality for a <c>C#</c> solution.
/// </summary>
internal interface ISolutionService
{
    /// <summary>
    /// Gets the list of all the projects contained in a solution.
    /// </summary>
    IReadOnlyCollection<Project> AllProjects { get; }

    /// <summary>
    /// Gets a project that matches the given project name or fully qualified path.
    /// </summary>
    /// <param name="nameOrFullPath">The name of the project or the fully qualified project file path to the <c>csproj</c> file.</param>
    /// <returns>A solution project.</returns>
    Project? GetProject(string nameOrFullPath);

    /// <summary>
    /// Returns a list of projects that match the given <paramref name="wildcardPattern"/>.
    /// </summary>
    /// <param name="wildcardPattern">The pattern to use to find solution projects.</param>
    /// <returns>The list of projects.</returns>
    IEnumerable<Project> GetProjects(string wildcardPattern);

    /// <summary>
    /// Builds the full file path to the release notes based on the given <paramref name="releaseType"/>
    /// and <paramref name="version"/>.
    /// </summary>
    /// <param name="releaseType">The type of release.</param>
    /// <param name="version">The version of the release.</param>
    /// <returns>The fully qualified file path to the release notes.</returns>
    string BuildReleaseNotesFilePath(ReleaseType releaseType, string version);

    /// <summary>
    /// Gets the release notes content based on the given <paramref name="releaseType"/> and <paramref name="version"/>.
    /// </summary>
    /// <param name="releaseType">The type of release.</param>
    /// <param name="version">The version of the release.</param>
    /// <returns>The contents of a release notes file.</returns>
    string GetReleaseNotes(ReleaseType releaseType, string version);

    /// <summary>
    /// Gets the release notes content as a list of lines based on the given <paramref name="releaseType"/> and <paramref name="version"/>.
    /// </summary>
    /// <param name="releaseType">The type of release.</param>
    /// <param name="version">The version of the release.</param>
    /// <returns>The list of content lines of a release notes file.</returns>
    IEnumerable<string> GetReleaseNotesAsLines(ReleaseType releaseType, string version);
}
