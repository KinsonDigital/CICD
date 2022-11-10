// <copyright file="TwitterServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem;
using CICDSystem.Reactables.Core;
using CICDSystem.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="TwitterService"/> class.
/// </summary>
public class TwitterServiceTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullSecretsReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new TwitterService(null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'secretsReactable')");
    }

    [Fact]
    public void Ctor_WithNoNullOrEmptyKeysOrSecrets_DoesNotThrowsException()
    {
        // Arrange
        IReactor<TwitterSecrets>? reactor = null;
        var mockReactable = new Mock<IReactable<TwitterSecrets>>();

        mockReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<TwitterSecrets>>()))
            .Callback<IReactor<TwitterSecrets>>(reactorObj => reactor = reactorObj);

        _ = new TwitterService(mockReactable.Object);

        var secrets = new TwitterSecrets("10", "20", "30", "40");
        reactor.OnNext(secrets);

        // Act
        var act = () => reactor.OnCompleted();

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Ctor_WithNullOrEmptyConsumerApiKey_ThrowsException(string value)
    {
        // Arrange
        IReactor<TwitterSecrets>? reactor = null;
        var mockReactable = new Mock<IReactable<TwitterSecrets>>();

        mockReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<TwitterSecrets>>()))
            .Callback<IReactor<TwitterSecrets>>(reactorObj => reactor = reactorObj);

        _ = new TwitterService(mockReactable.Object);

        var secrets = new TwitterSecrets(
            value,
            "20",
            "30",
            "40");
        reactor.OnNext(secrets);

        // Act
        var act = () => reactor.OnCompleted();

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("The twitter API keys and/or secrets are null or empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Ctor_WithNullOrEmptyConsumerApiSecret_ThrowsException(string value)
    {
        // Arrange
        IReactor<TwitterSecrets>? reactor = null;
        var mockReactable = new Mock<IReactable<TwitterSecrets>>();

        mockReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<TwitterSecrets>>()))
            .Callback<IReactor<TwitterSecrets>>(reactorObj => reactor = reactorObj);

        _ = new TwitterService(mockReactable.Object);

        var secrets = new TwitterSecrets(
            "10",
            value,
            "30",
            "40");
        reactor.OnNext(secrets);

        // Act
        var act = () => reactor.OnCompleted();

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("The twitter API keys and/or secrets are null or empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Ctor_WithNullOrEmptyAccessToken_ThrowsException(string value)
    {
        // Arrange
        IReactor<TwitterSecrets>? reactor = null;
        var mockReactable = new Mock<IReactable<TwitterSecrets>>();

        mockReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<TwitterSecrets>>()))
            .Callback<IReactor<TwitterSecrets>>(reactorObj => reactor = reactorObj);

        _ = new TwitterService(mockReactable.Object);

        var secrets = new TwitterSecrets(
            "10",
            "20",
            value,
            "40");
        reactor.OnNext(secrets);

        // Act
        var act = () => reactor.OnCompleted();

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("The twitter API keys and/or secrets are null or empty.");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Ctor_WithNullOrEmptyAccessTokenSecret_ThrowsException(string value)
    {
        // Arrange
        IReactor<TwitterSecrets>? reactor = null;
        var mockReactable = new Mock<IReactable<TwitterSecrets>>();

        mockReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<TwitterSecrets>>()))
            .Callback<IReactor<TwitterSecrets>>(reactorObj => reactor = reactorObj);

        _ = new TwitterService(mockReactable.Object);

        var secrets = new TwitterSecrets(
            "10",
            "20",
            "30",
            value);
        reactor.OnNext(secrets);

        // Act
        var act = () => reactor.OnCompleted();

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("The twitter API keys and/or secrets are null or empty.");
    }

    [Fact]
    public void Ctor_WhenOnCompletionIsInvoked_UnsubscribesFromReactable()
    {
        // Arrange
        var mockUnsubscriber = new Mock<IDisposable>();

        IReactor<TwitterSecrets>? reactor = null;
        var mockReactable = new Mock<IReactable<TwitterSecrets>>();

        mockReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<TwitterSecrets>>()))
            .Returns<IReactor<TwitterSecrets>>(_ => mockUnsubscriber.Object)
            .Callback<IReactor<TwitterSecrets>>(reactorObj => reactor = reactorObj);

        _ = new TwitterService(mockReactable.Object);

        var secrets = new TwitterSecrets(
            "10",
            "20",
            "30",
            "40");
        reactor.OnNext(secrets);

        // Act
        reactor.OnCompleted();

        // Assert
        mockUnsubscriber.Verify(m => m.Dispose());
    }
    #endregion
}
