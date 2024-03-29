// <copyright file="ExtensionMethodsTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Net;
using CICDSystem;
using CICDSystemTests.Helpers;
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

    [Fact]
    public async void CloseMilestone_WhenMilestoneDoesNotExist_ThrowsException()
    {
        // Arrange
        var milestoneA = CreateMilestoneObj("v1.2.3-preview.4");
        var milestoneB = CreateMilestoneObj("v4.5.6");

        var milestones = new[] { milestoneA, milestoneB };

        var mockMilestonesClient = new Mock<IMilestonesClient>();
        mockMilestonesClient.Setup(m => m.GetAllForRepository(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(milestones);

        // Act
        var act = async () => await mockMilestonesClient
            .Object.CloseMilestone("test-owner", "test-repo", "v7.8.9");

        // Assert
        var exceptionAssertions = await act.Should().ThrowAsync<NotFoundException>();
        exceptionAssertions.WithMessage("A milestone with the title/name 'v7.8.9' was not found.");
        var exception = exceptionAssertions.Subject.ToArray()[0];

        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async void CloseMilestone_WithIssueUpdatingMilestone_ThrowsException()
    {
        // Arrange
        var milestoneA = CreateMilestoneObj("v1.2.3-preview.4");
        var milestones = new[] { milestoneA };

        var mockMilestonesClient = new Mock<IMilestonesClient>();
        mockMilestonesClient.Setup(m => m.GetAllForRepository(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(milestones);

        mockMilestonesClient.Setup(m => m.Update(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<MilestoneUpdate>())).ReturnsAsync(null as Milestone);

        // Act
        var act = async () => await mockMilestonesClient
            .Object.CloseMilestone(It.IsAny<string>(), It.IsAny<string>(), "v1.2.3-preview.4");

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("The milestone 'v1.2.3-preview.4' could not be updated.");
    }

    [Fact]
    public async void CloseMilestone_WhenMilestoneExists_ClosesMilestone()
    {
        // Arrange
        var milestoneA = CreateMilestoneObj("v1.2.3-preview.4");
        var milestoneB = CreateMilestoneObj("v4.5.6");

        var milestones = new[] { milestoneA, milestoneB };
        MilestoneUpdate? mileStoneUpdateObj = null;

        var mockMilestonesClient = new Mock<IMilestonesClient>();
        mockMilestonesClient.Setup(m => m.GetAllForRepository(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(milestones);
        mockMilestonesClient.Setup(m => m.Update(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<MilestoneUpdate>()))
            .ReturnsAsync(milestoneB)
            .Callback<string, string, int, MilestoneUpdate>((_, _, _, milestoneUpdate) => mileStoneUpdateObj = milestoneUpdate);

        // Act
        var actual = await mockMilestonesClient
            .Object.CloseMilestone("test-owner", "test-repo", "v4.5.6");

        // Assert
        mockMilestonesClient.Verify(m => m.GetAllForRepository("test-owner", "test-repo"), Times.Once);
        mockMilestonesClient.Verify(m =>
            m.Update("test-owner", "test-repo", 123, It.IsAny<MilestoneUpdate>()));
        mileStoneUpdateObj.Should().NotBeNull();
        mileStoneUpdateObj.State.Should().Be(ItemState.Closed);
        actual.Should().BeEquivalentTo(milestoneB);
    }

    [Fact]
    public async void UpdateMilestoneDescription_WhenMilestoneDoesNotExist_ThrowsException()
    {
        // Arrange
        var milestoneA = CreateMilestoneObj("v1.2.3-preview.4");
        var milestoneB = CreateMilestoneObj("v4.5.6");

        var milestones = new[] { milestoneA, milestoneB };

        var mockMilestonesClient = new Mock<IMilestonesClient>();
        mockMilestonesClient.Setup(m => m.GetAllForRepository(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MilestoneRequest>()))
            .ReturnsAsync(milestones);

        // Act
        var act = async () => await mockMilestonesClient
            .Object.UpdateMilestoneDescription("test-owner", "test-repo", "v7.8.9", "test-description");

        // Assert
        var exceptionAssertions = await act.Should().ThrowAsync<NotFoundException>();
        exceptionAssertions.WithMessage("A milestone with the title/name 'v7.8.9' was not found.");
        var exception = exceptionAssertions.Subject.ToArray()[0];

        exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async void UpdateMilestoneDescription_WithIssueUpdatingMilestone_ThrowsException()
    {
        // Arrange
        var milestoneA = CreateMilestoneObj("v1.2.3-preview.4");
        var milestones = new[] { milestoneA };

        var mockMilestonesClient = new Mock<IMilestonesClient>();
        mockMilestonesClient.Setup(m => m.GetAllForRepository(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MilestoneRequest>()))
            .ReturnsAsync(milestones);

        mockMilestonesClient.Setup(m => m.Update(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<int>(),
            It.IsAny<MilestoneUpdate>())).ReturnsAsync(null as Milestone);

        // Act
        var act = async () => await mockMilestonesClient
            .Object.UpdateMilestoneDescription(It.IsAny<string>(), It.IsAny<string>(), "v1.2.3-preview.4", "test-description");

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("The milestone 'v1.2.3-preview.4' description could not be updated.");
    }

    [Fact]
    public async void UpdateMilestoneDescription_WhenMilestoneExists_ClosesMilestone()
    {
        // Arrange
        var milestoneA = CreateMilestoneObj("v1.2.3-preview.4");
        var milestoneB = CreateMilestoneObj("v4.5.6");

        var milestones = new[] { milestoneA, milestoneB };
        MilestoneUpdate? mileStoneUpdateObj = null;

        var mockMilestonesClient = new Mock<IMilestonesClient>();
        mockMilestonesClient.Setup(m => m.GetAllForRepository(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<MilestoneRequest>()))
            .ReturnsAsync(milestones);

        mockMilestonesClient.Setup(m => m.Update(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<MilestoneUpdate>()))
            .ReturnsAsync(milestoneB)
            .Callback<string, string, int, MilestoneUpdate>((_, _, _, milestoneUpdate) => mileStoneUpdateObj = milestoneUpdate);

        // Act
        var actual = await mockMilestonesClient
            .Object.UpdateMilestoneDescription("test-owner", "test-repo", "v4.5.6", "test-description");

        // Assert
        mockMilestonesClient.Verify(m =>
            m.GetAllForRepository("test-owner", "test-repo", It.IsAny<MilestoneRequest>()), Times.Once);
        mockMilestonesClient.Verify(m =>
            m.Update("test-owner", "test-repo", 123, It.IsAny<MilestoneUpdate>()));
        mileStoneUpdateObj.Should().NotBeNull();
        mileStoneUpdateObj.Description.Should().Be("test-description");
        actual.Should().BeEquivalentTo(milestoneB);
    }

    [Fact]
    public void GetLogText_WhenInvoked_ReturnsCorrectResult()
    {
        var expected = $"{Environment.NewLine}  Issue Number: 123";
        expected += $"{Environment.NewLine}  Issue Title: test-issue";
        expected += $"{Environment.NewLine}  Issue State: open";
        expected += $"{Environment.NewLine}  Issue Url: test-url";
        expected += $"{Environment.NewLine}  Labels (2):";
        expected += $"{Environment.NewLine}  \t  - `label-a`";
        expected += $"{Environment.NewLine}  \t  - `label-b`";

        var labelA = CreateLabelObj("label-a");
        var labelB = CreateLabelObj("label-b");
        var labels = new ReadOnlyCollection<Label>(new[] { labelA, labelB });
        var issue = CreateIssueObj(
            title: "test-issue",
            number: 123,
            state: ItemState.Open,
            htmlUrl: "test-url",
            labels: labels);

        // Act
        var actual = issue.GetLogText(2);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void IsPullRequest_WithPullRequestNotSet_ReturnsFalse()
    {
        // Arrange
        var issue = CreateIssueObj();

        // Act
        var actual = issue.IsPullRequest();

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void IsPullRequest_WithSetPullRequest_ReturnsTrue()
    {
        // Arrange
        var issue = CreateIssueObj(pr: CreatePullRequestObj());

        // Act
        var actual = issue.IsPullRequest();

        // Assert
        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData("🧪qa testing", true)]
    [InlineData("invalid-label", false)]
    public void IsQATestingIssue_WithDifferentLabelStates_ReturnsCorrectResult(string labelName, bool expected)
    {
        // Arrange
        var labels = new ReadOnlyCollection<Label>(new[] { CreateLabelObj(labelName) });

        var sut = CreateIssueObj(labels: labels);

        // Act
        var actual = sut.IsQATestingIssue();

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void IsQATestingIssue_WithNoLabels_ReturnsFalse()
    {
        // Arrange
        var labels = new ReadOnlyCollection<Label>(Array.Empty<Label>());

        var sut = CreateIssueObj(labels: labels);

        // Act
        var actual = sut.IsQATestingIssue();

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void IsQATestingIssue_WithMoreThanOneLabel_ReturnsFalse()
    {
        // Arrange
        var correctLabel = CreateLabelObj("🧪qa testing");
        var otherLabel = CreateLabelObj("other-label");
        var labels = new ReadOnlyCollection<Label>(new[] { correctLabel, otherLabel });

        var sut = CreateIssueObj(labels: labels);

        // Act
        var actual = sut.IsQATestingIssue();

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void IsQATestingIssue_WithNullLabels_ReturnsFalse()
    {
        // Arrange
        var sut = CreateIssueObj();

        // Act
        var actual = sut.IsQATestingIssue();

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void IsQATestingIssue_WhenIssueIsAPullRequest_ReturnsFalse()
    {
        // Arrange
        var sut = CreateIssueObj(pr: CreatePullRequestObj());

        // Act
        var actual = sut.IsQATestingIssue();

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData("preview/v1.2.3-preview.4", "preview/v#.#.#-preview.#", true)]
    [InlineData("feature/123-my-branch", "feature/123-my-branch", true)]
    [InlineData("", "", true)]
    [InlineData("", null, true)]
    [InlineData(null, "", true)]
    [InlineData(null, null, true)]
    public void EqualTo_WithCorrectPatternMatch_ReturnsTrue(
        string value,
        string pattern,
        bool expected)
    {
        // Act
        var act = value.EqualTo(pattern);

        // Assert
        act.Should().Be(expected);
    }

    [Fact]
    public void CreateDuplicateCharacters_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange & Act
        var actual = 5.CreateDuplicateCharacters('V');

        // Assert
        actual.Should().Be("VVVVV");
    }

    [Fact]
    public async void IssueExists_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        var mockClient = new Mock<IIssuesClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string _, string _, int _) => ObjectFactory.CreateIssue());

        // Act
        var actual = await mockClient.Object.IssueExists("test-owner", "test-repo", 123);

        // Assert
        mockClient.Verify(m => m.Get("test-owner", "test-repo", 123), Times.Once);
        actual.Should().BeTrue();
    }

    [Fact]
    public async void IssueExists_WhenIssueIsNotFound_ReturnsFalse()
    {
        // Arrange
        var mockClient = new Mock<IIssuesClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, string, int>((_, _, _) => throw new NotFoundException(string.Empty, HttpStatusCode.NotFound));

        // Act
        var actual = await mockClient.Object.IssueExists("test-owner", "test-repo", 123);

        // Assert
        mockClient.Verify(m => m.Get("test-owner", "test-repo", 123), Times.Once);
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(false, -1, false)]
    [InlineData(true, -1, false)]
    [InlineData(true, 0, false)]
    [InlineData(true, 2, true)]
    public async void HasLabels_WhenInvokedForTheIssuesClient_ReturnsCorrectResult(
        bool linkedToPR,
        int totalLabels,
        bool expected)
    {
        // Arrange
        var mockClient = new Mock<IIssuesClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string _, string _, int _) => ObjectFactory.CreateIssue(linkedToPR, totalLabels));

        // Act
        var actual = await mockClient.Object.HasLabels("test-owner", "test-repo", 123);

        // Assert
        mockClient.Verify(m => m.Get("test-owner", "test-repo", 123), Times.Once);
        actual.Should().Be(expected);
    }

    [Fact]
    public async void HasLabels_WhenIssueIsNotFoundForTheIssuesClient_ReturnsFalse()
    {
        // Arrange
        var mockClient = new Mock<IIssuesClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, string, int>((_, _, _) => throw new NotFoundException(string.Empty, HttpStatusCode.NotFound));

        // Act
        var actual = await mockClient.Object.HasLabels("test-owner", "test-repo", 123);

        // Assert
        mockClient.Verify(m => m.Get("test-owner", "test-repo", 123), Times.Once);
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, false)]
    [InlineData(2, true)]
    public async void HasLabels_WhenInvokedForThePullRequestsClient_ReturnsCorrectResult(
        int totalLabels,
        bool expected)
    {
        // Arrange
        var mockClient = new Mock<IPullRequestsClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync((string _, string _, int _) =>
                ObjectFactory.CreatePullRequest(totalLabels: totalLabels));

        // Act
        var actual = await mockClient.Object.HasLabels("test-owner", "test-repo", 123);

        // Assert
        mockClient.Verify(m => m.Get("test-owner", "test-repo", 123), Times.Once);
        actual.Should().Be(expected);
    }

    [Fact]
    public async void HasLabels_WhenIssueIsNotFoundForThePullRequestsClient_ReturnsFalse()
    {
        // Arrange
        var mockClient = new Mock<IPullRequestsClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, string, int>((_, _, _) => throw new NotFoundException(string.Empty, HttpStatusCode.NotFound));

        // Act
        var actual = await mockClient.Object.HasLabels("test-owner", "test-repo", 123);

        // Assert
        mockClient.Verify(m => m.Get("test-owner", "test-repo", 123), Times.Once);
        actual.Should().BeFalse();
    }

    [Fact]
    public async void Exists_WhenPullRequestExists_ReturnsTrue()
    {
        // Arrange
        var mockClient = new Mock<IPullRequestsClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));

        // Act
        var actual = await mockClient.Object.Exists("test-owner", "test-repo", 123);

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public async void Exists_WhenPullRequestDoestNotExist_ReturnsFalse()
    {
        // Arrange
        var mockClient = new Mock<IPullRequestsClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, string, int>((_, _, _) => throw new NotFoundException(string.Empty, HttpStatusCode.NotFound));

        // Act
        var actual = await mockClient.Object.Exists("test-owner", "test-repo", 123);

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(false, -1, false)]
    [InlineData(true, -1, true)]
    [InlineData(true, 0, true)]
    [InlineData(true, 2, true)]
    public async void IsAssigned_WithPullRequestClient_ReturnsCorrectResult(
        bool hasAssignee,
        int totalAssignees,
        bool expected)
    {
        // Arrange
        var mockClient = new Mock<IPullRequestsClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(() => ObjectFactory.CreatePullRequest(hasAssignee: hasAssignee, totalAssignees: totalAssignees));

        // Act
        var actual = await mockClient.Object.IsAssigned("test-owner", "test-repo", 123);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public async void IsAssigned_WithPullRequestClientAndWhenUserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var mockClient = new Mock<IPullRequestsClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, string, int>((_, _, _) => throw new NotFoundException(string.Empty, HttpStatusCode.NotFound));

        // Act
        var actual = await mockClient.Object.IsAssigned("test-owner", "test-repo", It.IsAny<int>());

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(false, -1, false)]
    [InlineData(true, -1, true)]
    [InlineData(true, 0, true)]
    [InlineData(true, 2, true)]
    public void IsAssigned_WhenCheckingIssue_ReturnsCorrectResult(
        bool hasAssignee,
        int totalAssignees,
        bool expected)
    {
        // Arrange
        var issue = ObjectFactory.CreateIssue(hasAssignee: hasAssignee, totalAssignees: totalAssignees);

        // Act
        var actual = issue.IsAssigned();

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(false, -1, false)]
    [InlineData(true, -1, true)]
    [InlineData(true, 0, true)]
    [InlineData(true, 2, true)]
    public async void IsAssigned_WithIssueClient_ReturnsCorrectResult(
        bool hasAssignee,
        int totalAssignees,
        bool expected)
    {
        // Arrange
        var mockClient = new Mock<IIssuesClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(() => ObjectFactory.CreateIssue(hasAssignee: hasAssignee, totalAssignees: totalAssignees));

        // Act
        var actual = await mockClient.Object.IsAssigned("test-owner", "test-repo", 123);

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public async void IsAssigned_WithIssueClientAndWhenUserDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var mockClient = new Mock<IIssuesClient>();
        mockClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, string, int>((_, _, _) => throw new NotFoundException(string.Empty, HttpStatusCode.NotFound));

        // Act
        var actual = await mockClient.Object.IsAssigned("test-owner", "test-repo", It.IsAny<int>());

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("test-value", "test-value")]
    [InlineData("\ttest-value", "test-value")]
    [InlineData(" test-value", "test-value")]
    [InlineData("\ntest-value", "test-value")]
    [InlineData("\rtest-value", "test-value")]
    [InlineData(" \t\n\rtest-value", "test-value")]
    public void TrimWhitespaceStart_WhenInvoked_ReturnsCorrectResult(string value, string expected)
    {
        // Arrange & Act
        var actual = value.TrimWhitespaceStart();

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("test-value", "test-value")]
    [InlineData("test-value\t", "test-value")]
    [InlineData("test-value ", "test-value")]
    [InlineData("test-value\n", "test-value")]
    [InlineData("test-value\r", "test-value")]
    [InlineData("test-value\t\n\r ", "test-value")]
    public void TrimWhitespaceEnd_WhenInvoked_ReturnsCorrectResult(string value, string expected)
    {
        // Arrange & Act
        var actual = value.TrimWhitespaceEnd();

        // Assert
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
            number: 123, // int
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

    /// <summary>
    /// Creates a new pull request object for the purpose of testing.
    /// </summary>
    /// <returns>The pull request to test.</returns>
    private static PullRequest CreatePullRequestObj()
    {
        return new PullRequest(
            id: 123456, // long
            nodeId: string.Empty, // string
            url: string.Empty, // string
            htmlUrl: string.Empty, // string
            diffUrl: string.Empty, // string
            patchUrl: string.Empty, // string
            issueUrl: string.Empty, // string
            statusesUrl: string.Empty, // string
            number: 10, // int
            state: ItemState.Open, // ItemState
            title: string.Empty, // string
            body: string.Empty, // string
            createdAt: DateTimeOffset.Now, // DateTimeOffset
            updatedAt: DateTimeOffset.Now, // DateTimeOffset
            closedAt: null, // DateTimeOffset?
            mergedAt: null, // DateTimeOffset?
            head: null, // GitReference
            @base: null, // GitReference
            user: null, // User
            assignee: null, // User
            assignees: null, // IReadOnlyList<User>
            draft: false, // bool
            mergeable: null, // bool?
            mergeableState: null, // MergeableState?
            mergedBy: null, // User
            mergeCommitSha: string.Empty, // string
            comments: 20, // int
            commits: 30, // int
            additions: 40, // int
            deletions: 50, // int
            changedFiles: 60, // int
            milestone: null, // Milestone
            locked: false, // bool
            maintainerCanModify: null, // bool?
            requestedReviewers: null, // IReadOnlyList<User>
            requestedTeams: null, // IReadOnlyList<Team>
            labels: null, // IReadOnlyList<Label>
            activeLockReason: null); // LockReason?
    }

    /// <summary>
    /// Creates a new issue object that contains the given parameters.
    /// </summary>
    /// <returns>The instance to use for testing.</returns>
    private static Issue CreateIssueObj(
        string title = "",
        int number = 0,
        ItemState state = ItemState.Open,
        string htmlUrl = "",
        IReadOnlyList<Label> labels = null,
        PullRequest pr = null)
    {
        return new Issue(
            url: string.Empty, // string
            htmlUrl: htmlUrl, // string
            commentsUrl: string.Empty, // string
            eventsUrl: null, // string
            number: number, // int
            state: state, // ItemState
            title: title, // string
            body: null, // string
            closedBy: null, // IReadOnlyList<LabelUser
            user: null, // User
            labels: labels, // IReadOnlyList<Label>
            assignee: null, // User
            assignees: null, // IReadOnlyList<User>
            milestone: null, // Milestone
            comments: 55, // int
            pullRequest: pr, // PullRequest
            closedAt: DateTimeOffset.Now, // DateTimeOffsetDateTimeOffset?
            createdAt: DateTimeOffset.Now, // DateTimeOffset
            updatedAt: DateTimeOffset.Now, // DateTimeOffset?
            id: 1, // int
            nodeId: string.Empty, // string
            locked: false, // bool
            repository: null, // Repository
            reactions: null, // ReactionSummary
            activeLockReason: null, // LockReason?
            stateReason: null); // ItemStateReason?
    }

    /// <summary>
    /// Creates a new label object that has the given name.
    /// </summary>
    /// <returns>The instance to use for testing.</returns>
    private static Label CreateLabelObj(string name)
    {
        return new Label(
            id: 123, // long
            url: string.Empty, // string
            name: name, // string
            nodeId: string.Empty, // string
            color: string.Empty, // string
            description: string.Empty, // string
            @default: false); // bool
    }
}
