// <copyright file="INugetDataService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Threading.Tasks;

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Provides NuGet related data.
/// </summary>
public interface INugetDataService
{
    /// <summary>
    /// Returns a list of NuGet versions that exist in nuget.org for a NuGet package that matches the given <paramref name="packageName"/>.
    /// </summary>
    /// <param name="packageName">The name of the package.</param>
    /// <remarks>
    ///     The param <paramref name="packageName"/> is not case sensitive.  The NuGet API
    ///     requires that it is lowercase and is taken care of for you.
    /// </remarks>
    /// <returns>The asynchronous result of the exiting NuGet package versions.</returns>
    Task<string[]> GetNugetVersions(string packageName);
}
