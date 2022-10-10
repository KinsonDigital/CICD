// <copyright file="ExtensionMethodsTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem;
using FluentAssertions;
using Moq;
using Octokit;
using Xunit;

namespace CICDSystemTests;

public class ExtensionMethodsTests
{
    #region Method Tests
    [Theory]
    [InlineData("CONVERTOKEBAB", "CONVERTOKEBAB")]
    [InlineData("converttokebab", "converttokebab")]
    [InlineData("Convert To Kebab", "convert-to-kebab")]
    [InlineData("ConvertTo Kebab", "convert-to-kebab")]
    [InlineData("Convert ToKebab", "convert-to-kebab")]
    [InlineData("ConvertToKebab", "convert-to-kebab")]
    [InlineData("convert to kebab", "convert-to-kebab")]
    [InlineData("convertTo kebab", "convert-to-kebab")]
    [InlineData("convert toKebab", "convert-to-kebab")]
    [InlineData("convert4to4kebab", "convert4to4kebab")]
    public void ToKebabCase_WhenInvoked_ReturnsCorrectResult(string value, string expected)
    {
        // Arrange & Act
        var actual = value.ToKebabCase();

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("v4.5.6", true)]
    [InlineData("v7.8.9", false)]
    public async void ReleaseExists_WhenInvoked_ReturnsCorrectResult(string tagName, bool expected)
    {
        // Arrange
        var releaseA = CreateReleaseObj("v1.2.3");
        var releaseB = CreateReleaseObj("v4.5.6");

        var releases = new[] { releaseA, releaseB };

        var mockReleasesClient = new Mock<IReleasesClient>();
        mockReleasesClient.Setup(m => m.GetAll(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(releases);

        // Act
        var actual = await mockReleasesClient.Object.ReleaseExists("test-owner", "test-repo", tagName);

        // Assert
        mockReleasesClient.Verify(m => m.GetAll("test-owner", "test-repo"), Times.Once);
        actual.Should().Be(expected);
    }
    #endregion

    /// <summary>
    /// Creates a new release object that contains the given <paramref name="tagName"/>.
    /// </summary>
    /// <param name="tagName">The name of the tag for the release.</param>
    /// <returns>The instance to use for testing.</returns>
    private static Release CreateReleaseObj(string tagName)
    {
        return new Release(
            url: string.Empty, // string
            htmlUrl: string.Empty, // string
            assetsUrl: string.Empty, // string
            uploadUrl: string.Empty, // string
            id: 1, // int
            nodeId: string.Empty, // string
            tagName: tagName, // string
            targetCommitish: string.Empty, // string
            name: string.Empty, // string
            body: string.Empty, // string
            draft: false, // bool
            prerelease: false, // bool
            createdAt: DateTimeOffset.Now, // DateTimeOffset
            publishedAt: null, // DateTimeOffset?
            author: null, // Author
            tarballUrl: string.Empty, // string
            zipballUrl: string.Empty, // string
            assets: null); // IReadOnlyList<ReleaseAsset>
    }
}
