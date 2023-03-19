// <copyright file="IReadMeService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Provides README file services.
/// </summary>
internal interface IReadMeService
{
    /// <summary>
    /// Executes pre-processing on a project README file.
    /// </summary>
    void RunPreProcessing();
}
