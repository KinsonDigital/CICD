// <copyright file="ObjectFactory.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using Octokit;

namespace CICDSystemTests.Helpers;

/// <summary>
/// Creates objects to assist in unit testing.
/// </summary>
public static class ObjectFactory
{
    /// <summary>
    /// Creates a <see cref="PullRequest"/> object for the purpose of testing and mocking.
    /// </summary>
    /// <param name="sourceBranch">The source branch of the pull request.</param>
    /// <param name="targetBranch">The target branch of the pull request.</param>
    /// <returns>The pull request object to help with testing.</returns>
    public static PullRequest CreatePullRequest(string sourceBranch, string targetBranch)
    {
#pragma warning disable SA1117
        var head = new GitReference(
            string.Empty, string.Empty, string.Empty, sourceBranch,
            string.Empty, new User(), new Repository());
        var @base = new GitReference(
            string.Empty, string.Empty, string.Empty, targetBranch,
            string.Empty, new User(), new Repository());
#pragma warning restore SA1117

        var result = new PullRequest(
            123, // id: long
            string.Empty, // nodeId: string
            string.Empty, // url: string
            string.Empty, // htmlUrl: string
            string.Empty, // diffUrl: string
            string.Empty, // patchUrl: string
            string.Empty, // issueUrl: string
            string.Empty, // statusesUrl: string
            0,  // number: int
            ItemState.Open, // state: ItemState
            "Test Pull Request", // title: string
            "test-body", // body: string
            DateTime.Now, // createdAt: DateTimeOffset
            DateTime.Now, // updatedAt: DateTimeOffset
            DateTime.Now, // closedAt: DateTimeOffset
            DateTime.Now, // mergedAt: DateTimeOffset
            head, // head: GitReference
            @base, // @base: GitReference
            new User(), // user: User
            new User(), // assignee: User
            new ReadOnlyCollection<User>(Array.Empty<User>()), // assignees: IReadOnlyList<User>
            false, // draft: bool
            true, // mergeable: bool
            MergeableState.Clean, // mergeableState: MergeableState
            new User(), // mergedBy: User
            string.Empty, // mergeCommitSha: string
            1, // comments: int
            1, // commits: int
            1, // additions: int
            1, // deletions: int
            1, // changedFiles: int
            new Milestone(), // milestone: Milestone
            false, // locked: bool
            true, // maintainerCanModify: bool
            new ReadOnlyCollection<User>(Array.Empty<User>()), // requestedReviewers:IReadOnlyList<User>
            new ReadOnlyCollection<Team>(Array.Empty<Team>()), // requestedTeams:IReadOnlyList<Team>
            new ReadOnlyCollection<Label>(Array.Empty<Label>()), // labels:IReadOnlyList<Label>
            LockReason.Resolved); // activeLockReason: LockReason

        return result;
    }

    /// <summary>
    /// Creates an <see cref="Issue"/> object for the purpose of testing and mocking.
    /// </summary>
    /// <returns>The pull request object to help with testing.</returns>
    public static Issue CreateIssue()
    {
        return new Issue(
            url: string.Empty, // string
            htmlUrl: string.Empty, // string
            commentsUrl: string.Empty, // string
            eventsUrl: string.Empty, // string
            number: 10, // int
            state: ItemState.Open, //  ItemStat
            title: string.Empty, // string
            body: string.Empty, // string
            closedBy: null, // User
            user: null, // User
            labels: null, // IReadOnlyList<Label>
            assignee: null, // User
            assignees: null, // IReadOnlyList<User>
            milestone: null, // Milestone
            comments: 20, // int
            pullRequest: null, // PullRequest
            closedAt: DateTimeOffset.Now, // DateTimeOffset?
            createdAt: DateTimeOffset.Now, // DateTimeOffset
            updatedAt: DateTimeOffset.Now, // DateTimeOffset?
            id: 30, // int
            nodeId: string.Empty, // string
            locked: false, // bool
            repository: null, // Repository
            reactions: null, // ReactionSummary
            activeLockReason: LockReason.Resolved); // LockReason
    }
}
