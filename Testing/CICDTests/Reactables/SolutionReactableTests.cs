// <copyright file="SolutionReactableTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables;
using CICDSystem.Reactables.Core;
using Moq;
using Nuke.Common.ProjectModel;
using Xunit;

namespace CICDSystemTests.Reactables;

/// <summary>
/// Tests the <see cref="SolutionReactable"/> class.
/// </summary>
public class SolutionReactableTests
{
    #region Method Tests
    [Fact]
    public void PushNotification_WhenInvoked_SendsPushNotification()
    {
        // Arrange
        var reactor = new Mock<IReactor<Solution>>();
        var expected = new Solution();

        var reactable = new SolutionReactable();
        reactable.Subscribe(reactor.Object);

        // Act
        reactable.PushNotification(expected);

        // Assert
        reactor.Verify(m => m.OnNext(expected), Times.Once());
    }
    #endregion
}
