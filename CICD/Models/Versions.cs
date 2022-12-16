// <copyright file="Versions.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable NotAccessedPositionalProperty.Global
namespace CICDSystem.Models;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Holds version information about a NuGet package.
/// </summary>
/// <param name="Version">Gets the version of the package.</param>
/// <param name="Downloads">Gets the number of downloads of the NuGet package version.</param>
[SuppressMessage(
    "StyleCop.CSharp.NamingRules",
    "SA1313:Parameter names should begin with lower-case letter",
    Justification = "The parameters are actually properties and need to maintain property naming conventions.")]
public record Versions(string Version, int Downloads);
