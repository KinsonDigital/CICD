// <copyright file="NugetDataServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Models;
using FluentAssertions;
using Flurl.Http.Testing;
using Xunit;

namespace CICDSystemTests.Services;

using CICDSystem.Services;

/// <summary>
/// Tests the <see cref="NugetDataService"/> class.
/// </summary>
public class NugetDataServiceTests
{
    private readonly HttpTest mockHttp;

    /// <summary>
    /// Initializes a new instance of the <see cref="NugetDataServiceTests"/> class.
    /// </summary>
    public NugetDataServiceTests() => this.mockHttp = new HttpTest();

    #region Method Tests
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async void GetNugetVersions_WithNullOrEmptyPackageName_ThrowsException(string packageName)
    {
        // Arrange
        var sut = new NugetDataService();

        // Act
        var act = () => sut.GetNugetVersions(packageName);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'packageName')");
    }

    [Fact]
    public async void GetNugetVersions_WithNullDataResponse_ThrowsException()
    {
        // Arrange
        this.mockHttp.RespondWithJson(string.Empty);

        var sut = new NugetDataService();

        // Act
        var act = () => sut.GetNugetVersions("test-package");

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>()
            .WithMessage("There was an issue getting data from NuGet.");
    }

    [Fact]
    public async void GetNugetVersions_WhenNugetPackageDoesNotExist_ReturnsEmptyArray()
    {
        // Arrange
        var data = new NugetPackageResponse { Data = Array.Empty<NugetPackageModel>() };
        this.mockHttp.RespondWithJson(data);

        var sut = new NugetDataService();

        // Act
        var actual = await sut.GetNugetVersions("test-package");

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public async void GetNugetVersions_WhenNugetPackageExists_ReturnsCorrectResult()
    {
        // Arrange
        var expected = new[] { "1.2.3", "4.5.6" };
        var nugetVersions = new[]
        {
            new Versions("1.2.3", 1),
            new Versions("4.5.6", 1),
        };

        var packageModel = new NugetPackageModel(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            2,
            false,
            Array.Empty<PackageType>(),
            nugetVersions);
        var data = new NugetPackageResponse { Data = new[] { packageModel } };

        this.mockHttp.RespondWithJson(data);

        var sut = new NugetDataService();

        // Act
        var actual = await sut.GetNugetVersions("test-package");

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }
    #endregion
}
