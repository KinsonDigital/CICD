// <copyright file="NugetPackageModel.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable NotAccessedPositionalProperty.Global
namespace CICDSystem.Models;

using System.Diagnostics.CodeAnalysis;

/// <summary>
/// Holds information about a NuGet package.
/// </summary>
/// <param name="Registration"></param>
/// <param name="Id">Gets the ID of the package.</param>
/// <param name="Version">Gets the full SemVer 2.0.0 version string of the package (could contain build metadata).</param>
/// <param name="Description">Gets the absolute URL to the associated registration index.</param>
/// <param name="Summary">Gets the summary of the package.</param>
/// <param name="Title">Gets the title of the package.</param>
/// <param name="IconUrl">Gets the URL to the icon.</param>
/// <param name="LicenseUrl">Gets the URL to the license.</param>
/// <param name="ProjectUrl">Gets the URL to the project.</param>
/// <param name="Tags">Gets the tags.</param>
/// <param name="Authors">Gets the authors.</param>
/// <param name="Owners">Gets the owners.</param>
/// <param name="TotalDownloads">Gets this value can be inferred by the sum of downloads in the versions array.</param>
/// <param name="Verified">Gets a value indicating whether or not the package is verified.</param>
/// <param name="PackageTypes">Gets the package types defined by the package author (added in SearchQueryService/3.5.0).</param>
/// <param name="Versions">Gets all of the versions of the package.</param>
[SuppressMessage(
    "StyleCop.CSharp.NamingRules",
    "SA1313:Parameter names should begin with lower-case letter",
    Justification = "The parameters are actually properties and need to maintain property naming conventions.")]
public record NugetPackageModel(
    string Registration,
    string Id,
    string Version,
    string Description,
    string Summary,
    string Title,
    string IconUrl,
    string LicenseUrl,
    string ProjectUrl,
    string[] Tags,
    string[] Authors,
    string[] Owners,
    int TotalDownloads,
    bool Verified,
    PackageType[] PackageTypes,
    Versions[] Versions);
