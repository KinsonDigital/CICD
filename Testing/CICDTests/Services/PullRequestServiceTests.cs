// <copyright file="PullRequestServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Net;
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
/// Tests the <see cref="PullRequestService"/> class.
/// </summary>
public class PullRequestServiceTests
{
    private const string SrcBranch = "src-branch";
    private const string TargetBranch = "target-branch";
    private const string RepoOwner = "repo-owner";
    private const string RepoName = "repo-name";
    private readonly Mock<IReactable<(string, string)>> mockRepoInfoReactable;
    private readonly Mock<IReactable<int>> mockPRNumReactable;
    private readonly Mock<IExecutionContextService> mockContextService;
    private readonly Mock<IHttpClientFactory> mockClientFactory;
    private readonly Mock<IPullRequestsClient> mockPRClient;
    private IReactor<(string, string)>? repoInfoReactor;

    /// <summary>
    /// Initializes a new instance of the <see cref="PullRequestServiceTests"/> class.
    /// </summary>
    public PullRequestServiceTests()
    {
        this.mockRepoInfoReactable = new Mock<IReactable<(string, string)>>();
        this.mockPRNumReactable = new Mock<IReactable<int>>();
        this.mockContextService = new Mock<IExecutionContextService>();

        var mockGitHubClient = new Mock<IGitHubClient>();
        this.mockPRClient = new Mock<IPullRequestsClient>();

        mockGitHubClient.SetupGet(p => p.PullRequest).Returns(this.mockPRClient.Object);

        this.mockClientFactory = new Mock<IHttpClientFactory>();
        this.mockClientFactory.Setup(m => m.CreateGitHubClient()).Returns(mockGitHubClient.Object);
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullRepoInfoReactableReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new PullRequestService(
                null,
                this.mockPRNumReactable.Object,
                this.mockClientFactory.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'repoInfoReactable')");
    }

    [Fact]
    public void Ctor_WithNullPRNumberReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new PullRequestService(
                this.mockRepoInfoReactable.Object,
                null,
                this.mockClientFactory.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'prNumberReactable')");
    }

    [Fact]
    public void Ctor_WithNullClientFactoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new PullRequestService(
                this.mockRepoInfoReactable.Object,
                this.mockPRNumReactable.Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'clientFactory')");
    }

    [Fact]
    public void Ctor_WhenInvoked_SetsUpUnsubscriberDisposal()
    {
        // Arrange
        IReactor<int>? reactor = null;
        var mockUnsubscriber = new Mock<IDisposable>();

        this.mockPRNumReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<int>>()))
            .Returns<IReactor<int>>(_ => mockUnsubscriber.Object)
            .Callback<IReactor<int>>(reactorObj => reactor = reactorObj);

        _ = CreateService();

        // Act
        reactor.OnCompleted();

        // Assert
        mockUnsubscriber.Verify(m => m.Dispose(), Times.Once);
    }
    #endregion

    #region Prop Tests
    [Fact]
    public void SourceBranch_WhenPRDoesNotExist_ThrowException()
    {
        // Arrange
        MockAsServerBuild();
        MockPRClient(throwNotFound: true);

        var service = CreateService();

        // Act
        var act = () => service.SourceBranch;

        // Assert
        act.Should().Throw<NotFoundException>()
            .WithMessage("not found");
    }

    [Fact]
    public void SourceBranch_WhenPRExists_ReturnsCorrectResult()
    {
        // Arrange
        MockAsServerBuild();
        MockInfoReactable();
        MockPRClient();

        var service = CreateService();

        this.mockRepoInfoReactable.Object.PushNotification((RepoOwner, RepoName));

        // Act
        var actual = service.SourceBranch;

        // Assert
        actual.Should().Be(SrcBranch);
    }

    [Fact]
    public void TargetBranch_WhenPRDoesNotExist_ThrowException()
    {
        // Arrange
        MockAsServerBuild();
        MockPRClient(throwNotFound: true);

        var service = CreateService();

        // Act
        var act = () => service.TargetBranch;

        // Assert
        act.Should().Throw<NotFoundException>()
            .WithMessage("not found");
    }

    [Fact]
    public void TargetBranch_WhenPRExists_ReturnsCorrectResult()
    {
        // Arrange
        MockAsServerBuild();
        MockInfoReactable();
        MockPRClient();

        var service = CreateService();

        this.mockRepoInfoReactable.Object.PushNotification((RepoOwner, RepoName));

        // Act
        var actual = service.TargetBranch;

        // Assert
        actual.Should().Be(TargetBranch);
    }

    [Fact]
    public void PullRequestNumber_WhenGettingValueWithLocalBuild_ReturnsCorrectResult()
    {
        // Arrange
        IReactor<int>? reactor = null;

        MockAsServerBuild();

        this.mockPRNumReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<int>>()))
            .Callback<IReactor<int>>(reactorObj => reactor = reactorObj);

        this.mockPRNumReactable.Setup(m => m.PushNotification(It.IsAny<int>()))
            .Callback<int>(data => reactor.OnNext(data));

        var sut = CreateService();

        this.mockPRNumReactable.Object.PushNotification(123);

        // Act
        var actual = sut.PullRequestNumber;

        // Assert
        actual.Should().Be(123);
    }
    #endregion

    /// <summary>
    /// Mocks the context as running on the server.
    /// </summary>
    private void MockAsServerBuild()
    {
        this.mockContextService.SetupGet(p => p.IsServerBuild).Returns(true);
        this.mockContextService.SetupGet(p => p.IsLocalBuild).Returns(false);
    }

    /// <summary>
    /// Mocks the <see cref="Reactable{TData}.Subscribe"/> and <see cref="Reactable{TData}.PushNotification"/> methods of the repository info <see cref="Reactable{TData}"/>.
    /// </summary>
    private void MockInfoReactable()
    {
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => this.repoInfoReactor = reactorObj);

        this.mockRepoInfoReactable.Setup(m => m.PushNotification(It.IsAny<(string, string)>()))
            .Callback<(string, string)>(data => this.repoInfoReactor.OnNext(data));
    }

    /// <summary>
    /// Mocks the <see cref="IPullRequestsClient"/>.<see cref="IPullRequestsClient.Get(string,string,int)"/> method to return a pull request object.
    /// </summary>
    private void MockPRClient(bool throwNotFound = false)
    {
        if (throwNotFound)
        {
            this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Callback<string, string, int>((_, _, _) => throw new NotFoundException("not found", HttpStatusCode.NotFound));
        }
        else
        {
            var pullRequest = ObjectFactory.CreatePullRequest(SrcBranch, TargetBranch);
            this.mockPRClient.Setup(m => m.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(pullRequest);
        }
    }

    /// <summary>
    /// Creates a new instance of <see cref="PullRequestService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private PullRequestService CreateService()
        => new (this.mockRepoInfoReactable.Object,
            this.mockPRNumReactable.Object,
            this.mockClientFactory.Object);
}
