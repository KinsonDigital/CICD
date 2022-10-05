// <copyright file="IExecutionContextService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services;

/// <summary>
/// Returns values indicating the execution context of the CICD system.
/// </summary>
internal interface IExecutionContextService
{
    /// <summary>
    /// Gets a value indicating whether or not the execution context is on the local machine.
    /// </summary>
    bool IsLocalBuild { get; }

    /// <summary>
    /// Gets a value indicating whether or not the execution context is on the server.
    /// </summary>
    bool IsServerBuild { get; }
}
