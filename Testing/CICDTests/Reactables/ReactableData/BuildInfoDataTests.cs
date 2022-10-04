// <copyright file="BuildInfoDataTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables.ReactableData;
using FluentAssertions;
using Xunit;

namespace CICDSystemTests.Reactables.ReactableData;

/// <summary>
/// Tests the <see cref="BuildInfoData"/> class.
/// </summary>
public class BuildInfoDataTests
{
    #region Constructor Tests
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Ctor_WithNullRepoOwnerParam_ThrowsException(string value)
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new BuildInfoData(
                value,
                nameof(BuildInfoData.RepoName),
                nameof(BuildInfoData.ProjectName),
                nameof(BuildInfoData.Token));
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'repoOwner')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Ctor_WithNullRepoNameParam_ThrowsException(string value)
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new BuildInfoData(
                nameof(BuildInfoData.RepoOwner),
                value,
                nameof(BuildInfoData.ProjectName),
                nameof(BuildInfoData.Token));
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'repoName')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Ctor_WithNullProjectNameParam_ThrowsException(string value)
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new BuildInfoData(
                nameof(BuildInfoData.RepoOwner),
                nameof(BuildInfoData.RepoName),
                value,
                nameof(BuildInfoData.Token));
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'projectName')");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Ctor_WithNullTokenParam_ThrowsException(string value)
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new BuildInfoData(
                nameof(BuildInfoData.RepoOwner),
                nameof(BuildInfoData.RepoName),
                nameof(BuildInfoData.ProjectName),
                value);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'token')");
    }

    [Fact]
    public void Ctor_WhenInvoked_SetsProperties()
    {
        // Arrange & Act
        var sut = new BuildInfoData(
            nameof(BuildInfoData.RepoOwner),
            nameof(BuildInfoData.RepoName),
            nameof(BuildInfoData.ProjectName),
            nameof(BuildInfoData.Token));

        // Assert
        sut.RepoOwner.Should().Be(nameof(BuildInfoData.RepoOwner));
        sut.RepoName.Should().Be(nameof(BuildInfoData.RepoName));
        sut.ProjectName.Should().Be(nameof(BuildInfoData.ProjectName));
        sut.Token.Should().Be(nameof(BuildInfoData.Token));
    }
    #endregion
}
