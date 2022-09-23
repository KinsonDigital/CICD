// <copyright file="IWorkflowService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services;

using System;
using System.IO;

/// <summary>
/// Generates workflow files for the targets in the Nuke build system.
/// </summary>
public interface IWorkflowService
{
    /// <summary>
    /// Generates workflows for all of the nuke targets at the given <paramref name="destinationDir"/>.
    /// </summary>
    /// <param name="destinationDir">The directory location to generate the workflow files at.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="destinationDir"/> is null or argument.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    ///     Thrown if the workflow template file directory location does not exist.
    /// </exception>
    void GenerateWorkflows(string destinationDir);
}
