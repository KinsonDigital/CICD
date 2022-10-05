// <copyright file="GitHubActionsServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Services;
using FluentAssertions;
using Moq;
using Octokit;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="GitHubActionsService"/> class.
/// </summary>
public class GitHubActionsServiceTests
{
    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullRepoOwnerParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new GitHubActionsService(
                It.IsAny<int>(),
                null,
                "test-name",
                new Mock<ISecretService>().Object,
                new Mock<IExecutionContextService>().Object,
                new Mock<IGitRepoService>().Object,
                new Mock<IGitHubClient>().Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'repoOwner')");
    }

    [Fact]
    public void Ctor_WithNullRepoNameParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new GitHubActionsService(
                It.IsAny<int>(),
                "test-owner",
                null,
                new Mock<ISecretService>().Object,
                new Mock<IExecutionContextService>().Object,
                new Mock<IGitRepoService>().Object,
                new Mock<IGitHubClient>().Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'repoName')");
    }

    [Fact]
    public void Ctor_WithNullSecretServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new GitHubActionsService(
                It.IsAny<int>(),
                "test-owner",
                "test-name",
                null,
                new Mock<IExecutionContextService>().Object,
                new Mock<IGitRepoService>().Object,
                new Mock<IGitHubClient>().Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'secretService')");
    }

    [Fact]
    public void Ctor_WithNullExecutionContextServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new GitHubActionsService(
                It.IsAny<int>(),
                "test-owner",
                "test-name",
                new Mock<ISecretService>().Object,
                null,
                new Mock<IGitRepoService>().Object,
                new Mock<IGitHubClient>().Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'executionContextService')");
    }

    [Fact]
    public void Ctor_WithNullRepoServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new GitHubActionsService(
                It.IsAny<int>(),
                "test-owner",
                "test-name",
                new Mock<ISecretService>().Object,
                new Mock<IExecutionContextService>().Object,
                null,
                new Mock<IGitHubClient>().Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'repoService')");
    }

    [Fact]
    public void Ctor_WithNullGithubClientParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new GitHubActionsService(
                It.IsAny<int>(),
                "test-owner",
                "test-name",
                new Mock<ISecretService>().Object,
                new Mock<IExecutionContextService>().Object,
                new Mock<IGitRepoService>().Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'gitHubClient')");
    }
    #endregion
}
