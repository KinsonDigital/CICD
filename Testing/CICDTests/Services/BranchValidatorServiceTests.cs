// <copyright file="BranchValidatorServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Net;
using CICDSystem;
using CICDSystem.Factories;
using CICDSystem.Reactables.Core;
using CICDSystem.Services;
using CICDSystem.Services.Interfaces;
using CICDSystemTests.Helpers;
using FluentAssertions;
using Moq;
using Octokit;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="BranchValidatorService"/> class.
/// </summary>
public class BranchValidatorServiceTests
{
    private const string RepoOwner = "test-owner";
    private const string RepoName = "test-repo-name";
    private readonly Mock<IIssuesClient> mockIssuesClient;
    private readonly Mock<IPullRequestsClient> mockPRClient;
    private readonly Mock<IHttpClientFactory> mockHttpClientFactory;
    private readonly Mock<IReactable<(string, string)>> mockRepoInfoReactable;
    private readonly Mock<IGitRepoWrapper> mockGitRepoWrapper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BranchValidatorServiceTests"/> class.
    /// </summary>
    public BranchValidatorServiceTests()
    {
        this.mockPRClient = new Mock<IPullRequestsClient>();
        this.mockIssuesClient = new Mock<IIssuesClient>();

        var mockGitHubClient = new Mock<IGitHubClient>();
        mockGitHubClient.SetupGet(p => p.PullRequest).Returns(this.mockPRClient.Object);
        mockGitHubClient.SetupGet(p => p.Issue).Returns(this.mockIssuesClient.Object);

        this.mockRepoInfoReactable = new Mock<IReactable<(string, string)>>();

        this.mockHttpClientFactory = new Mock<IHttpClientFactory>();
        this.mockHttpClientFactory.Setup(m => m.CreateGitHubClient()).Returns(mockGitHubClient.Object);

        this.mockGitRepoWrapper = new Mock<IGitRepoWrapper>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullClientFactoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new BranchValidatorService(
                null,
                this.mockGitRepoWrapper.Object,
                this.mockRepoInfoReactable.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'clientFactory')");
    }

    [Fact]
    public void Ctor_WithNullGitRepoWrapperParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new BranchValidatorService(
                this.mockHttpClientFactory.Object,
                null,
                this.mockRepoInfoReactable.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'gitRepoWrapper')");
    }

    [Fact]
    public void Ctor_WithNullRepoInfoReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new BranchValidatorService(
                this.mockHttpClientFactory.Object,
                this.mockGitRepoWrapper.Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'repoInfoReactable')");
    }

    [Fact]
    public void Ctor_WhenOnCompletedIsInvoked_InvokesUnsubscriberDispose()
    {
        // Arrange
        IReactor<(string, string)>? reactor = null;
        var mockUnsubscriber = new Mock<IDisposable>();

        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj)
            .Returns<IReactor<(string, string)>>(_ => mockUnsubscriber.Object);

        this.mockRepoInfoReactable.Setup(m => m.EndNotifications())
            .Callback(() => reactor.OnCompleted());

        _ = CreateService();

        // Act
        this.mockRepoInfoReactable.Object.EndNotifications();

        // Assert
        mockUnsubscriber.Verify(m => m.Dispose(), Times.Once);
    }

    [Fact]
    public void Ctor_WhenOnCompletedIsInvokedWithNullUnsubscriber_DoesNotThrowException()
    {
        // Arrange
        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj)
            .Returns<IReactor<(string, string)>>(_ => null!);

        this.mockRepoInfoReactable.Setup(m => m.EndNotifications())
            .Callback(() => reactor.OnCompleted());

        _ = CreateService();

        // Act
        var act = () => this.mockRepoInfoReactable.Object.EndNotifications();

        // Assert
        act.Should().NotThrow("the unsubscriber should have null check.");
    }
    #endregion

#pragma warning disable SA1202
    #region Method Tests
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidSyntax_WithNullOrEmptyBranchAndWithoutPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.ValidSyntax(branch, string.Empty);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidSyntax_WithNullOrEmptyBranchAndWithPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.ValidSyntax(branch, string.Empty, _ => true);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Fact]
    internal void ValidSyntax_WithInvalidBranchType_ThrowsException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.ValidSyntax("test-branch", (BranchType)1234);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("The enumeration is out of range. (Parameter 'branchType')Actual value was 1234.");
    }

    [Theory]
    [InlineData("release/v1.2.3", "release/v#.#.#", true)]
    [InlineData("release/v1.2.3", "preview/v#.#.#-preview.#", false)]
    [InlineData("other-branch", "release/v#.#.#", false)]
    public void ValidSyntax_WhenInvokedWithSyntaxAndWithoutPredicates_ReturnsCorrectResult(
        string branchName,
        string syntax,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.ValidSyntax(branchName, syntax);
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidSyntax_WithNullOrEmptyBranchAndBranchTypeAndWithoutPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.ValidSyntax(branch, It.IsAny<BranchType>());

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ValidSyntax_WithNullOrEmptyBranchAndBranchTypeAndWithPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.ValidSyntax(branch, It.IsAny<BranchType>(), _ => true);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Fact]
    internal void ValidSyntax_WithInvalidBranchTypeWithPredicates_ThrowsException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.ValidSyntax("test-branch", (BranchType)1234, _ => true);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("The enumeration is out of range. (Parameter 'branchType')Actual value was 1234.");
    }

    [Theory]
    [InlineData("release/v1.2.3", "release/v#.#.#", "release/", "2.3", true)]
    [InlineData("release/v1.2.3", "release/v#.#.#", "preview/", "2.3", false)]
    [InlineData("release/v1.2.3", "release/v#.#.#", "release/", "4.5", false)]
    [InlineData("release/v1.2.3", "preview/v#.#.#-preview.#", "release/", "2.3", false)]
    [InlineData("other-branch", "release/v#.#.#", "release/", "2.3", false)]
    public void ValidSyntax_WhenInvokedWithSyntaxAndPredicates_ReturnsCorrectResult(
        string branchName,
        string syntax,
        string startsWith,
        string endsWith,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.ValidSyntax(
            branchName,
            syntax,
            branch => branch.StartsWith(startsWith),
            branch => branch.EndsWith(endsWith));
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData("master", BranchType.Master, true)]
    [InlineData("develop", BranchType.Develop, true)]
    [InlineData("feature/123-my-branch", BranchType.Feature, true)]
    [InlineData("preview/feature/123-my-branch", BranchType.PreviewFeature, true)]
    [InlineData("release/v1.2.3", BranchType.Release, true)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview, true)]
    [InlineData("hotfix/123-my-fix", BranchType.HotFix, true)]
    [InlineData("other-branch", BranchType.Other, true)]
    internal void ValidSyntax_WhenInvokedWithBranchTypeAndWithoutPredicates_ReturnsCorrectResult(
        string branchName,
        BranchType branchType,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.ValidSyntax(branchName, branchType);
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]

    [InlineData("master", BranchType.Master, "mas", "ter", true)]
    [InlineData("develop", BranchType.Develop, "dev", "elop", true)]
    [InlineData("feature/123-my-branch", BranchType.Feature, "feature/", "-branch", true)]
    [InlineData("preview/feature/123-my-branch", BranchType.PreviewFeature, "preview/feature/", "-branch", true)]
    [InlineData("release/v1.2.3", BranchType.Release, "release/", "2.3", true)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview, "preview/", "-preview.4", true)]
    [InlineData("hotfix/123-my-fix", BranchType.HotFix, "hotfix/", "-fix", true)]
    [InlineData("other-branch", BranchType.Other, "other", "-branch", true)]
    [InlineData("release/v1.2.3", BranchType.Release, "preview/", "2.3", false)]
    [InlineData("release/v1.2.3", BranchType.Release, "release/", "4.5", false)]
    [InlineData("other-branch", BranchType.Release, "release/", "2.3", false)]
    internal void ValidSyntax_WhenInvokedWithBranchTypeAndPredicates_ReturnsCorrectResult(
        string branchName,
        BranchType branchType,
        string startsWith,
        string endsWith,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.ValidSyntax(
            branchName,
            branchType,
            branch => branch.StartsWith(startsWith),
            branch => branch.EndsWith(endsWith));
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsMasterBranch_WithNullOrEmptyBranchAndWithoutPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsMasterBranch(branch);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsMasterBranch_WithNullOrEmptyBranchAndWithPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsMasterBranch(branch, _ => true);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData("master", true)]
    [InlineData("other-branch", false)]
    public void IsMasterBranch_WhenInvokedWithoutPredicates_ReturnsCorrectResult(
        string branchName,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsMasterBranch(branchName);
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData("master", "mas", "ter", true)]
    [InlineData("master", "asdf", "ter", false)]
    [InlineData("master", "mas", "asdf", false)]
    [InlineData("other-branch", "mas", "ter", false)]
    [InlineData("other-branch", "asdf", "ter", false)]
    [InlineData("other-branch", "mas", "asdf", false)]
    public void IsMasterBranch_WhenInvokedWithPredicates_ReturnsCorrectResult(
        string branchName,
        string startsWith,
        string endsWith,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsMasterBranch(branchName,
            branch => branch.StartsWith(startsWith),
            branch => branch.EndsWith(endsWith));
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsDevelopBranch_WithNullOrEmptyBranchAndWithoutPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsDevelopBranch(branch);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsDevelopBranch_WithNullOrEmptyBranchAndWithPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsDevelopBranch(branch, _ => true);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData("develop", true)]
    [InlineData("other-branch", false)]
    public void IsDevelopBranch_WhenInvokedWithoutPredicates_ReturnsCorrectResult(
        string branchName,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsDevelopBranch(branchName);
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData("develop", "dev", "elop", true)]
    [InlineData("develop", "asdf", "elop", false)]
    [InlineData("develop", "dev", "asdf", false)]
    [InlineData("other-branch", "dev", "elop", false)]
    [InlineData("other-branch", "asdf", "elop", false)]
    [InlineData("other-branch", "dev", "asdf", false)]
    public void IsDevelopBranch_WhenInvokedWithPredicates_ReturnsCorrectResult(
        string branchName,
        string startsWith,
        string endsWith,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsDevelopBranch(branchName,
            branch => branch.StartsWith(startsWith),
            branch => branch.EndsWith(endsWith));
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsFeatureBranch_WithNullOrEmptyBranchAndWithoutPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsFeatureBranch(branch);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsFeatureBranch_WithNullOrEmptyBranchAndWithPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsFeatureBranch(branch, _ => true);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData("feature/123-my-branch", true)]
    [InlineData("other-branch", false)]
    public void IsFeatureBranch_WhenInvokedWithoutPredicates_ReturnsCorrectResult(
        string branchName,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsFeatureBranch(branchName);
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData("feature/123-my-branch", "feature", "-branch", true)]
    [InlineData("feature/123-my-branch", "asdf", "-branch", false)]
    [InlineData("feature/123-my-branch", "feature", "asdf", false)]
    [InlineData("other-branch", "feature", "-branch", false)]
    [InlineData("other-branch", "asdf", "-branch", false)]
    [InlineData("other-branch", "feature", "asdf", false)]
    public void IsFeatureBranch_WhenInvokedWithPredicates_ReturnsCorrectResult(
        string branchName,
        string startsWith,
        string endsWith,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsFeatureBranch(branchName,
            branch => branch.StartsWith(startsWith),
            branch => branch.EndsWith(endsWith));
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsPreviewBranch_WithNullOrEmptyBranchAndWithoutPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsPreviewBranch(branch);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsPreviewBranch_WithNullOrEmptyBranchAndWithPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsPreviewBranch(branch, _ => true);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData("preview/v1.2.3-preview.4", true)]
    [InlineData("other-branch", false)]
    public void IsPreviewBranch_WhenInvokedWithoutPredicates_ReturnsCorrectResult(
        string branchName,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsPreviewBranch(branchName);
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData("preview/v1.2.3-preview.4", "preview/", "-preview.4", true)]
    [InlineData("preview/v1.2.3-preview.4", "asdf", "-preview.4", false)]
    [InlineData("preview/v1.2.3-preview.4", "preview/", "asdf", false)]
    [InlineData("other-preview.4", "preview/", "-preview.4", false)]
    [InlineData("other-preview.4", "asdf", "-preview.4", false)]
    [InlineData("other-preview.4", "preview/", "asdf", false)]
    public void IsPreviewBranch_WhenInvokedWithPredicates_ReturnsCorrectResult(
        string branchName,
        string startsWith,
        string endsWith,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsPreviewBranch(branchName,
            branch => branch.StartsWith(startsWith),
            branch => branch.EndsWith(endsWith));
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsPreviewFeatureBranch_WithNullOrEmptyBranchAndWithoutPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsPreviewFeatureBranch(branch);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsPreviewFeatureBranch_WithNullOrEmptyBranchAndWithPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsPreviewFeatureBranch(branch, _ => true);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData("preview/feature/123-my-branch", true)]
    [InlineData("other-branch", false)]
    public void IsPreviewFeatureBranch_WhenInvokedWithoutPredicates_ReturnsCorrectResult(
        string branchName,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsPreviewFeatureBranch(branchName);
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData("preview/feature/123-my-branch", "preview/", "-branch", true)]
    [InlineData("preview/feature/123-my-branch", "asdf", "-branch", false)]
    [InlineData("preview/feature/123-my-branch", "preview/", "asdf", false)]
    [InlineData("other-branch", "preview/", "-branch", false)]
    [InlineData("other-branch", "asdf", "-branch", false)]
    [InlineData("other-branch", "preview/", "asdf", false)]
    public void IsPreviewFeatureBranch_WhenInvokedWithPredicates_ReturnsCorrectResult(
        string branchName,
        string startsWith,
        string endsWith,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsPreviewFeatureBranch(branchName,
            branch => branch.StartsWith(startsWith),
            branch => branch.EndsWith(endsWith));
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsReleaseBranch_WithNullOrEmptyBranchAndWithoutPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsReleaseBranch(branch);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsReleaseBranch_WithNullOrEmptyBranchAndWithPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsReleaseBranch(branch, _ => true);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData("release/v1.2.3", true)]
    [InlineData("other-branch", false)]
    public void IsReleaseBranch_WhenInvokedWithoutPredicates_ReturnsCorrectResult(
        string branchName,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsReleaseBranch(branchName);
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData("release/v1.2.3", "release/", "2.3", true)]
    [InlineData("release/v1.2.3", "asdf", "2.3", false)]
    [InlineData("release/v1.2.3", "release/", "asdf", false)]
    [InlineData("other2.3", "release/", "2.3", false)]
    [InlineData("other2.3", "asdf", "2.3", false)]
    [InlineData("other2.3", "release/", "asdf", false)]
    public void IsReleaseBranch_WhenInvokedWithPredicates_ReturnsCorrectResult(
        string branchName,
        string startsWith,
        string endsWith,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsReleaseBranch(branchName,
            branch => branch.StartsWith(startsWith),
            branch => branch.EndsWith(endsWith));
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsHotFixBranch_WithNullOrEmptyBranchAndWithoutPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsHotFixBranch(branch);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void IsHotFixBranch_WithNullOrEmptyBranchAndWithPredicates_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.IsHotFixBranch(branch, _ => true);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData("hotfix/123-my-fix", true)]
    [InlineData("other-branch", false)]
    public void IsHotFixBranch_WhenInvokedWithoutPredicates_ReturnsCorrectResult(
        string branchName,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsHotFixBranch(branchName);
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData("hotfix/123-my-fix", "hotfix/", "-fix", true)]
    [InlineData("hotfix/123-my-fix", "asdf", "-fix", false)]
    [InlineData("hotfix/123-my-fix", "hotfix/", "asdf", false)]
    [InlineData("other-fix", "hotfix/", "-fix", false)]
    [InlineData("other-fix", "asdf", "-fix", false)]
    [InlineData("other-fix", "hotfix/", "asdf", false)]
    public void IsHotFixBranch_WhenInvokedWithPredicates_ReturnsCorrectResult(
        string branchName,
        string startsWith,
        string endsWith,
        bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var returnedService = service.IsHotFixBranch(branchName,
            branch => branch.StartsWith(startsWith),
            branch => branch.EndsWith(endsWith));
        var actual = returnedService.GetValue();

        // Assert
        actual.Should().Be(expected);
        returnedService.Should().BeSameAs(service);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void PRSourceBranchCorrect_WithNullOrEmptyBranchAndWithBranchType_ThrowsException(int prNumber)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.PRSourceBranchCorrect(prNumber, It.IsAny<BranchType>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The parameter must be greater than 0. (Parameter 'prNumber')");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void PRSourceBranchCorrect_WithNullOrEmptyBranchAndWithPredicates_ThrowsException(int prNumber)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () =>
            service.PRSourceBranchCorrect(prNumber, It.IsAny<BranchType>(), _ => true);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The parameter must be greater than 0. (Parameter 'prNumber')");
    }

    [Fact]
    internal void PRSourceBranchCorrect_WithInvalidBranchTypeWithoutPredicates_ThrowsException()
    {
        // Arrange
        var pullRequest = ObjectFactory.CreatePullRequest("src-branch", "target-branch");
        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(pullRequest);
        var service = CreateService();

        // Act
        var act = () => service.PRSourceBranchCorrect(123, (BranchType)1234);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("The enumeration is out of range. (Parameter 'branchType')Actual value was 1234.");
    }

    [Fact]
    internal void PRSourceBranchCorrect_WithInvalidBranchTypeWithPredicates_ThrowsException()
    {
        // Arrange
        var pullRequest = ObjectFactory.CreatePullRequest("src-branch", "target-branch");
        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(pullRequest);
        var service = CreateService();

        // Act
        var act = () => service.PRSourceBranchCorrect(123, (BranchType)1234, _ => true);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("The enumeration is out of range. (Parameter 'branchType')Actual value was 1234.");
    }

    [Theory]
    [InlineData("master", BranchType.Master, true)]
    [InlineData("develop", BranchType.Develop, true)]
    [InlineData("feature/10-my-feature", BranchType.Feature, true)]
    [InlineData("preview/feature/20-my-feature", BranchType.PreviewFeature, true)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview, true)]
    [InlineData("release/v1.2.3", BranchType.Release, true)]
    [InlineData("hotfix/30-my-fix", BranchType.HotFix, true)]
    [InlineData("develop", BranchType.Other, false)]
    [InlineData("feature/10-my-feature", BranchType.Other, false)]
    [InlineData("preview/feature/20-my-feature", BranchType.Other, false)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Other, false)]
    [InlineData("release/v1.2.3", BranchType.Other, false)]
    [InlineData("hotfix/30-my-fix", BranchType.Other, false)]
    [InlineData("other-branch", BranchType.Other, false)]
    [InlineData("master", BranchType.Other, false)]
    internal void PRSourceBranchCorrect_WhenInvokedWithoutPredicates_ReturnsCorrectResult(
        string srcBranch,
        BranchType branchType,
        bool expected)
    {
        // Arrange
        var repoInfoData = (RepoOwner, RepoName);
        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj);
        this.mockRepoInfoReactable.Setup(m => m.PushNotification(It.IsAny<(string, string)>()))
            .Callback<(string, string)>(data => reactor.OnNext(data));

        var pullRequest = ObjectFactory.CreatePullRequest(srcBranch, "target-branch");
        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(pullRequest);

        var service = CreateService();
        this.mockRepoInfoReactable.Object.PushNotification(repoInfoData);

        // Act
        var actual = service.PRSourceBranchCorrect(123, branchType).GetValue();

        // Assert
        this.mockPRClient.Verify(m => m.Get(RepoOwner, RepoName, 123), Times.Once);
        actual.Should().Be(expected);
    }

    [Fact]
    internal void PRSourceBranchCorrect_WhenInvokedWithoutPredicatesAndPRDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repoInfoData = (RepoOwner, RepoName);
        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj);
        this.mockRepoInfoReactable.Setup(m => m.PushNotification(It.IsAny<(string, string)>()))
            .Callback<(string, string)>(data => reactor.OnNext(data));

        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, string, int>((_, _, _) =>
                throw new NotFoundException("not found", HttpStatusCode.NotFound));

        var service = CreateService();
        this.mockRepoInfoReactable.Object.PushNotification(repoInfoData);

        // Act
        var actual = service.PRSourceBranchCorrect(123, It.IsAny<BranchType>()).GetValue();

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData("master", BranchType.Master, "mas", "ter", true)]
    [InlineData("develop", BranchType.Develop, "dev", "elop", true)]
    [InlineData("feature/10-my-branch", BranchType.Feature, "feature/", "-branch", true)]
    [InlineData("preview/feature/20-my-branch", BranchType.PreviewFeature, "preview/", "-branch", true)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview, "preview/", "-preview.4", true)]
    [InlineData("release/v1.2.3", BranchType.Release, "release/", "2.3", true)]
    [InlineData("hotfix/30-my-fix", BranchType.HotFix, "hotfix/", "-fix", true)]
    [InlineData("master", BranchType.Master, "asdf", "ter", false)]
    [InlineData("master", BranchType.Master, "mas", "asdf", false)]
    [InlineData("develop", BranchType.Develop, "asdf", "elop", false)]
    [InlineData("develop", BranchType.Develop, "dev", "asdf", false)]
    [InlineData("feature/10-my-branch", BranchType.Feature, "asdf", "-branch", false)]
    [InlineData("feature/10-my-branch", BranchType.Feature, "feature/", "asdf", false)]
    [InlineData("preview/feature/20-my-branch", BranchType.PreviewFeature, "asdf", "-branch", false)]
    [InlineData("preview/feature/20-my-branch", BranchType.PreviewFeature, "preview/", "asdf", false)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview, "asdf", "-preview.4", false)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview, "preview/", "asdf", false)]
    [InlineData("release/v1.2.3", BranchType.Release, "asdf", "2.3", false)]
    [InlineData("release/v1.2.3", BranchType.Release, "release/", "asdf", false)]
    [InlineData("hotfix/30-my-fix", BranchType.HotFix, "asdf", "-fix", false)]
    [InlineData("hotfix/30-my-fix", BranchType.HotFix, "hotfix/", "asdf", false)]
    [InlineData("master", BranchType.Other, "mas", "ter", false)]
    [InlineData("develop", BranchType.Other, "dev", "elop", false)]
    [InlineData("feature/10-my-branch", BranchType.Other, "feature/", "-branch", false)]
    [InlineData("preview/feature/20-my-branch", BranchType.Other, "preview/", "-branch", false)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Other, "preview/", "-preview.4", false)]
    [InlineData("release/v1.2.3", BranchType.Other, "release/", "2.3", false)]
    [InlineData("hotfix/30-my-fix", BranchType.Other, "hotfix/", "-fix", false)]
    [InlineData("other-branch", BranchType.Other, "other", "-branch", false)]
    internal void PRSourceBranchCorrect_WhenInvokedWithPredicates_ReturnsCorrectResult(
        string srcBranch,
        BranchType branchType,
        string startsWith,
        string endsWith,
        bool expected)
    {
        // Arrange
        var repoInfoData = (RepoOwner, RepoName);
        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj);
        this.mockRepoInfoReactable.Setup(m => m.PushNotification(It.IsAny<(string, string)>()))
            .Callback<(string, string)>(data => reactor.OnNext(data));

        var pullRequest = ObjectFactory.CreatePullRequest(srcBranch, "target-branch");
        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(pullRequest);

        var service = CreateService();
        this.mockRepoInfoReactable.Object.PushNotification(repoInfoData);

        // Act
        var actual = service.PRSourceBranchCorrect(
            123,
            branchType,
            branch => branch.StartsWith(startsWith),
            branch => branch.EndsWith(endsWith)).GetValue();

        // Assert
        this.mockPRClient.Verify(m => m.Get(RepoOwner, RepoName, 123), Times.Once);
        actual.Should().Be(expected);
    }

    [Fact]
    internal void PRSourceBranchCorrect_WhenInvokedWithPredicatesAndPRDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repoInfoData = (RepoOwner, RepoName);
        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj);
        this.mockRepoInfoReactable.Setup(m => m.PushNotification(It.IsAny<(string, string)>()))
            .Callback<(string, string)>(data => reactor.OnNext(data));

        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, string, int>((_, _, _) =>
                throw new NotFoundException("not found", HttpStatusCode.NotFound));

        var service = CreateService();
        this.mockRepoInfoReactable.Object.PushNotification(repoInfoData);

        // Act
        var actual = service.PRSourceBranchCorrect(123, It.IsAny<BranchType>(), _ => true).GetValue();

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void PRTargetBranchCorrect_WithNullOrEmptyBranchAndWithBranchType_ThrowsException(int prNumber)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.PRTargetBranchCorrect(prNumber, It.IsAny<BranchType>());

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The parameter must be greater than 0. (Parameter 'prNumber')");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public void PRTargetBranchCorrect_WithNullOrEmptyBranchAndWithPredicates_ThrowsException(int prNumber)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () =>
            service.PRTargetBranchCorrect(prNumber, It.IsAny<BranchType>(), _ => true);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("The parameter must be greater than 0. (Parameter 'prNumber')");
    }

    [Fact]
    internal void PRTargetBranchCorrect_WithInvalidBranchTypeWithoutPredicates_ThrowsException()
    {
        // Arrange
        var pullRequest = ObjectFactory.CreatePullRequest("src-branch", "target-branch");
        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(pullRequest);
        var service = CreateService();

        // Act
        var act = () => service.PRTargetBranchCorrect(123, (BranchType)1234);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("The enumeration is out of range. (Parameter 'branchType')Actual value was 1234.");
    }

    [Fact]
    internal void PRTargetBranchCorrect_WithInvalidBranchTypeWithPredicates_ThrowsException()
    {
        // Arrange
        var pullRequest = ObjectFactory.CreatePullRequest("src-branch", "target-branch");
        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(pullRequest);
        var service = CreateService();

        // Act
        var act = () => service.PRTargetBranchCorrect(123, (BranchType)1234, _ => true);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("The enumeration is out of range. (Parameter 'branchType')Actual value was 1234.");
    }

    [Theory]
    [InlineData("master", BranchType.Master, true)]
    [InlineData("develop", BranchType.Develop, true)]
    [InlineData("feature/10-my-feature", BranchType.Feature, true)]
    [InlineData("preview/feature/20-my-feature", BranchType.PreviewFeature, true)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview, true)]
    [InlineData("release/v1.2.3", BranchType.Release, true)]
    [InlineData("hotfix/30-my-fix", BranchType.HotFix, true)]
    [InlineData("develop", BranchType.Other, false)]
    [InlineData("feature/10-my-feature", BranchType.Other, false)]
    [InlineData("preview/feature/20-my-feature", BranchType.Other, false)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Other, false)]
    [InlineData("release/v1.2.3", BranchType.Other, false)]
    [InlineData("hotfix/30-my-fix", BranchType.Other, false)]
    [InlineData("other-branch", BranchType.Other, false)]
    [InlineData("master", BranchType.Other, false)]
    internal void PRTargetBranchCorrect_WhenInvokedWithoutPredicates_ReturnsCorrectResult(
        string targetBranch,
        BranchType branchType,
        bool expected)
    {
        // Arrange
        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj);
        this.mockRepoInfoReactable.Setup(m => m.PushNotification(It.IsAny<(string, string)>()))
            .Callback<(string, string)>(data => reactor.OnNext(data));

        var pullRequest = ObjectFactory.CreatePullRequest("src-branch", targetBranch);
        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(pullRequest);

        var service = CreateService();

        // Act
        var actual = service.PRTargetBranchCorrect(123, branchType).GetValue();

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    internal void PRTargetBranchCorrect_WhenInvokedWithoutPredicatesAndPRDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repoInfoData = (RepoOwner, RepoName);
        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj);
        this.mockRepoInfoReactable.Setup(m => m.PushNotification(It.IsAny<(string, string)>()))
            .Callback<(string, string)>(data => reactor.OnNext(data));

        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, string, int>((_, _, _) =>
                throw new NotFoundException("not found", HttpStatusCode.NotFound));

        var service = CreateService();
        this.mockRepoInfoReactable.Object.PushNotification(repoInfoData);

        // Act
        var actual = service.PRTargetBranchCorrect(123, It.IsAny<BranchType>()).GetValue();

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData("master", BranchType.Master, "mas", "ter", true)]
    [InlineData("develop", BranchType.Develop, "dev", "elop", true)]
    [InlineData("feature/10-my-branch", BranchType.Feature, "feature/", "-branch", true)]
    [InlineData("preview/feature/20-my-branch", BranchType.PreviewFeature, "preview/", "-branch", true)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview, "preview/", "-preview.4", true)]
    [InlineData("release/v1.2.3", BranchType.Release, "release/", "2.3", true)]
    [InlineData("hotfix/30-my-fix", BranchType.HotFix, "hotfix/", "-fix", true)]
    [InlineData("master", BranchType.Master, "asdf", "ter", false)]
    [InlineData("master", BranchType.Master, "mas", "asdf", false)]
    [InlineData("develop", BranchType.Develop, "asdf", "elop", false)]
    [InlineData("develop", BranchType.Develop, "dev", "asdf", false)]
    [InlineData("feature/10-my-branch", BranchType.Feature, "asdf", "-branch", false)]
    [InlineData("feature/10-my-branch", BranchType.Feature, "feature/", "asdf", false)]
    [InlineData("preview/feature/20-my-branch", BranchType.PreviewFeature, "asdf", "-branch", false)]
    [InlineData("preview/feature/20-my-branch", BranchType.PreviewFeature, "preview/", "asdf", false)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview, "asdf", "-preview.4", false)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview, "preview/", "asdf", false)]
    [InlineData("release/v1.2.3", BranchType.Release, "asdf", "2.3", false)]
    [InlineData("release/v1.2.3", BranchType.Release, "release/", "asdf", false)]
    [InlineData("hotfix/30-my-fix", BranchType.HotFix, "asdf", "-fix", false)]
    [InlineData("hotfix/30-my-fix", BranchType.HotFix, "hotfix/", "asdf", false)]
    [InlineData("master", BranchType.Other, "mas", "ter", false)]
    [InlineData("develop", BranchType.Other, "dev", "elop", false)]
    [InlineData("feature/10-my-branch", BranchType.Other, "feature/", "-branch", false)]
    [InlineData("preview/feature/20-my-branch", BranchType.Other, "preview/", "-branch", false)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Other, "preview/", "-preview.4", false)]
    [InlineData("release/v1.2.3", BranchType.Other, "release/", "2.3", false)]
    [InlineData("hotfix/30-my-fix", BranchType.Other, "hotfix/", "-fix", false)]
    [InlineData("other-branch", BranchType.Other, "other", "-branch", false)]
    internal void PRTargetBranchCorrect_WhenInvokedWithPredicates_ReturnsCorrectResult(
        string targetBranch,
        BranchType branchType,
        string startsWith,
        string endsWith,
        bool expected)
    {
        // Arrange
        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj);
        this.mockRepoInfoReactable.Setup(m => m.PushNotification(It.IsAny<(string, string)>()))
            .Callback<(string, string)>(data => reactor.OnNext(data));

        var pullRequest = ObjectFactory.CreatePullRequest("src-branch", targetBranch);
        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(pullRequest);

        var service = CreateService();

        // Act
        var actual = service.PRTargetBranchCorrect(
            123,
            branchType,
            branch => branch.StartsWith(startsWith),
            branch => branch.EndsWith(endsWith)).GetValue();

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    internal void PRTargetBranchCorrect_WhenInvokedWithPredicatesAndPRDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var repoInfoData = (RepoOwner, RepoName);
        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj);
        this.mockRepoInfoReactable.Setup(m => m.PushNotification(It.IsAny<(string, string)>()))
            .Callback<(string, string)>(data => reactor.OnNext(data));

        this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
            .Callback<string, string, int>((_, _, _) =>
                throw new NotFoundException("not found", HttpStatusCode.NotFound));

        var service = CreateService();
        this.mockRepoInfoReactable.Object.PushNotification(repoInfoData);

        // Act
        var actual = service.PRTargetBranchCorrect(123, It.IsAny<BranchType>(), _ => true).GetValue();

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void BranchIssueNumberExists_WithNullOrEmpty_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.BranchIssueNumberExists(branch);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData("develop")]
    [InlineData("master")]
    [InlineData("preview/v1.2.3-preview.4")]
    [InlineData("release/v1.2.3")]
    public void BranchIssueNumberExists_WhenTheIssueDoesNotExistInTheBranchName_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.BranchIssueNumberExists(branch);

        // Assert
        this.mockIssuesClient.Verify(m =>
            m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        act.Should().Throw<Exception>()
            .WithMessage($"The branch '{branch}' is not the correct branch that contains an issue number.");
    }

    [Theory]
    [InlineData("feature/123-my-branch", true)]
    [InlineData("preview/feature/123-my-branch", true)]
    [InlineData("hotfix/123-my-branch", true)]
    public void BranchIssueNumberExists_WhenAnIssueNumberExistsInTheBranchName_ReturnsCorrectResult(
        string branch,
        bool expected)
    {
        // Arrange
        var repoInfoData = (RepoOwner, RepoName);
        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj);
        this.mockRepoInfoReactable.Setup(m => m.PushNotification(It.IsAny<(string, string)>()))
            .Callback<(string, string)>(data => reactor.OnNext(data));

        this.mockIssuesClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), 123))
            .ReturnsAsync(new Issue());
        var service = CreateService();

        this.mockRepoInfoReactable.Object.PushNotification(repoInfoData);

        // Act
        var actual = service.BranchIssueNumberExists(branch);

        // Assert
        this.mockIssuesClient.Verify(m => m.Get(RepoOwner, RepoName, 123), Times.Once);
        actual.Should().Be(expected);
    }

    [Fact]
    public void BranchIssueNumberExists_WhenAnIssueDoesNotExist_ReturnsFalse()
    {
        // Arrange
        this.mockIssuesClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), 123))
            .Callback<string, string, int>((_, _, _) => throw new NotFoundException("not found", HttpStatusCode.NotFound));

        var service = CreateService();

        // Act
        var actual = service.BranchIssueNumberExists("feature/123-my-branch");

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void Or_WhenUsedByItself_ResolvesToFalse()
    {
        // Arrange
        var service = CreateService();

        // Act
        var actual = service.Or().GetValue();

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData("master", true)]
    [InlineData("develop", true)]
    [InlineData("other-branch", false)]
    public void Or_WithTwoMethodsThatDoesNotEndWithOrMethod_ReturnsCorrectResult(string branch, bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var actual = service
            .IsMasterBranch(branch)
            .Or()
            .IsDevelopBranch(branch)
            .GetValue();

        // Assert
        actual.Should().Be(expected);
    }

    [Fact]
    public void Reset_WhenUsingMethodsBeforeReset_ResolvesToFalse()
    {
        // Arrange
        var service = CreateService();
        service
            .IsMasterBranch("master")
            .Or()
            .IsDevelopBranch("develop");

        // Act
        var objResult = service.Reset();
        var actual = objResult.GetValue();

        // Assert
        objResult.Should().BeSameAs(service);
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData("master", true)]
    [InlineData("develop", true)]
    [InlineData("other-branch", false)]
    public void GetValue_WithTwoMethodsThatDoesEndWithOrMethod_ReturnsCorrectResult(string branch, bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var actual = service
            .IsMasterBranch(branch)
            .Or()
            .IsDevelopBranch(branch)
            .Or()
            .GetValue();

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("develop", true)]
    [InlineData("release/v1.2.3", true)]
    [InlineData("other-branch", false)]
    public void GetValue_WithThreeMethodsThatDoesNotEndWithOrMethod_ReturnsCorrectResult(string branch, bool expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var actual = service
            .IsDevelopBranch(branch)
            .Or()
            .IsReleaseBranch(branch)
            .ValidSyntax(branch, "release/v#.#.#")
            .GetValue();

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetBranchType_WithNullOrEmpty_ThrowsException(string branch)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.GetBranchType(branch);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'branch')");
    }

    [Theory]
    [InlineData("master", BranchType.Master)]
    [InlineData("develop", BranchType.Develop)]
    [InlineData("feature/456-my-branch", BranchType.Feature)]
    [InlineData("preview/feature/123-my-branch", BranchType.PreviewFeature)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview)]
    [InlineData("release/v1.2.3", BranchType.Release)]
    [InlineData("other-branch", BranchType.Other)]
    internal void GetBranchType_WhenInvoked_ReturnsCorrectResult(
        string branch,
        BranchType expected)
    {
        // Arrange
        var service = CreateService();

        // Act
        var actual = service.GetBranchType(branch);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CurrentBranchIsValid_WithBranchSyntaxAndWhenRepoBranchAsNull_ThrowsException(string branch)
    {
        // Arrange
        this.mockGitRepoWrapper.SetupGet(p => p.Branch).Returns(branch);
        var service = CreateService();

        // Act
        var act = () => service.CurrentBranchIsValid(It.IsAny<string>());

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("Could not detect the current repository branch.  Does a repository exist?");
    }

    [Theory]
    [InlineData("master", "master", true)]
    [InlineData("develop", "develop", true)]
    [InlineData("feature/123-my-branch", "feature/#-my-branch", true)]
    [InlineData("preview/feature/123-my-branch", "preview/feature/#-my-branch", true)]
    [InlineData("release/v1.2.3", "release/v#.#.#", true)]
    [InlineData("preview/v1.2.3-preview.4", "preview/v#.#.#-preview.#", true)]
    [InlineData("hotfix/123-my-fix", "hotfix/#-my-fix", true)]
    [InlineData("other-branch", "other-branch", true)]
    internal void CurrentBranchIsValid_WithBranchSyntax_ReturnsCorrectResult(
        string branch,
        string branchSyntax,
        bool expected)
    {
        // Arrange
        this.mockGitRepoWrapper.SetupGet(p => p.Branch).Returns(branch);

        var service = CreateService();

        // Act
        var actual = service.CurrentBranchIsValid(branchSyntax);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CurrentBranchIsValid_WithBranchTypeAndWhenRepoBranchAsNull_ThrowsException(string branch)
    {
        // Arrange
        this.mockGitRepoWrapper.SetupGet(p => p.Branch).Returns(branch);
        var service = CreateService();

        // Act
        var act = () => service.CurrentBranchIsValid(It.IsAny<BranchType>());

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("Could not detect the current repository branch.  Does a repository exist?");
    }

    [Theory]
    [InlineData("master", BranchType.Master, true)]
    [InlineData("develop", BranchType.Develop, true)]
    [InlineData("feature/123-my-branch", BranchType.Feature, true)]
    [InlineData("preview/feature/123-my-branch", BranchType.PreviewFeature, true)]
    [InlineData("release/v1.2.3", BranchType.Release, true)]
    [InlineData("preview/v1.2.3-preview.4", BranchType.Preview, true)]
    [InlineData("hotfix/123-my-fix", BranchType.HotFix, true)]
    [InlineData("other-branch", BranchType.Other, false)]
    internal void CurrentBranchIsValid_WithBranchType_ReturnsCorrectResult(
        string branch,
        BranchType branchType,
        bool expected)
    {
        // Arrange
        this.mockGitRepoWrapper.SetupGet(p => p.Branch).Returns(branch);

        var service = CreateService();

        // Act
        var actual = service.CurrentBranchIsValid(branchType);

        // Assert
        actual.Should().Be(expected);
    }
    #endregion
#pragma warning restore SA1202

    /// <summary>
    /// Creates a new instance of <see cref="BranchValidatorService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private BranchValidatorService CreateService()
        => new (this.mockHttpClientFactory.Object,
                this.mockGitRepoWrapper.Object,
                this.mockRepoInfoReactable.Object);
}
