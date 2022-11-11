// <copyright file="TwitterSecrets.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem;

/// <summary>
/// Holds all of the secrets required to send out a tweet.
/// </summary>
public struct TwitterSecrets
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TwitterSecrets"/> struct.
    /// </summary>
    /// <param name="twitterConsumerApiKey">The Twitter consumer API key.  Essentially the Twitter username.</param>
    /// <param name="twitterConsumerApiSecret">The Twitter consumer API secret.  Essentially the Twitter password.</param>
    /// <param name="twitterAccessToken">The Twitter access token.</param>
    /// <param name="twitterAccessTokenSecret">The Twitter access token secret.</param>
    public TwitterSecrets(
        string twitterConsumerApiKey = "",
        string twitterConsumerApiSecret = "",
        string twitterAccessToken = "",
        string twitterAccessTokenSecret = "")
    {
        TwitterConsumerApiKey = twitterConsumerApiKey;
        TwitterConsumerApiSecret = twitterConsumerApiSecret;
        TwitterAccessToken = twitterAccessToken;
        TwitterAccessTokenSecret = twitterAccessTokenSecret;
    }

    /// <summary>
    /// Gets or sets the Twitter consumer API key.
    /// </summary>
    public string TwitterConsumerApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Twitter consumer API secret.
    /// </summary>
    public string TwitterConsumerApiSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Twitter access token.
    /// </summary>
    public string TwitterAccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Twitter access token secret.
    /// </summary>
    public string TwitterAccessTokenSecret { get; set; } = string.Empty;

    /// <summary>
    /// Returns a value indicating whether or not any of the secrets or tokens are null or empty.
    /// </summary>
    /// <returns><c>true</c> if anything is null or empty.</returns>
    public bool AnyNullOrEmpty() =>
        string.IsNullOrEmpty(TwitterConsumerApiKey) ||
        string.IsNullOrEmpty(TwitterConsumerApiSecret) ||
        string.IsNullOrEmpty(TwitterAccessToken) ||
        string.IsNullOrEmpty(TwitterAccessTokenSecret);

    /// <summary>
    /// Returns a value indicating whether or not all of the secrets are set.
    /// </summary>
    /// <returns><c>true</c> if anything is null or empty.</returns>
    public bool AllSecretsSet() =>
        !string.IsNullOrEmpty(TwitterConsumerApiKey) &&
        !string.IsNullOrEmpty(TwitterConsumerApiSecret) &&
        !string.IsNullOrEmpty(TwitterAccessToken) &&
        !string.IsNullOrEmpty(TwitterAccessTokenSecret);
}
