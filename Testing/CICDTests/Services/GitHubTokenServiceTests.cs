// <copyright file="GitHubTokenServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Services;
using CICDSystem.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="GitHubTokenService"/> class.
/// </summary>
public class GitHubTokenServiceTests
{
    private readonly Mock<ISecretService> mockSecretService;
    private readonly Mock<IExecutionContextService> mockExecutionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubTokenServiceTests"/> class.
    /// </summary>
    public GitHubTokenServiceTests()
    {
        this.mockSecretService = new Mock<ISecretService>();
        this.mockExecutionService = new Mock<IExecutionContextService>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullSecretServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new GitHubTokenService(null, this.mockExecutionService.Object);
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
            _ = new GitHubTokenService(this.mockSecretService.Object, null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'executionContextService')");
    }
    #endregion

    #region Method Tests

    [Fact]
    public void GetToken_WhenInvokedInLocalExecutionContext_ReturnCorrectValue()
    {
        // Arrange
        this.mockSecretService.Setup(m => m.LoadSecret("GitHubApiToken"))
            .Returns("test-token");

        var factory = new GitHubTokenService(this.mockSecretService.Object, this.mockExecutionService.Object);

        // Act
        var actual = factory.GetToken();

        // Assert
        actual.Should().Be("test-token");
    }
    #endregion
}
