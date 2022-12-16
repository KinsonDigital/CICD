// <copyright file="FindDirService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using CICDSystem.Guards;
using CICDSystem.Services.Interfaces;

// ReSharper disable InconsistentNaming
namespace CICDSystem.Services;

/// <inheritdoc/>
public class FindDirService : IFindDirService
{
    private readonly IDirectory directory;
    private readonly IPath path;

    /// <summary>
    /// Initializes a new instance of the <see cref="FindDirService"/> class.
    /// </summary>
    /// <param name="directory">Manages directories.</param>
    /// <param name="path">Manages paths.</param>
    public FindDirService(
        IDirectory directory,
        IPath path)
    {
        EnsureThat.ParamIsNotNull(directory, nameof(directory));
        EnsureThat.ParamIsNotNull(path, nameof(path));

        this.directory = directory;
        this.path = path;
    }

    /// <inheritdoc/>
    public string FindDescendentDir(string startPath, string dirNameToFind)
    {
        if (string.IsNullOrEmpty(startPath))
        {
            return string.Empty;
        }

        var dirsToSearch = new List<string>();
        var siblingDirs = this.directory.GetDirectories(startPath);

        dirsToSearch.AddRange(siblingDirs);

        var foundPath = string.Empty;

        while (string.IsNullOrEmpty(foundPath))
        {
            foundPath = dirsToSearch.FirstOrDefault(d => d.EndsWith(dirNameToFind));

            if (string.IsNullOrEmpty(foundPath) is false)
            {
                break;
            }

            // Remove the directory from the end
            startPath = this.path.GetDirectoryName(startPath) ?? string.Empty;

            // If the string is null or empty, every descendant has been checked.
            if (string.IsNullOrEmpty(startPath))
            {
                break;
            }

            var moreDirs = this.directory.GetDirectories(startPath);

            dirsToSearch.Clear();
            dirsToSearch.AddRange(moreDirs);
        }

        // TODO: Use cross plat extension method here
        return foundPath is null ? string.Empty : foundPath.Replace('\\', '/');
    }
}
