// <copyright file="IFindDirService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Provides directory search functionality.
/// </summary>
public interface IFindDirService
{
    /// <summary>
    /// Walks up one directory at a time starting at the given <paramref name="startPath"/>
    /// until a directory with a name that matches the given <paramref name="dirNameToFind"/> is found.
    /// </summary>
    /// <param name="startPath">The directory path to start the search.</param>
    /// <param name="dirNameToFind">The name to the directory to find.</param>
    /// <returns>The directory path that contains the <paramref name="dirNameToFind"/>.</returns>
    string FindDescendentDir(string? startPath, string dirNameToFind);
}
