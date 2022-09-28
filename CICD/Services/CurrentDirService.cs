// <copyright file="CurrentDirService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;

namespace CICDSystem.Services;

/// <inheritdoc/>
[ExcludeFromCodeCoverage]
internal sealed class CurrentDirService : ICurrentDirService
{
    private readonly IPath path;

    /// <summary>
    /// Initializes a new instance of the <see cref="CurrentDirService"/> class.
    /// </summary>
    /// <param name="path">Manages paths.</param>
    public CurrentDirService(IPath path) => this.path = path;

    /// <inheritdoc/>
    public string GetCurrentDirectory() => $"{this.path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}";
}
