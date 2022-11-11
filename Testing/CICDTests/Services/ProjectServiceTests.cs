// <copyright file="ProjectServiceTests.cs" company="KinsonDigital">
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
/// Tests the <see cref="ProjectService"/> class.
/// </summary>
public class ProjectServiceTests
{
    private readonly Mock<ISolutionWrapper> mockSolutionWrapper;
    private readonly Mock<IReactable<(string, string)>> mockRepoInfoReactable;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectServiceTests"/> class.
    /// </summary>
    public ProjectServiceTests()
    {
        this.mockSolutionWrapper = new Mock<ISolutionWrapper>();
        this.mockRepoInfoReactable = new Mock<IReactable<(string, string)>>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullSolutionWrapperParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ProjectService(
                null,
                this.mockRepoInfoReactable.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'solutionWrapper')");
    }

    [Fact]
    public void Ctor_WithNullRepoInfoReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ProjectService(
                this.mockSolutionWrapper.Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'repoInfoReactable')");
    }

    [Fact]
    public void Ctor_WhenRepoInfoReactableEndsNotifications_UnsubscribesFromReactable()
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
    #endregion

    #region Method Tests
    [Fact]
    public void GetVersion_WithNonExistentProject_ThrowsException()
    {
        // Arrange
        IReactor<(string, string)>? reactor = null;
        this.mockSolutionWrapper.Setup(m => m.GetProject(It.IsAny<string>()))
            .Returns(() => null);

        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj);

        var sut = CreateService();
        reactor.OnNext(("test-owner", "test-project"));

        // Act
        var act = () => sut.GetVersion();

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("The project 'test-project' could not be found.");
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="ProjectService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private ProjectService CreateService()
        => new (this.mockSolutionWrapper.Object, this.mockRepoInfoReactable.Object);
}
