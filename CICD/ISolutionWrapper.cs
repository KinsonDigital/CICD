// <copyright file="ISolutionWrapper.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Collections.Generic;
using Nuke.Common.ProjectModel;

namespace CICDSystem;

/// <summary>
/// A thin wrapper around the <see cref="Nuke.Common"/>.
/// <see cref="Nuke.Common.ProjectModel"/>.<see cref="Nuke.Common.ProjectModel.Solution"/> object.
/// </summary>
internal interface ISolutionWrapper
{
    /// <summary>
    /// Gets the root directory of the solution.
    /// </summary>
    string Directory { get; }

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
}
