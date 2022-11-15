// <copyright file="IReadmeService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Provides README file services.
/// </summary>
public interface IReadmeService
{
    /// <summary>
    /// Executes pre-processing on a project README file.
    /// </summary>
    void RunPreProcessing();
}
