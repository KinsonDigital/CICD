// <copyright file="HttpClientFactoryTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Factories;
using CICDSystem.Reactables.Core;
using CICDSystem.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CICDSystemTests.Factories;

/// <summary>
/// Tests the <see cref="HttpClientFactory"/> class.
/// </summary>
public class HttpClientFactoryTests
{
    private readonly Mock<IReactable<string>> mockProductNameReactable;
    private readonly Mock<IGitHubTokenService> mockTokenService;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpClientFactoryTests"/> class.
    /// </summary>
    public HttpClientFactoryTests()
    {
        this.mockProductNameReactable = new Mock<IReactable<string>>();
        this.mockTokenService = new Mock<IGitHubTokenService>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullRepoInfoReactableParam_ThrowsException()
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
            .WithMessage("The parameter must not be null. (Parameter 'productNamReactable')");
    }

    [Fact]
    public void Ctor_WithNullTokenServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new HttpClientFactory(
                this.mockProductNameReactable.Object,
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
        IReactor<string>? reactor = null;
        this.mockProductNameReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<string>>()))
            .Callback<IReactor<string>>(reactorObj => reactor = reactorObj);

        this.mockProductNameReactable.Setup(m => m.PushNotification(It.IsAny<string>()))
            .Callback<string>(data => reactor.OnNext(data));

        var sut = CreateFactory();
        this.mockProductNameReactable.Object.PushNotification(productName);

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
        IReactor<string>? reactor = null;
        this.mockProductNameReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<string>>()))
            .Callback<IReactor<string>>(reactorObj => reactor = reactorObj);

        this.mockProductNameReactable.Setup(m => m.PushNotification(It.IsAny<string>()))
            .Callback<string>(data => reactor.OnNext(data));

        var data = "test-token";
        this.mockTokenService.Setup(m => m.GetToken()).Returns(token);
        var sut = CreateFactory();

        this.mockProductNameReactable.Object.PushNotification(data);

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
        IReactor<string>? reactor = null;

        this.mockProductNameReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<string>>()))
            .Callback<IReactor<string>>(reactorObj => reactor = reactorObj);

        this.mockProductNameReactable.Setup(m => m.PushNotification(It.IsAny<string>()))
            .Callback<string>(data => reactor.OnNext(data));

        this.mockTokenService.Setup(m => m.GetToken()).Returns("test-token");

        var sut = CreateFactory();

        var data = "test-token";
        this.mockProductNameReactable.Object.PushNotification(data);

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
        => new (this.mockProductNameReactable.Object, this.mockTokenService.Object);
}
