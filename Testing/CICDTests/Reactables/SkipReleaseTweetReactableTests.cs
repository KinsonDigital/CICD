// <copyright file="SkipReleaseTweetReactableTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables;
using CICDSystem.Reactables.Core;
using Moq;
using Xunit;

namespace CICDSystemTests.Reactables;

/// <summary>
/// Tests the <see cref="SkipReleaseTweetReactable"/> class.
/// </summary>
public class SkipReleaseTweetReactableTests
{
    #region Method Tests
    [Fact]
    public void PushNotification_WhenInvoked_SendsPushNotification()
    {
        // Arrange
        var reactor = new Mock<IReactor<bool>>();
        const bool expected = true;

        var reactable = new SkipReleaseTweetReactable();
        reactable.Subscribe(reactor.Object);

        // Act
        reactable.PushNotification(expected);

        // Assert
        reactor.Verify(m => m.OnNext(expected), Times.Once());
    }
    #endregion
}
