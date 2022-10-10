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

    [Theory]
    [InlineData("v4.5.6", true)]
    [InlineData("v10.20.30-preview.9", false)]
    public async void MilestoneExists_WhenInvoked_ReturnsCorrectResult(string version, bool expected)
    {
        // Arrange
        var milestoneA = CreateMilestoneObj("v1.2.3-preview.4");
        var milestoneB = CreateMilestoneObj("v4.5.6");

        var milestones = new[] { milestoneA, milestoneB };

        var mockMilestonesClient = new Mock<IMilestonesClient>();
        mockMilestonesClient.Setup(m => m.GetAllForRepository(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(milestones);

        // Act
        var actual = await mockMilestonesClient.Object.MilestoneExists("test-owner", "test-repo", version);

        // Assert
        mockMilestonesClient.Verify(m => m.GetAllForRepository("test-owner", "test-repo"), Times.Once);
        actual.Should().Be(expected);
    }

    [Fact]
    public async void GetByTitle_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        const string title = "v4.5.6";
        var milestoneA = CreateMilestoneObj("v1.2.3-preview.4");
        var milestoneB = CreateMilestoneObj("v4.5.6");

        var milestones = new[] { milestoneA, milestoneB };

        var mockMilestonesClient = new Mock<IMilestonesClient>();
        mockMilestonesClient.Setup(m => m.GetAllForRepository(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(milestones);

        // Act
        var actual = await mockMilestonesClient.Object.GetByTitle("test-owner", "test-repo", title);

        // Assert
        actual.Should().NotBeNull();
        actual.Should().BeEquivalentTo(milestoneB);
        actual.Should().NotBeEquivalentTo(milestoneA);
        mockMilestonesClient.Verify(m => m.GetAllForRepository("test-owner", "test-repo"), Times.Once);
    }

    [Theory]
    [InlineData("v4.5.6", "test-html-url")]
    [InlineData("v10.20.30-preview.9", "")]
    public async void GetHtmlUrl_WhenInvoked_ReturnsCorrectResult(string title, string expected)
    {
        // Arrange
        var milestoneA = CreateMilestoneObj("v1.2.3-preview.4", expected);
        var milestoneB = CreateMilestoneObj("v4.5.6", expected);

        var milestones = new[] { milestoneA, milestoneB };

        var mockMilestonesClient = new Mock<IMilestonesClient>();
        mockMilestonesClient.Setup(m => m.GetAllForRepository(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(milestones);

        // Act
        var actual = await mockMilestonesClient.Object.GetHtmlUrl("test-owner", "test-repo", title);

        // Assert
        mockMilestonesClient.Verify(m => m.GetAllForRepository("test-owner", "test-repo"), Times.Once);
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

    /// <summary>
    /// Creates a new milestone object that contains the given <paramref name="title"/>.
    /// </summary>
    /// <param name="title">The title of the milestone.</param>
    /// <param name="htmlUrl">The HTMl URL to the milestone page.</param>
    /// <returns>The instance to use for testing.</returns>
    private static Milestone CreateMilestoneObj(string title, string htmlUrl = null)
    {
        return new Milestone(
            url: string.Empty, // string
            htmlUrl: string.IsNullOrEmpty(htmlUrl) ? string.Empty : htmlUrl, // string
            id: 1, // long
            number: 2, // int
            nodeId: string.Empty, // string
            state: ItemState.Open, // ItemState
            title: title, // string
            description: "test-milestone", // string
            creator: null, // User
            openIssues: 3, // int
            closedIssues: 4, // int
            createdAt: DateTimeOffset.Now, // DateTimeOffset
            dueOn: DateTimeOffset.Now, // DateTimeOffset?
            closedAt: DateTimeOffset.Now, // DateTimeOffset?
            updatedAt: DateTimeOffset.Now); // DateTimeOffset?
    }
}
