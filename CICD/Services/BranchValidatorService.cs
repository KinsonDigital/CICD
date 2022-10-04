// <copyright file="BranchValidatorService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Linq;
using CICDSystem.Factories;
using CICDSystem.Guards;
using CICDSystem.Reactables.Core;
using CICDSystem.Reactables.ReactableData;
using Octokit;

namespace CICDSystem.Services;

/// <inheritdoc/>
internal sealed class BranchValidatorService : IBranchValidatorService
{
    private readonly IHttpClientFactory clientFactory;
    private readonly IDisposable unsubscriber;
    private readonly IGitRepoService gitRepoService;
    private readonly List<bool> andValues = new ();
    private readonly List<List<bool>> orValues = new ();
    private string repoOwner = string.Empty;
    private string repoName = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="BranchValidatorService"/> class.
    /// </summary>
    /// <param name="clientFactory">Creates HTTP clients.</param>
    /// <param name="gitRepoService">Provides GIT repository information.</param>
    /// <param name="buildInfoReactable">Provides push notifications of build information.</param>
    public BranchValidatorService(
        IHttpClientFactory clientFactory,
        IGitRepoService gitRepoService,
        IReactable<BuildInfoData> buildInfoReactable)
    {
        EnsureThat.ParamIsNotNull(clientFactory, nameof(clientFactory));
        EnsureThat.ParamIsNotNull(gitRepoService, nameof(gitRepoService));
        EnsureThat.ParamIsNotNull(buildInfoReactable, nameof(buildInfoReactable));

        this.clientFactory = clientFactory;

        this.gitRepoService = gitRepoService;

        this.unsubscriber = buildInfoReactable.Subscribe(new Reactor<BuildInfoData>(
            onNext: data =>
            {
                this.repoOwner = data.RepoOwner;
                this.repoName = data.RepoName;
            },
            onCompleted: () => this.unsubscriber?.Dispose()));
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService ValidSyntax(string branch, string branchPattern)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsCorrectBranch(branchPattern));

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService ValidSyntax(string branch, string branchPattern, params Predicate<string>[] andPredicates)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsCorrectBranch(branchPattern));

        foreach (var andPredicate in andPredicates)
        {
            this.andValues.Add(andPredicate(branch));
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="branchType"/> is an invalid value.</exception>
    public IBranchValidatorService ValidSyntax(string branch, BranchType branchType)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        var isValidBranch = branchType switch
        {
            BranchType.Master => branch.IsMasterBranch(),
            BranchType.Develop => branch.IsDevelopBranch(),
            BranchType.Feature => branch.IsFeatureBranch(),
            BranchType.PreviewFeature => branch.IsPreviewFeatureBranch(),
            BranchType.Release => branch.IsReleaseBranch(),
            BranchType.Preview => branch.IsPreviewBranch(),
            BranchType.HotFix => branch.IsHotFixBranch(),
            BranchType.Other => true,
            _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, "The enumeration is out of range.")
        };

        this.andValues.Add(isValidBranch);

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="branchType"/> is an invalid value.</exception>
    public IBranchValidatorService ValidSyntax(string branch, BranchType branchType, params Predicate<string>[] andPredicates)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        var isValidBranch = branchType switch
        {
            BranchType.Master => branch.IsMasterBranch(),
            BranchType.Develop => branch.IsDevelopBranch(),
            BranchType.Feature => branch.IsFeatureBranch(),
            BranchType.PreviewFeature => branch.IsPreviewFeatureBranch(),
            BranchType.Release => branch.IsReleaseBranch(),
            BranchType.Preview => branch.IsPreviewBranch(),
            BranchType.HotFix => branch.IsHotFixBranch(),
            BranchType.Other => true,
            _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, "The enumeration is out of range.")
        };

        this.andValues.Add(isValidBranch);

        foreach (var andPredicate in andPredicates)
        {
            this.andValues.Add(andPredicate(branch));
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsMasterBranch(string branch)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch == "master");

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsMasterBranch(string branch, params Predicate<string>[] andPredicates)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch == "master");

        foreach (var andPredicate in andPredicates)
        {
            this.andValues.Add(andPredicate(branch));
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsDevelopBranch(string branch)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch == "develop");

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsDevelopBranch(string branch, params Predicate<string>[] andPredicates)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch == "develop");

        foreach (var andPredicate in andPredicates)
        {
            this.andValues.Add(andPredicate(branch));
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsFeatureBranch(string branch)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsFeatureBranch());

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsFeatureBranch(string branch, params Predicate<string>[] andPredicates)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsFeatureBranch());

        foreach (var andPredicate in andPredicates)
        {
            this.andValues.Add(andPredicate(branch));
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsPreviewBranch(string branch)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsPreviewBranch());

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsPreviewBranch(string branch, params Predicate<string>[] andPredicates)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsPreviewBranch());

        foreach (var andPredicate in andPredicates)
        {
            this.andValues.Add(andPredicate(branch));
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsPreviewFeatureBranch(string branch)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsPreviewFeatureBranch());

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsPreviewFeatureBranch(string branch, params Predicate<string>[] andPredicates)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsPreviewFeatureBranch());

        foreach (var andPredicate in andPredicates)
        {
            this.andValues.Add(andPredicate(branch));
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsReleaseBranch(string branch)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsReleaseBranch());

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsReleaseBranch(string branch, params Predicate<string>[] andPredicates)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsReleaseBranch());

        foreach (var andPredicate in andPredicates)
        {
            this.andValues.Add(andPredicate(branch));
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsHotFixBranch(string branch)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsHotFixBranch());

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public IBranchValidatorService IsHotFixBranch(string branch, params Predicate<string>[] andPredicates)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        this.andValues.Add(branch.IsHotFixBranch());

        foreach (var andPredicate in andPredicates)
        {
            this.andValues.Add(andPredicate(branch));
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="prNumber"/> is less than or equal to 0.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="branchType"/> is an invalid value.</exception>
    public IBranchValidatorService PRSourceBranchCorrect(int prNumber, BranchType branchType)
    {
        if (prNumber <= 0)
        {
            throw new ArgumentException("The parameter must be greater than 0.", nameof(prNumber));
        }

        var client = this.clientFactory.CreateGitHubClient();
        var prClient = client.PullRequest;

        try
        {
            var pullRequest = prClient.Get(this.repoOwner, this.repoName, prNumber).Result;

            var branch = pullRequest.Head.Ref;

            var isCorrect = branchType switch
            {
                BranchType.Master => branch.IsMasterBranch(),
                BranchType.Develop => branch.IsDevelopBranch(),
                BranchType.Feature => branch.IsFeatureBranch(),
                BranchType.PreviewFeature => branch.IsPreviewFeatureBranch(),
                BranchType.Release => branch.IsReleaseBranch(),
                BranchType.Preview => branch.IsPreviewBranch(),
                BranchType.HotFix => branch.IsHotFixBranch(),
                BranchType.Other => false,
                _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, "The enumeration is out of range.")
            };

            this.andValues.Add(isCorrect);
        }
        catch (NotFoundException)
        {
            this.andValues.Add(false);
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="prNumber"/> is less than or equal to 0.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="branchType"/> is an invalid value.</exception>
    public IBranchValidatorService PRSourceBranchCorrect(int prNumber, BranchType branchType, params Predicate<string>[] andPredicates)
    {
        if (prNumber <= 0)
        {
            throw new ArgumentException("The parameter must be greater than 0.", nameof(prNumber));
        }

        var client = this.clientFactory.CreateGitHubClient();
        var prClient = client.PullRequest;

        try
        {
            var pullRequest = prClient.Get(this.repoOwner, this.repoName, prNumber).Result;

            var branch = pullRequest.Head.Ref;

            var isCorrect = branchType switch
            {
                BranchType.Master => branch.IsMasterBranch(),
                BranchType.Develop => branch.IsDevelopBranch(),
                BranchType.Feature => branch.IsFeatureBranch(),
                BranchType.PreviewFeature => branch.IsPreviewFeatureBranch(),
                BranchType.Release => branch.IsReleaseBranch(),
                BranchType.Preview => branch.IsPreviewBranch(),
                BranchType.HotFix => branch.IsHotFixBranch(),
                BranchType.Other => false,
                _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, "The enumeration is out of range.")
            };

            this.andValues.Add(isCorrect);

            foreach (var andPredicate in andPredicates)
            {
                this.andValues.Add(andPredicate(branch));
            }
        }
        catch (NotFoundException)
        {
            this.andValues.Add(false);
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="prNumber"/> is less than or equal to 0.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="branchType"/> is an invalid value.</exception>
    public IBranchValidatorService PRTargetBranchCorrect(int prNumber, BranchType branchType)
    {
        if (prNumber <= 0)
        {
            throw new ArgumentException("The parameter must be greater than 0.", nameof(prNumber));
        }

        var client = this.clientFactory.CreateGitHubClient();
        var prClient = client.PullRequest;

        try
        {
            var pullRequest = prClient.Get(this.repoOwner, this.repoName, prNumber).Result;

            var branch = pullRequest.Base.Ref;

            var isCorrect = branchType switch
            {
                BranchType.Master => branch.IsMasterBranch(),
                BranchType.Develop => branch.IsDevelopBranch(),
                BranchType.Feature => branch.IsFeatureBranch(),
                BranchType.PreviewFeature => branch.IsPreviewFeatureBranch(),
                BranchType.Release => branch.IsReleaseBranch(),
                BranchType.Preview => branch.IsPreviewBranch(),
                BranchType.HotFix => branch.IsHotFixBranch(),
                BranchType.Other => false,
                _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, "The enumeration is out of range.")
            };

            this.andValues.Add(isCorrect);
        }
        catch (NotFoundException)
        {
            this.andValues.Add(false);
        }

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="prNumber"/> is less than or equal to 0.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="branchType"/> is an invalid value.</exception>
    public IBranchValidatorService PRTargetBranchCorrect(int prNumber, BranchType branchType, params Predicate<string>[] andPredicates)
    {
        if (prNumber <= 0)
        {
            throw new ArgumentException("The parameter must be greater than 0.", nameof(prNumber));
        }

        var client = this.clientFactory.CreateGitHubClient();
        var prClient = client.PullRequest;

        try
        {
            var pullRequest = prClient.Get(this.repoOwner, this.repoName, prNumber).Result;

            var branch = pullRequest.Base.Ref;

            var isCorrect = branchType switch
            {
                BranchType.Master => branch.IsMasterBranch(),
                BranchType.Develop => branch.IsDevelopBranch(),
                BranchType.Feature => branch.IsFeatureBranch(),
                BranchType.PreviewFeature => branch.IsPreviewFeatureBranch(),
                BranchType.Release => branch.IsReleaseBranch(),
                BranchType.Preview => branch.IsPreviewBranch(),
                BranchType.HotFix => branch.IsHotFixBranch(),
                BranchType.Other => false,
                _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, "The enumeration is out of range.")
            };

            this.andValues.Add(isCorrect);

            foreach (var andPredicate in andPredicates)
            {
                this.andValues.Add(andPredicate(branch));
            }
        }
        catch (NotFoundException)
        {
            this.andValues.Add(false);
        }

        return this;
    }

    /// <inheritdoc/>
    public IBranchValidatorService Or()
    {
        if (this.andValues.Count <= 0)
        {
            return this;
        }

        var listToAdd = new bool[this.andValues.Count];
        this.andValues.CopyTo(listToAdd);

        this.orValues.Add(listToAdd.ToList());

        this.andValues.Clear();

        return this;
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty..</exception>
    /// <exception cref="Exception">Thrown if the issue number embedded in the <paramref name="branch"/> does not exist.</exception>
    public bool BranchIssueNumberExists(string branch)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        var issueClient = this.clientFactory.CreateGitHubClient().Issue;

        var branchType = GetBranchType(branch);

        var issueNumber = branchType switch
        {
            BranchType.Feature => int.Parse(branch.Split('/')[1].Split('-')[0]), // feature/123-my-branch
            BranchType.PreviewFeature => int.Parse(branch.Split('/')[2].Split('-')[0]), // preview/feature/123-my-branch
            BranchType.HotFix => int.Parse(branch.Split('/')[1].Split('-')[0]), // hotfix/123-my-fix
            _ => -1,
        };

        if (issueNumber is -1)
        {
            throw new Exception($"The branch '{branch}' is not the correct branch that contains an issue number.");
        }

        try
        {
            _ = issueClient.Get(this.repoOwner, this.repoName, issueNumber).Result;

            return true;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException">Thrown if the <paramref name="branch"/> is null or empty.</exception>
    public BranchType GetBranchType(string branch)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(branch, nameof(branch));

        if (branch.IsDevelopBranch())
        {
            return BranchType.Develop;
        }

        if (branch.IsMasterBranch())
        {
            return BranchType.Master;
        }

        if (branch.IsFeatureBranch())
        {
            return BranchType.Feature;
        }

        if (branch.IsPreviewFeatureBranch())
        {
            return BranchType.PreviewFeature;
        }

        if (branch.IsPreviewBranch())
        {
            return BranchType.Preview;
        }

        if (branch.IsReleaseBranch())
        {
            return BranchType.Release;
        }

        if (branch.IsHotFixBranch())
        {
            return BranchType.HotFix;
        }

        return BranchType.Other;
    }

    /// <inheritdoc/>
    /// <exception cref="Exception">Thrown if the currently checked out GIT branch is null or empty..</exception>
    public bool CurrentBranchIsValid(string branchPattern)
    {
        var branch = this.gitRepoService.Branch;

        if (string.IsNullOrEmpty(branch))
        {
            throw new Exception("Could not detect the current repository branch.  Does a repository exist?");
        }

        return branch.IsCorrectBranch(branchPattern);
    }

    /// <inheritdoc/>
    /// <exception cref="Exception">Thrown if the currently checked out GIT branch is null or empty..</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="branchType"/> is an invalid value.</exception>
    public bool CurrentBranchIsValid(BranchType branchType)
    {
        var branch = this.gitRepoService.Branch;

        if (string.IsNullOrEmpty(branch))
        {
            throw new Exception("Could not detect the current repository branch.  Does a repository exist?");
        }

        var isValid = branchType switch
        {
            BranchType.Master => branch.IsMasterBranch(),
            BranchType.Develop => branch.IsDevelopBranch(),
            BranchType.Feature => branch.IsFeatureBranch(),
            BranchType.PreviewFeature => branch.IsPreviewFeatureBranch(),
            BranchType.Release => branch.IsReleaseBranch(),
            BranchType.Preview => branch.IsPreviewBranch(),
            BranchType.HotFix => branch.IsHotFixBranch(),
            BranchType.Other => false,
            _ => throw new ArgumentOutOfRangeException(nameof(branchType), branchType, null)
        };

        return isValid;
    }

    /// <inheritdoc/>
    public IBranchValidatorService Reset()
    {
        this.andValues.Clear();
        this.orValues.Clear();

        return this;
    }

    /// <inheritdoc/>
    public bool GetValue()
    {
        if (this.andValues.Count <= 0 && this.orValues.Count <= 0)
        {
            return false;
        }

        if (this.andValues.Count <= 0 && this.orValues.Count > 0)
        {
            return ResolveOrValues();
        }

        if (this.andValues.Count > 0 && this.orValues.Count <= 0)
        {
            return ResolveAndValues();
        }

        var andResult = ResolveAndValues();
        var orResult = ResolveOrValues();

        this.andValues.Clear();
        this.orValues.Clear();

        return andResult || orResult;
    }

    /// <summary>
    /// Resolves all of the collected <c>and</c> values.
    /// </summary>
    /// <returns>A single result of all the <c>and</c> values.</returns>
    private bool ResolveAndValues() => this.andValues.Count > 0 && this.andValues.All(v => v);

    /// <summary>
    /// Resolves all of the <c>or</c> values.
    /// </summary>
    /// <returns>A single result of all the <c>or</c> values.</returns>
    /// <remarks>
    ///     The <c>or</c> values are a list of <c>and</c> value lists.  Each list is resolved first into a single
    ///     <c>bool</c> result and then all of the resolved <c>and</c> results are then resolved in an <c>or</c> manner.
    /// </remarks>
    private bool ResolveOrValues()
    {
        var resultsOfSets = this.orValues.Select(orSet => orSet.Count > 0 && orSet.All(i => i)).ToList();

        return resultsOfSets.Count > 0 && resultsOfSets.Any(v => v);
    }
}
