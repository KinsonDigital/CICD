// <copyright file="TwitterService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using System.Diagnostics.CodeAnalysis;
using CICDSystem.Guards;
using CICDSystem.Reactables.Core;
using CICDSystem.Services.Interfaces;
using Nuke.Common.Tools.Twitter;

namespace CICDSystem.Services;

/// <inheritdoc/>
internal sealed class TwitterService : ITwitterService
{
    private readonly IDisposable unsubscriber;
    private TwitterSecrets secrets;

    /// <summary>
    /// Initializes a new instance of the <see cref="TwitterService"/> class.
    /// </summary>
    /// <param name="secretsReactable">Provides push notifications of Twitter secrets.</param>
    public TwitterService(IReactable<TwitterSecrets> secretsReactable)
    {
        EnsureThat.ParamIsNotNull(secretsReactable, nameof(secretsReactable));

        this.unsubscriber = secretsReactable.Subscribe(new Reactor<TwitterSecrets>(
            onNext: secretsData =>
            {
                if (!string.IsNullOrEmpty(secretsData.TwitterConsumerApiKey))
                {
                    this.secrets.TwitterConsumerApiKey = secretsData.TwitterConsumerApiKey;
                }

                if (!string.IsNullOrEmpty(secretsData.TwitterConsumerApiSecret))
                {
                    this.secrets.TwitterConsumerApiSecret = secretsData.TwitterConsumerApiSecret;
                }

                if (!string.IsNullOrEmpty(secretsData.TwitterAccessToken))
                {
                    this.secrets.TwitterAccessToken = secretsData.TwitterAccessToken;
                }

                if (!string.IsNullOrEmpty(secretsData.TwitterAccessTokenSecret))
                {
                    this.secrets.TwitterAccessTokenSecret = secretsData.TwitterAccessTokenSecret;
                }
            }, () =>
            {
                // If any of the secrets are null or empty, throw an exception
                if (this.secrets.AnyNullOrEmpty())
                {
                    throw new Exception("The Twitter API keys and/or secrets are null or empty.");
                }

                this.unsubscriber?.Dispose();
            }));
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public void SendTweet(string message) =>
        TwitterTasks.SendTweet(
            message,
            this.secrets.TwitterConsumerApiKey,
            this.secrets.TwitterConsumerApiSecret,
            this.secrets.TwitterAccessToken,
            this.secrets.TwitterAccessTokenSecret);
}
