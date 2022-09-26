// <copyright file="IExecutionContextService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services;

/// <summary>
/// Returns values indicating the execution context of the CICD system.
/// </summary>
public interface IExecutionContextService
{
    /// <summary>
    /// Gets a value indicating whether or not the execution context is on the local machine.
    /// </summary>
    /// <remarks>This is most likely locally on the development machine.</remarks>
    bool IsLocalBuild { get; }

    /// <summary>
    /// Gets a value indicating whether or not the execution context is on the server.
    /// </summary>
    /// <remarks>This is on some kind of CICD server such as GitHub actions.</remarks>
    bool IsServerBuild { get; }
}
