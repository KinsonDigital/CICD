// <copyright file="TweetService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using System.IO.Abstractions;
using CICDSystem.Guards;
using CICDSystem.Reactables.Core;
using CICDSystem.Services.Interfaces;
using Serilog;

namespace CICDSystem.Services;

/// <inheritdoc/>
internal sealed class TweetService : ITweetService
{
    private const string TemplateFileName = "ReleaseTweetTemplate.txt";
    private readonly ISolutionService solutionService;
    private readonly ITwitterService twitterService;
    private readonly ILogger logger;
    private readonly IProjectService projectService;
    private readonly IFile file;
    private readonly IDisposable repoInfoUnsubscriber;
    private readonly IDisposable skipReleaseUnsubscriber;
    private bool skipTweet;
    private string repoOwner = string.Empty;
    private string repoName = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="TweetService"/> class.
    /// </summary>
    /// <param name="solutionService">Provides information about a solution.</param>
    /// <param name="twitterService">Provides functionality with Twitter.</param>
    /// <param name="loggerService">Provides logging services.</param>
    /// <param name="projectService">Provides project services.</param>
    /// <param name="file">Manages files.</param>
    /// <param name="repoInfoReactable">Provides push notifications about repository information.</param>
    /// <param name="skipReleaseTweetReactable">Provides push notifications if release tweets should be skipped.</param>
    public TweetService(
        ISolutionService solutionService,
        ITwitterService twitterService,
        IConsoleLoggerService loggerService,
        IProjectService projectService,
        IFile file,
        IReactable<(string, string)> repoInfoReactable,
        IReactable<bool> skipReleaseTweetReactable)
    {
        EnsureThat.ParamIsNotNull(solutionService, nameof(solutionService));
        EnsureThat.ParamIsNotNull(twitterService, nameof(twitterService));
        EnsureThat.ParamIsNotNull(loggerService, nameof(loggerService));
        EnsureThat.ParamIsNotNull(projectService, nameof(projectService));
        EnsureThat.ParamIsNotNull(file, nameof(file));
        EnsureThat.ParamIsNotNull(repoInfoReactable, nameof(repoInfoReactable));
        EnsureThat.ParamIsNotNull(skipReleaseTweetReactable, nameof(skipReleaseTweetReactable));

        this.solutionService = solutionService;
        this.twitterService = twitterService;
        this.logger = loggerService.Logger;
        this.projectService = projectService;
        this.file = file;

        this.repoInfoUnsubscriber = repoInfoReactable.Subscribe(new Reactor<(string repoOwner, string repoName)>(
            onNext: repoInfo =>
            {
                this.repoOwner = repoInfo.repoOwner;
                this.repoName = repoInfo.repoName;
            },
            onCompleted: () => this.repoInfoUnsubscriber?.Dispose()));

        this.skipReleaseUnsubscriber = skipReleaseTweetReactable.Subscribe(new Reactor<bool>(
            onNext: skip =>
            {
                this.skipTweet = skip;
            },
            onCompleted: () => this.skipReleaseUnsubscriber?.Dispose()));
    }

    /// <inheritdoc/>
    public void SendReleaseTweet()
    {
        if (this.skipTweet)
        {
            this.logger.Information("Release tweet set to be skipped.");
            return;
        }

        var rootDir = this.solutionService.Directory;

        var templateFilePath = $"{rootDir}/.github/{TemplateFileName}";

        if (this.file.Exists(templateFilePath) is false)
        {
            this.logger.Warning("Release tweet skipped.  No release template file found.");
            return;
        }

        var tweetContent = this.file.ReadAllText(templateFilePath) ?? string.Empty;

        if (string.IsNullOrEmpty(tweetContent))
        {
            this.logger.Warning("Release tweet not sent.  No release template content.");
            return;
        }

        const string leftBracket = "{";
        const string rightBracket = "}";
        const string repoOwnerInjectionPoint = $"{leftBracket}REPO_OWNER{rightBracket}";
        const string projNameInjectionPoint = $"{leftBracket}PROJECT_NAME{rightBracket}";
        const string versionInjectionPoint = $"{leftBracket}VERSION{rightBracket}";

        var version = this.projectService.GetVersion();

        version = version.StartsWith("v")
            ? version.Remove(0, 1)
            : version;

        tweetContent = tweetContent.Replace(repoOwnerInjectionPoint, this.repoOwner);
        tweetContent = tweetContent.Replace(projNameInjectionPoint, this.repoName);
        tweetContent = tweetContent.Replace(versionInjectionPoint, version);

        this.twitterService.SendTweet(tweetContent);
    }
}
