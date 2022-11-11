// <copyright file="IConsoleLoggerService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using Serilog;

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Logs messages of different types to the console.
/// </summary>
internal interface IConsoleLoggerService
{
    /// <summary>
    /// Gets the logger that logs to the console.
    /// </summary>
    public ILogger Logger { get; }
}
