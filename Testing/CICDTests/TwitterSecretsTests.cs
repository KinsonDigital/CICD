// <copyright file="TwitterSecretsTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem;
using FluentAssertions;
using Xunit;

namespace CICDSystemTests;

/// <summary>
/// Tests the <see cref="TwitterSecrets"/> class.
/// </summary>
public class TwitterSecretsTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WhenInvoked_SetsProperties()
    {
        // Arrange & Act
        var sut = new TwitterSecrets(
            "secret-1",
            "secret-2",
            "secret-3",
            "secret-4");

        // Assert
        sut.TwitterConsumerApiKey.Should().Be("secret-1");
        sut.TwitterConsumerApiSecret.Should().Be("secret-2");
        sut.TwitterAccessToken.Should().Be("secret-3");
        sut.TwitterAccessTokenSecret.Should().Be("secret-4");
    }
    #endregion

    #region Prop Tests
    [Fact]
    public void TwitterConsumerApiKey_WhenSettingValue_ReturnsCorrectResult()
    {
        // Arrange
        var sut = default(TwitterSecrets);

        // Act
        sut.TwitterConsumerApiKey = "test-value";

        // Assert
        sut.TwitterConsumerApiKey.Should().Be("test-value");
    }

    [Fact]
    public void TwitterConsumerApiSecret_WhenSettingValue_ReturnsCorrectResult()
    {
        // Arrange
        var sut = default(TwitterSecrets);

        // Act
        sut.TwitterConsumerApiSecret = "test-value";

        // Assert
        sut.TwitterConsumerApiSecret.Should().Be("test-value");
    }

    [Fact]
    public void TwitterAccessToken_WhenSettingValue_ReturnsCorrectResult()
    {
        // Arrange
        var sut = default(TwitterSecrets);

        // Act
        sut.TwitterAccessToken = "test-value";

        // Assert
        sut.TwitterAccessToken.Should().Be("test-value");
    }

    [Fact]
    public void TwitterAccessTokenSecret_WhenSettingValue_ReturnsCorrectResult()
    {
        // Arrange
        var sut = default(TwitterSecrets);

        // Act
        sut.TwitterAccessTokenSecret = "test-value";

        // Assert
        sut.TwitterAccessTokenSecret.Should().Be("test-value");
    }
    #endregion

    #region Method Tests
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AnyNullOrEmpty_WithNullOrEmptyTwitterConsumerApiKey_ReturnsTrue(string value)
    {
        // Arrange
        var sut = new TwitterSecrets(
            twitterConsumerApiKey: value,
            twitterConsumerApiSecret: "secret-2",
            twitterAccessToken: "secret-3",
            twitterAccessTokenSecret: "secret-4");

        // Act
        var actual = sut.AnyNullOrEmpty();

        // Assert
        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AnyNullOrEmpty_WithNullOrEmptyTwitterConsumerApiSecret_ReturnsTrue(string value)
    {
        // Arrange
        var sut = new TwitterSecrets(
            twitterConsumerApiKey: "secret-1",
            twitterConsumerApiSecret: value,
            twitterAccessToken: "secret-3",
            twitterAccessTokenSecret: "secret-4");

        // Act
        var actual = sut.AnyNullOrEmpty();

        // Assert
        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AnyNullOrEmpty_WithNullOrEmptyTwitterAccessToken_ReturnsTrue(string value)
    {
        // Arrange
        var sut = new TwitterSecrets(
            twitterConsumerApiKey: "secret-1",
            twitterConsumerApiSecret: "secret-2",
            twitterAccessToken: value,
            twitterAccessTokenSecret: "secret-4");

        // Act
        var actual = sut.AnyNullOrEmpty();

        // Assert
        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AnyNullOrEmpty_WithNullOrEmptyTwitterAccessTokenSecret_ReturnsTrue(string value)
    {
        // Arrange
        var sut = new TwitterSecrets(
            twitterConsumerApiKey: "secret-1",
            twitterConsumerApiSecret: "secret-2",
            twitterAccessToken: "secret-3",
            twitterAccessTokenSecret: value);

        // Act
        var actual = sut.AnyNullOrEmpty();

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void AnyNullOrEmpty_WithNoNullOrEmptySecrets_ReturnsFalse()
    {
        // Arrange
        var sut = new TwitterSecrets(
            twitterConsumerApiKey: "secret-1",
            twitterConsumerApiSecret: "secret-2",
            twitterAccessToken: "secret-3",
            twitterAccessTokenSecret: "secret-4");

        // Act
        var actual = sut.AnyNullOrEmpty();

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData("secret-1", true)]
    [InlineData(null, false)]
    [InlineData("", false)]
    public void AllSecretsSet_WhenInvoked_ReturnsCorrectResult(string value, bool expected)
    {
        // Arrange
        var sut = new TwitterSecrets(
            twitterConsumerApiKey: value,
            twitterConsumerApiSecret: "secret-2",
            twitterAccessToken: "secret-3",
            twitterAccessTokenSecret: "secret-4");

        // Act
        var actual = sut.AllSecretsSet();

        // Assert
        actual.Should().Be(expected);
    }
    #endregion
}
