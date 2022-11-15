// <copyright file="NugetDataService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CICDSystem.Models;
using RestSharp;

namespace CICDSystem.Services;

/// <inheritdoc />
[ExcludeFromCodeCoverage]
public sealed class NugetDataService : IDisposable
{
    /* Resources:
     * These links refer to the documentation for the NuGet API
     * 1. Package Content: https://docs.microsoft.com/en-us/nuget/api/package-base-address-resource
     * 2.Nuget Server API: https://docs.microsoft.com/en-us/nuget/api/overview
     */
    private const string BaseUrl = "https://api.nuget.org";
    private readonly RestClient client;
    private bool isDisposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="NugetDataService"/> class.
    /// </summary>
    public NugetDataService() => this.client = new RestClient(BaseUrl);

    /// <summary>
    /// Returns a list of NuGet versions that exist in nuget.org for a NuGet package that matches the given <paramref name="packageName"/>.
    /// </summary>
    /// <param name="packageName">The name of the package.</param>
    /// <remarks>
    ///     The param <paramref name="packageName"/> is not case sensitive.  The NuGet API
    ///     requires that it is lowercase and is taken care of for you.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="packageName"/> param is null or empty.
    /// </exception>
    /// <exception cref="HttpRequestException">
    ///     Thrown if any HTTP based error occurs.
    /// </exception>
    /// <returns>The asynchronous result of the exiting NuGet package versions.</returns>
    public async Task<string[]> GetNugetVersions(string packageName)
    {
        if (string.IsNullOrEmpty(packageName))
        {
            throw new ArgumentNullException(nameof(packageName), "Must provide a NuGet package name.");
        }

        this.client.AcceptedContentTypes = new[] { "application/vnd.github.v3+json" };

        const string serviceIndexId = "v3-flatcontainer";
        var fullUrl = $"{BaseUrl}/{serviceIndexId}/{packageName.ToLower()}/index.json";
        var request = new RestRequest(fullUrl);

        var response = await this.client.ExecuteAsync<NugetVersionsModel>(request, Method.Get);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            return response.Data is null ? Array.Empty<string>() : response.Data.Versions.ToArray();
        }

        var exception = response.ErrorException ?? new Exception("There was an issue getting data from NuGet.");

        throw new HttpRequestException(
            exception.Message,
            inner: null,
            statusCode: response.StatusCode);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.client.Dispose();

        this.isDisposed = true;
    }
}
