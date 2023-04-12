// <copyright file="TweetServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.IO.Abstractions;
using CICDSystem;
using CICDSystem.Reactables.Core;
using CICDSystem.Services;
using CICDSystem.Services.Interfaces;
using FluentAssertions;
using Moq;
using Serilog;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="ReleaseTweetService"/> class.
/// </summary>
public class TweetServiceTests
{
    private readonly Mock<ISolutionService> mockSolutionService;
    private readonly Mock<ITwitterService> mockTwitterService;
    private readonly Mock<IConsoleLoggerService> mockConsoleLoggerService;
    private readonly Mock<IProjectService> mockProjectService;
    private readonly Mock<IFile> mockFile;
    private readonly Mock<IReactable<bool>> mockSkipTweetReactable;
    private readonly Mock<IReactable<(string, string)>> mockRepoInfoReactable;
    private readonly Mock<IReactable<TwitterSecrets>> mockSecretesReactable;
    private readonly Mock<ILogger> mockLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TweetServiceTests"/> class.
    /// </summary>
    public TweetServiceTests()
    {
        this.mockSolutionService = new Mock<ISolutionService>();
        this.mockSolutionService.SetupGet(p => p.Directory).Returns("C:/test-dir");

        this.mockTwitterService = new Mock<ITwitterService>();

        this.mockLogger = new Mock<ILogger>();

        this.mockConsoleLoggerService = new Mock<IConsoleLoggerService>();
        this.mockConsoleLoggerService.SetupGet(p => p.Logger).Returns(this.mockLogger.Object);

        this.mockProjectService = new Mock<IProjectService>();

        this.mockFile = new Mock<IFile>();
        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);

        this.mockRepoInfoReactable = new Mock<IReactable<(string, string)>>();
        this.mockSkipTweetReactable = new Mock<IReactable<bool>>();

        this.mockSecretesReactable = new Mock<IReactable<TwitterSecrets>>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullSolutionServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReleaseTweetService(
                null,
                this.mockTwitterService.Object,
                this.mockConsoleLoggerService.Object,
                this.mockProjectService.Object,
                this.mockFile.Object,
                this.mockRepoInfoReactable.Object,
                this.mockSkipTweetReactable.Object,
                this.mockSecretesReactable.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'solutionService')");
    }

    [Fact]
    public void Ctor_WithNullTwitterServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReleaseTweetService(
                this.mockSolutionService.Object,
                null,
                this.mockConsoleLoggerService.Object,
                this.mockProjectService.Object,
                this.mockFile.Object,
                this.mockRepoInfoReactable.Object,
                this.mockSkipTweetReactable.Object,
                this.mockSecretesReactable.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'twitterService')");
    }

    [Fact]
    public void Ctor_WithNullLoggerServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReleaseTweetService(
                this.mockSolutionService.Object,
                this.mockTwitterService.Object,
                null,
                this.mockProjectService.Object,
                this.mockFile.Object,
                this.mockRepoInfoReactable.Object,
                this.mockSkipTweetReactable.Object,
                this.mockSecretesReactable.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'loggerService')");
    }

    [Fact]
    public void Ctor_WithNullProjectServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReleaseTweetService(
                this.mockSolutionService.Object,
                this.mockTwitterService.Object,
                this.mockConsoleLoggerService.Object,
                null,
                this.mockFile.Object,
                this.mockRepoInfoReactable.Object,
                this.mockSkipTweetReactable.Object,
                this.mockSecretesReactable.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'projectService')");
    }

    [Fact]
    public void Ctor_WithNullFileParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReleaseTweetService(
                this.mockSolutionService.Object,
                this.mockTwitterService.Object,
                this.mockConsoleLoggerService.Object,
                this.mockProjectService.Object,
                null,
                this.mockRepoInfoReactable.Object,
                this.mockSkipTweetReactable.Object,
                this.mockSecretesReactable.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'file')");
    }

    [Fact]
    public void Ctor_WithNullRepoInfoReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReleaseTweetService(
                this.mockSolutionService.Object,
                this.mockTwitterService.Object,
                this.mockConsoleLoggerService.Object,
                this.mockProjectService.Object,
                this.mockFile.Object,
                null,
                this.mockSkipTweetReactable.Object,
                this.mockSecretesReactable.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'repoInfoReactable')");
    }

    [Fact]
    public void Ctor_WithNullSkipReleaseTweetReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReleaseTweetService(
                this.mockSolutionService.Object,
                this.mockTwitterService.Object,
                this.mockConsoleLoggerService.Object,
                this.mockProjectService.Object,
                this.mockFile.Object,
                this.mockRepoInfoReactable.Object,
                null,
                this.mockSecretesReactable.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'skipReleaseTweetReactable')");
    }

    [Fact]
    public void Ctor_WithNullSecretsReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReleaseTweetService(
                this.mockSolutionService.Object,
                this.mockTwitterService.Object,
                this.mockConsoleLoggerService.Object,
                this.mockProjectService.Object,
                this.mockFile.Object,
                this.mockRepoInfoReactable.Object,
                this.mockSkipTweetReactable.Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'SecretsReactable')");
    }

    [Fact]
    public void Ctor_WhenRepoInfoSubscriptionOnCompleteIsInvoked_UnsubscribesFromReactable()
    {
        // Arrange
        var mockUnsubscriber = new Mock<IDisposable>();
        IReactor<(string, string)>? reactor = null;

        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj)
            .Returns<IReactor<(string, string)>>(_ => mockUnsubscriber.Object);

        _ = CreateService();

        // Act
        reactor.OnCompleted();
        reactor.OnCompleted();

        // Assert
        mockUnsubscriber.Verify(m => m.Dispose(), Times.Once);
    }

    [Fact]
    public void Ctor_WhenSkipReleaseTweetSubscriptionOnCompleteIsInvoked_UnsubscribesFromReactable()
    {
        // Arrange
        var mockUnsubscriber = new Mock<IDisposable>();
        IReactor<bool>? reactor = null;

        this.mockSkipTweetReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<bool>>()))
            .Callback<IReactor<bool>>(reactorObj => reactor = reactorObj)
            .Returns<IReactor<bool>>(_ => mockUnsubscriber.Object);

        _ = CreateService();

        // Act
        reactor.OnCompleted();
        reactor.OnCompleted();

        // Assert
        mockUnsubscriber.Verify(m => m.Dispose(), Times.Once);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void SendReleaseTweet_WithReleaseTweetSetToBeSkipped_LogsInfoMessage()
    {
        // Arrange
        const string expectedInfoMsg = "Release tweet set to be skipped.";

        IReactor<bool>? reactor = null;

        this.mockSkipTweetReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<bool>>()))
            .Callback<IReactor<bool>>(reactorObj => reactor = reactorObj);

        var sut = CreateService();
        reactor.OnNext(true);

        // Act
        sut.SendReleaseTweet();

        // Assert
        this.mockLogger.Verify(m => m.Information(expectedInfoMsg), Times.Once);
    }

    [Fact]
    public void SendReleaseTweet_WhenTweetTemplateDoesNotExist_LogsWarning()
    {
        // Arrange
        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(false);

        var sut = CreateService();

        // Act
        sut.SendReleaseTweet();

        // Assert
        this.mockLogger.Verify(m => m.Warning("Release tweet skipped.  No release template file found."));
    }

    [Fact]
    public void SendReleaseTweet_WhenTweetContentIsEmpty_LogsWarning()
    {
        // Arrange
        this.mockFile.Setup(m => m.ReadAllText(It.IsAny<string>())).Returns(string.Empty);

        var sut = CreateService();

        // Act
        sut.SendReleaseTweet();

        // Assert
        this.mockLogger.Verify(m => m.Warning("Release tweet not sent.  No release template content."));
    }

    [Fact]
    public void SendReleaseTweet_WhenInvoked_SendsReleaseTweet()
    {
        // Arrange
        const string projName = "test-project";
        const string repoName = "test-repo";
        const string version = "v1.2.3";
        var secrets = new TwitterSecrets()
        {
            TwitterConsumerApiKey = "test-api-key",
            TwitterConsumerApiSecret = "test-api-secret",
            TwitterAccessToken = "test-access-token",
            TwitterAccessTokenSecret = "test-access-token-secret",
        };
        var tweetTemplate = "{REPO_OWNER}";
        tweetTemplate += $"{Environment.NewLine}{{PROJECT_NAME}}";
        tweetTemplate += $"{Environment.NewLine}{{VERSION}}";

        var expectedTweetMessage = projName;
        expectedTweetMessage += $"{Environment.NewLine}{repoName}";
        expectedTweetMessage += $"{Environment.NewLine}{version.Remove(0, 1)}";

        IReactor<(string, string)>? repoInfoReactor = null;
        IReactor<TwitterSecrets>? secretsReactor = null;

        this.mockFile.Setup(m => m.ReadAllText(It.IsAny<string>())).Returns(tweetTemplate);

        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => repoInfoReactor = reactorObj);

        this.mockSecretesReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<TwitterSecrets>>()))
            .Callback<IReactor<TwitterSecrets>>(reactor => secretsReactor = reactor);

        this.mockProjectService.Setup(m => m.GetVersion())
            .Returns(version);

        var sut = CreateService();

        repoInfoReactor.OnNext((projName, repoName));
        secretsReactor.OnNext(secrets);

        // Act
        sut.SendReleaseTweet();

        // Assert
        this.mockFile.Verify(m => m.Exists("C:/test-dir/.github/ReleaseTweetTemplate.txt"), Times.Once);
        this.mockFile.Verify(m => m.ReadAllText("C:/test-dir/.github/ReleaseTweetTemplate.txt"), Times.Once);
        this.mockProjectService.Verify(m => m.GetVersion(), Times.Once);
        this.mockTwitterService.Verify(m => m.SendTweet(
            expectedTweetMessage,
            secrets.TwitterConsumerApiKey,
            secrets.TwitterConsumerApiSecret,
            secrets.TwitterAccessToken,
            secrets.TwitterAccessTokenSecret), Times.Once);
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="ReleaseTweetService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private ReleaseTweetService CreateService()
        => new (this.mockSolutionService.Object,
            this.mockTwitterService.Object,
            this.mockConsoleLoggerService.Object,
            this.mockProjectService.Object,
            this.mockFile.Object,
            this.mockRepoInfoReactable.Object,
            this.mockSkipTweetReactable.Object,
            this.mockSecretesReactable.Object);
}
