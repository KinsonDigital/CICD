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
    /// <param name="linkedToPR">The issue is linked to a pull request.</param>
    /// <param name="totalLabels">The total number of labels to add to the issue.</param>
    /// <returns>The pull request object to help with testing.</returns>
    /// <remarks>
    ///     <para>A <paramref name="totalLabels"/> value of -1 will have the labels null.</para>
    ///     <para>A <paramref name="totalLabels"/> value of 0 will not have the labels null but have no labels.</para>
    ///     <para>A <paramref name="totalLabels"/> value greater than 0 will have labels.</para>
    /// </remarks>
    public static Issue CreateIssue(bool linkedToPR = true, int totalLabels = -1)
    {
        var labels = new List<Label>();

        for (var i = 0; i < totalLabels; i++)
        {
            labels.Add(CreateLabel($"label-{i}"));
        }

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
            labels: totalLabels < 0 ? null : new ReadOnlyCollection<Label>(labels.ToArray()), // IReadOnlyList<Label>
            assignee: null, // User
            assignees: null, // IReadOnlyList<User>
            milestone: null, // Milestone
            comments: 20, // int
            pullRequest: linkedToPR ? null : CreatePullRequest(string.Empty, string.Empty), // PullRequest
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

    /// <summary>
    /// Creates a <see cref="Label"/> with the given label <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name of the label.</param>
    /// <returns>The label.</returns>
    public static Label CreateLabel(string name = "")
    {
        return new Label(
            id: 40, // long
            url: string.Empty, // string
            name: name, // string
            nodeId: string.Empty, // string
            color: string.Empty, // string
            description: string.Empty, // string
            @default: false); // bool
    }
}
