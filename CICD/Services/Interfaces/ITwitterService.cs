// <copyright file="ITwitterService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

using System.Threading.Tasks;

/// <summary>
/// Provides Twitter functionality.
/// </summary>
public interface ITwitterService
{
    /// <summary>
    /// Sends a tweet to Twitter.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="consumerAPIKey">The consumer API key.</param>
    /// <param name="consumerAPISecret">The consumer API secret.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="accessTokenSecret">The access token secret.</param>
    /// <exception cref="Exception">
    ///     Thrown if an HTTP request other than <see cref="HttpStatusCode.OK"/> and <see cref="HttpStatusCode.Created"/>.
    /// </exception>
    public void SendTweet(string message, string consumerAPIKey, string consumerAPISecret, string accessToken, string accessTokenSecret);

    /// <summary>
    /// Asynchronously sends a tweet to Twitter.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="consumerAPIKey">The consumer API key.</param>
    /// <param name="consumerAPISecret">The consumer API secret.</param>
    /// <param name="accessToken">The access token.</param>
    /// <param name="accessTokenSecret">The access token secret.</param>
    /// <exception cref="Exception">
    ///     Thrown if an HTTP request other than <see cref="HttpStatusCode.OK"/> and <see cref="HttpStatusCode.Created"/>.
    /// </exception>
    public Task SendTweetAsync(string message, string consumerAPIKey, string consumerAPISecret, string accessToken, string accessTokenSecret);
}
