// <copyright file="GitHubClientServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Factories;
using CICDSystem.Services;
using FluentAssertions;
using Moq;
using Octokit;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="GitHubClientService"/> class.
/// </summary>
public class GitHubClientServiceTests
{
    private readonly Mock<IHttpClientFactory> mockHttpClientFactory;
    private readonly Mock<IGitHubTokenService> mockGitHubTokenService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubClientServiceTests"/> class.
    /// </summary>
    public GitHubClientServiceTests()
    {
        this.mockHttpClientFactory = new Mock<IHttpClientFactory>();
        this.mockGitHubTokenService = new Mock<IGitHubTokenService>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullClientFactoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new GitHubClientService(
                null,
                this.mockGitHubTokenService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'clientFactory')");
    }

    [Fact]
    public void Ctor_WithNullTokenServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new GitHubClientService(
                this.mockHttpClientFactory.Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'tokenService')");
    }
    #endregion

    #region Method Tests
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void GetClient_WithNullOrEmptyParam_ThrowsException(string productName)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.GetClient(productName);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'productName')");
    }

    [Fact]
    public void GetClient_WhenNoTokenIsLoaded_ThrowsException()
    {
        // Arrange
        var expectedMsg = "The token could not be loaded.  If running locally, check that a 'local-secrets.json' file exists in ";
        expectedMsg += "the '.github' folder with the correct 'GitHubApiToken' key and value.";
        expectedMsg += $"{Environment.NewLine}If running on the server, verify that the workflow is setting up and environment variable named ";
        expectedMsg += $"{Environment.NewLine}'GITHUB_TOKEN' with the token value.";

        this.mockGitHubTokenService.Setup(m => m.GetToken()).Returns(string.Empty);
        var service = CreateService();

        // Act
        var act = () => service.GetClient("test-product");

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage(expectedMsg);
    }

    [Fact]
    public void GetClient_WhenInvoked_CreatesGitHubClient()
    {
        // Arrange
        var mockGitHubClient = new Mock<IGitHubClient>();
        this.mockGitHubTokenService.Setup(m => m.GetToken()).Returns("test-token");
        this.mockHttpClientFactory.Setup(m => m.CreateGitHubClient(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(mockGitHubClient.Object);

        var service = CreateService();

        // Act
        var actualA = service.GetClient("test-product");
        var actualB = service.GetClient("test-product");

        // Assert
        this.mockGitHubTokenService.Verify(m => m.GetToken(), Times.AtLeastOnce);
        this.mockHttpClientFactory.Verify(m =>
            m.CreateGitHubClient("test-product", "test-token"), Times.AtLeastOnce);
        actualA.Should().BeSameAs(mockGitHubClient.Object);
        actualB.Should().BeSameAs(mockGitHubClient.Object);
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="GitHubClientService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private GitHubClientService CreateService()
        => new (this.mockHttpClientFactory.Object,
            this.mockGitHubTokenService.Object);
}
