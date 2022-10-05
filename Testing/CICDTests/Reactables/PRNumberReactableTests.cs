// <copyright file="PRNumberReactableTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables;
using CICDSystem.Reactables.Core;
using Moq;
using Xunit;

namespace CICDSystemTests.Reactables;

/// <summary>
/// Tests the <see cref="ProductNameReactable"/> class.
/// </summary>
public class PRNumberReactableTests
{
    #region Method Tests
    [Fact]
    public void PushNotification_WhenInvoked_SendsPushNotification()
    {
        // Arrange
        var reactor = new Mock<IReactor<int>>();
        const int expected = 123;

        var reactable = new PRNumberReactable();
        reactable.Subscribe(reactor.Object);

        // Act
        reactable.PushNotification(expected);

        // Assert
        reactor.Verify(m => m.OnNext(expected), Times.Once());
    }
    #endregion
}
