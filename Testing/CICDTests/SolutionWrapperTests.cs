// <copyright file="SolutionWrapperTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem;
using CICDSystem.Reactables.Core;
using FluentAssertions;
using Moq;
using Nuke.Common.ProjectModel;
using Xunit;

namespace CICDSystemTests;

/// <summary>
/// Tests the <see cref="SolutionWrapper"/> class.
/// </summary>
public class SolutionWrapperTests
{
    private readonly Mock<IReactable<Solution>> mockSolutionReactable;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolutionWrapperTests"/> class.
    /// </summary>
    public SolutionWrapperTests() => this.mockSolutionReactable = new Mock<IReactable<Solution>>();

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullSolutionReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new SolutionWrapper(null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'solutionReactable')");
    }

    [Fact]
    public void Ctor_WhenInvoked_SubscribesToReactable()
    {
        // Arrange
        IReactor<Solution>? reactor = null;

        this.mockSolutionReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<Solution>>()))
            .Callback<IReactor<Solution>>(reactorObj =>
            {
                reactorObj.Should().NotBeNull("this is required to test the subscription method.");
                reactor = reactorObj;
            });

        // Act
        _ = new SolutionWrapper(this.mockSolutionReactable.Object);
        reactor.OnNext(new Solution());

        // Assert
        this.mockSolutionReactable.Verify(m => m.Subscribe(It.IsAny<IReactor<Solution>>()), Times.Once);
    }

    [Fact]
    public void Ctor_WhenEndNotificationsIsInvoked_UnsubscribesFromReactable()
    {
        // Arrange
        var mockUnsubscriber = new Mock<IDisposable>();
        IReactor<Solution>? reactor = null;

        this.mockSolutionReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<Solution>>()))
            .Returns(() => mockUnsubscriber.Object)
            .Callback<IReactor<Solution>>(reactorObj =>
            {
                reactorObj.Should().NotBeNull("this is required to test the subscription method.");
                reactor = reactorObj;
            });

        _ = new SolutionWrapper(this.mockSolutionReactable.Object);

        // Act
        reactor.OnCompleted();

        // Assert
        mockUnsubscriber.Verify(m => m.Dispose(), Times.Once);
    }
    #endregion
}
