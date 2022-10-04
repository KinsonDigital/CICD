// <copyright file="HttpClientFactoryTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Factories;
using CICDSystem.Reactables.Core;
using CICDSystem.Reactables.ReactableData;
using CICDSystem.Services;
using CICDSystemTests.Reactables.ReactableData;
using FluentAssertions;
using Moq;
using Xunit;

namespace CICDSystemTests.Factories;

/// <summary>
/// Tests the <see cref="HttpClientFactory"/> class.
/// </summary>
public class HttpClientFactoryTests
{
    private readonly Mock<IReactable<BuildInfoData>> mockBuildInfoReactable;
    private readonly Mock<IGitHubTokenService> mockTokenService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientFactoryTests"/> class.
    /// </summary>
    public HttpClientFactoryTests()
    {
        this.mockBuildInfoReactable = new Mock<IReactable<BuildInfoData>>();
        this.mockTokenService = new Mock<IGitHubTokenService>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullBuildInfoReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new HttpClientFactory(
                null,
                this.mockTokenService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'buildInfoReactable')");
    }

    [Fact]
    public void Ctor_WithNullTokenServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new HttpClientFactory(
                this.mockBuildInfoReactable.Object,
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
    public void CreateGitHubClient_WithNullOrEmptyProductName_ThrowsException(string productName)
    {
        // Arrange
        var sut = CreateFactory();

        // Act
        var act = () => sut.CreateGitHubClient();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The internal product name cannot be null or empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CreateGitHubClient_WithNullOrEmptyToken_ThrowsException(string token)
    {
        // Arrange
        IReactor<BuildInfoData>? reactor = null;
        this.mockBuildInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<BuildInfoData>>()))
            .Callback<IReactor<BuildInfoData>>(reactorObj => reactor = reactorObj);

        this.mockBuildInfoReactable.Setup(m => m.PushNotification(It.IsAny<BuildInfoData>()))
            .Callback<BuildInfoData>(data => reactor.OnNext(data));

        var buildInfoData = new BuildInfoData("test-owner", "test-repo-name", "test-project", "test-token");
        this.mockTokenService.Setup(m => m.GetToken()).Returns(token);
        var sut = CreateFactory();

        this.mockBuildInfoReactable.Object.PushNotification(buildInfoData);

        // Act
        var act = () => sut.CreateGitHubClient();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The token must not be null or empty.");
    }

    [Fact]
    public void CreateGitHubClient_WhenInvoked_DisposesOfUnsubscriber()
    {
        // Arrange
        IReactor<BuildInfoData>? reactor = null;

        this.mockBuildInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<BuildInfoData>>()))
            .Callback<IReactor<BuildInfoData>>(reactorObj => reactor = reactorObj);

        this.mockBuildInfoReactable.Setup(m => m.PushNotification(It.IsAny<BuildInfoData>()))
            .Callback<BuildInfoData>(data => reactor.OnNext(data));

        this.mockTokenService.Setup(m => m.GetToken()).Returns("test-token");

        var sut = CreateFactory();

        var buildInfoData = new BuildInfoData("test-owner", "test-repo-name", "test-project", "test-token");
        this.mockBuildInfoReactable.Object.PushNotification(buildInfoData);

        // Act
        var actual = sut.CreateGitHubClient();

        // Assert
        this.mockTokenService.Verify(m => m.GetToken(), Times.Once);
        actual.Should().NotBeNull();
        actual.Connection.CredentialStore.GetCredentials().Result.Password.Should().Be("test-token");
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="HttpClientFactory"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private HttpClientFactory CreateFactory()
        => new (this.mockBuildInfoReactable.Object, this.mockTokenService.Object);
}
