// <copyright file="RepoInfoReactableTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables;
using CICDSystem.Reactables.Core;
using Moq;
using Xunit;

namespace CICDSystemTests.Reactables;

/// <summary>
/// Tests the <see cref="RepoInfoReactable"/> class.
/// </summary>
public class RepoInfoReactableTests
{
    #region Method Tests
    [Fact]
    public void PushNotification_WhenInvoked_SendsPushNotification()
    {
        // Arrange
        var reactor = new Mock<IReactor<(string, string)>>();
        var expected = ("test-owner", "test-repo-name");

        var reactable = new RepoInfoReactable();
        reactable.Subscribe(reactor.Object);

        // Act
        reactable.PushNotification(expected);

        // Assert
        reactor.Verify(m => m.OnNext(expected), Times.Once());
    }
    #endregion
}
