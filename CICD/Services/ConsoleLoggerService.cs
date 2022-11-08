// <copyright file="ConsoleLoggerService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using CICDSystem.Services.Interfaces;
using Serilog;

namespace CICDSystem.Services;

/// <inheritdoc/>
[ExcludeFromCodeCoverage]
internal sealed class ConsoleLoggerService : IConsoleLoggerService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleLoggerService"/> class.
    /// </summary>
    public ConsoleLoggerService() =>
        Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

    /// <inheritdoc/>
    public ILogger Logger { get; }
}
