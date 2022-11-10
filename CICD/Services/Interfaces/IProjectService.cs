﻿// <copyright file="IProjectService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Provides ability to get information about a project.
/// </summary>
public interface IProjectService
{
    /// <summary>
    /// Gets the version of a project.
    /// </summary>
    /// <returns>The version set in the project.</returns>
    string GetVersion();
}