// <copyright file="BuildInfoReactableTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables;
using CICDSystem.Reactables.Core;
using CICDSystem.Reactables.ReactableData;
using Moq;
using Xunit;

namespace CICDSystemTests.Reactables;

/// <summary>
/// Tests the <see cref="BuildInfoReactable"/> class.
/// </summary>
public class BuildInfoReactableTests
{
    #region Method Tests
    [Fact]
    public void PushNotification_WhenInvoked_SendsPushNotification()
    {
        // Arrange
        var reactor = new Mock<IReactor<BuildInfoData>>();

        var reactable = new BuildInfoReactable();
        reactable.Subscribe(reactor.Object);

        // Act
        var buildInfoData = new BuildInfoData("test-owner", "test-repo-name", "test-project", "test-token");
        reactable.PushNotification(buildInfoData);

        // Assert
        reactor.Verify(m => m.OnNext(buildInfoData), Times.Once());
    }
    #endregion
}
