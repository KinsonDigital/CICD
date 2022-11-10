// <copyright file="ITweetService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Provides tweet related functionality.
/// </summary>
internal interface ITweetService
{
    /// <summary>
    /// Sends a release tweet.
    /// </summary>
    void SendReleaseTweet();
}
