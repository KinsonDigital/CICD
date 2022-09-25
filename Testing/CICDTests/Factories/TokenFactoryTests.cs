// <copyright file="TokenFactoryTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Factories;
using CICDSystem.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace CICDSystemTests.Factories;

/// <summary>
/// Tests the <see cref="TokenFactory"/> class.
/// </summary>
public class TokenFactoryTests
{
    private readonly Mock<ILoadSecretsService> mockSecretsService;
    private readonly Mock<IExecutionContextService> mockExecutionService;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenFactoryTests"/> class.
    /// </summary>
    public TokenFactoryTests()
    {
        this.mockSecretsService = new Mock<ILoadSecretsService>();
        this.mockExecutionService = new Mock<IExecutionContextService>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullSecretsServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new TokenFactory(null, this.mockExecutionService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'secretsService')");
    }

    [Fact]
    public void Ctor_WithNullExecutionContextServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new TokenFactory(this.mockSecretsService.Object, null);
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
        this.mockSecretsService.Setup(m => m.LoadSecret("GithubAPIToken"))
            .Returns("test-token");

        var factory = new TokenFactory(this.mockSecretsService.Object, this.mockExecutionService.Object);

        // Act
        var actual = factory.GetToken();

        // Assert
        actual.Should().Be("test-token");
    }
    #endregion
}
