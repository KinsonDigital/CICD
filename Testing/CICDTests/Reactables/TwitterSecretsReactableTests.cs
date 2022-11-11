// <copyright file="TwitterSecretsReactableTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem;
using CICDSystem.Reactables;
using CICDSystem.Reactables.Core;
using Moq;
using Xunit;

namespace CICDSystemTests.Reactables;

/// <summary>
/// Tests the <see cref="TwitterSecretsReactable"/> class.
/// </summary>
public class TwitterSecretsReactableTests
{
    #region Method Tests
    [Fact]
    public void PushNotification_WhenInvoked_SendsPushNotification()
    {
        // Arrange
        var reactor = new Mock<IReactor<TwitterSecrets>>();
        var expected = new TwitterSecrets("10", "20", "30", "40");

        var reactable = new TwitterSecretsReactable();
        reactable.Subscribe(reactor.Object);

        // Act
        reactable.PushNotification(expected);

        // Assert
        reactor.Verify(m => m.OnNext(expected), Times.Once());
    }
    #endregion
}
