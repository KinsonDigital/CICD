// <copyright file="NugetPackageResponse.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace CICDSystem.Models;

using System;

/// <summary>
/// Holds NuGet package response data.
/// </summary>
public class NugetPackageResponse
{
    /// <summary>
    /// Gets the list of NuGet packages of a NuGet API request.
    /// </summary>
    public NugetPackageModel[] Data { get; init; } = Array.Empty<NugetPackageModel>();
}
