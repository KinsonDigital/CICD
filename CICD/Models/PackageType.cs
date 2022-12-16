// <copyright file="PackageType.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable NotAccessedPositionalProperty.Global
namespace CICDSystem.Models;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Holds the type of package.
/// </summary>
/// <param name="Name">The type of package.</param>
[SuppressMessage(
    "StyleCop.CSharp.NamingRules",
    "SA1313:Parameter names should begin with lower-case letter",
    Justification = "The parameters are actually properties and need to maintain property naming conventions.")]
public record PackageType(string Name);
