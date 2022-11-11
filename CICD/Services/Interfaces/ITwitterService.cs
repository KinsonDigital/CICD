// <copyright file="ITwitterService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Provides Twitter functionality.
/// </summary>
public interface ITwitterService
{
    /// <summary>
    /// Creates a tweet using the given <paramref name="message"/>.
    /// </summary>
    /// <param name="message">The message to tweet.</param>
    void SendTweet(string message);
}
