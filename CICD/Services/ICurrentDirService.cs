// <copyright file="ICurrentDirService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services;

/// <summary>
/// Gets the current execution directory/
/// </summary>
public interface ICurrentDirService
{
    /// <summary>
    /// Returns the directory that the application is currently being executed in.
    /// </summary>
    /// <returns>The current directory.</returns>
    string GetCurrentDirectory();
}
