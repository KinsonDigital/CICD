// <copyright file="NugetDataService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

/* Resources:
 * NuGet API documentation: https://learn.microsoft.com/en-us/nuget/api/search-query-service-resource
 */

// ReSharper disable InconsistentNaming
namespace CICDSystem.Services;

using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Guards;
using Models;
using Interfaces;
using Flurl.Http;

/// <inheritdoc />
public sealed class NugetDataService : INugetDataService
{
    private const string BaseUrl = "https://azuresearch-usnc.nuget.org";

    /// <inheritdoc />
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="packageName"/> param is null or empty.
    /// </exception>
    /// <exception cref="HttpRequestException">
    ///     Thrown if any HTTP based error occurs.
    /// </exception>
    public async Task<string[]> GetNugetVersions(string packageName)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(packageName, nameof(packageName));

        var query = $"query?q=packageId:{packageName}&take=1000&prerelease=true&semVerLevel=2.0.0";
        var fullUrl = $"{BaseUrl}/{query}";

        var nugetPackageData = await fullUrl.GetJsonAsync<NugetPackageResponse>();

        if (nugetPackageData is null)
        {
            throw new HttpRequestException("There was an issue getting data from NuGet.");
        }

        return nugetPackageData.Data.Length <= 0
            ? Array.Empty<string>()
            : nugetPackageData.Data[0].Versions.Select(v => v.Version).ToArray();
    }
}
