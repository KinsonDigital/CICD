// <copyright file="ExtensionMethods.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using Project = Nuke.Common.ProjectModel.Project;

namespace CICDSystem;

/// <summary>
/// Provides helper methods throughout the project.
/// </summary>
internal static class ExtensionMethods
{
    private const char MatchNumbers = '#';
    private const char MatchAnything = '*';
    private static readonly char[] Digits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', };
    private static readonly char[] UpperCaseLetters =
    {
        'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
        'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
    };
    private static readonly char[] LowerCaseLetters =
    {
        'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
        'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
    };

    public static string ToKebabCase(this string value)
    {
        var allUpperCase = value.All(c => UpperCaseLetters.Contains(c));
        var allLowerCase = value.All(c => LowerCaseLetters.Contains(c));
        var noSpaces = value.All(c => c != ' ');
        if (string.IsNullOrEmpty(value) ||
            (noSpaces && (allUpperCase || allLowerCase)))
        {
            return value;
        }

        value = value.Trim();
        value = value.Replace(' ', '-');
        var result = string.Empty;

        for (var i = 0; i < value.Length; i++)
        {
            if (i == 0)
            {
                result += value[i].IsUpperCase() ? value[i].ToLowerCase().ToString() : value[i].ToString();
                continue;
            }

            if (i == value.Length - 1)
            {
                result += value[i].ToLowerCase();
                continue;
            }

            var currentIsUpperCase = UpperCaseLetters.Contains(value[i]);
            var prevIsHyphen = value[i - 1] == '-';

            if (currentIsUpperCase && prevIsHyphen)
            {
                result += value[i].ToLowerCase();
                continue;
            }

            var prevIsLowerCase = LowerCaseLetters.Contains(value[i - 1]);
            var nextIsLowerCase = LowerCaseLetters.Contains(value[i + 1]);
            var isValidForConversion = currentIsUpperCase && prevIsLowerCase && nextIsLowerCase;

            result += isValidForConversion
                ? $"-{value[i].ToLowerCase()}"
                : value[i];
        }

        return result;
    }

    public static bool IsUpperCase(this char value) => UpperCaseLetters.Contains(value);

    public static char ToLowerCase(this char value) => value.ToString().ToLower()[0];

    public static void LogRequirementTitle(this string requirementName, string value)
    {
        var indent = 15.CreateDuplicateCharacters(' ');
        Log.Information($"✅ Requirement '{requirementName}' Executed ✅{Environment.NewLine}{indent}{value}{Environment.NewLine}");
    }

    public static string CreateDuplicateCharacters(this int value, char character)
    {
        var result = string.Empty;

        if (value <= 0)
        {
            return character.ToString();
        }

        for (var i = 0; i < value; i++)
        {
            result += character.ToString();
        }

        return result;
    }

    public static string CapitalizeWords(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var result = string.Empty;

        foreach (var word in words)
        {
            if (word.Length == 1)
            {
                result += $" {word} ";
            }
            else
            {
                var newWord = $"{word[0].ToString().ToUpper()}{word[1..]}";
                result += $" {newWord} ";
            }
        }

        return result.Trim().Replace("  ", " ");
    }

    public static string ToSpaceDelimitedSections(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var result = string.Empty;

        for (var i = 0; i < value.Length; i++)
        {
            var character = value[i];

            result += UpperCaseLetters.Contains(character) && i != 0
                ? $" {character.ToString()}"
                : character.ToString();
        }

        return result.Replace("    ", " ").Replace("   ", " ").Replace("  ", " ");
    }

    public static bool IsProductionVersion(this string value) => EqualTo(value, "v#.#.#") || EqualTo(value, "#.#.#");

    public static bool IsPreviewVersion(this string value) => EqualTo(value, "v#.#.#-preview.#") || EqualTo(value, "#.#.#-preview.#");

    public static (bool hasVersion, string version) ExtractBranchVersion(this string branch)
    {
        if (string.IsNullOrEmpty(branch))
        {
            return (false, string.Empty);
        }

        var containsVersions = branch.IsPreviewBranch() || branch.IsReleaseBranch();

        return containsVersions
            ? (true, branch.Split('/')[1])
            : (false, string.Empty);
    }

    public static bool ContainsNumbers(this string value) => Digits.Any(value.Contains);

    public static bool DoesNotContainNumbers(this string value)
        => !ContainsNumbers(value);

    public static bool IsOnCorrectBranch(this GitRepository repo, string branchPattern)
    {
        if (repo.Branch is null)
        {
            return false;
        }

        return EqualTo(repo.Branch, branchPattern);
    }

    public static bool IsCorrectBranch(this string branch, string branchPattern) => EqualTo(branch, branchPattern);

    public static bool IsMasterBranch(this string branch) => branch == "master";

    public static bool IsDevelopBranch(this string branch) => branch == "develop";

    public static bool IsFeatureBranch(this string branch) => IsCorrectBranch(branch, "feature/#-*");

    public static bool IsPreviewBranch(this string branch) => IsCorrectBranch(branch, "preview/v*.*.*-preview.#");

    public static bool IsReleaseBranch(this string branch) => IsCorrectBranch(branch, "release/v*.*.*");

    public static bool IsPreviewFeatureBranch(this string branch) => IsCorrectBranch(branch, "preview/feature/#-*");

    public static bool IsHotFixBranch(this string branch) => IsCorrectBranch(branch, "hotfix/#-*");

    public static BranchType GetBranchType(this string branch)
    {
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

    public static bool HasCorrectVersionSyntax(this Project project, string versionPattern)
    {
        var currentVersion = project.GetVersion();

        return EqualTo(currentVersion, versionPattern);
    }

    public static bool HasCorrectFileVersionSyntax(this Project project, string versionPattern)
    {
        var currentVersion = project.GetFileVersion();

        return EqualTo(currentVersion, versionPattern);
    }

    public static bool HasCorrectAssemblyVersionSyntax(this Project project, string versionPattern)
    {
        var currentVersion = project.GetAssemblyVersion();

        return EqualTo(currentVersion, versionPattern);
    }

    public static bool VersionExists(this Project project) => !string.IsNullOrEmpty(project.GetProperty("Version"));

    public static bool FileVersionExists(this Project project) => !string.IsNullOrEmpty(project.GetProperty("FileVersion"));

    public static bool AssemblyVersionExists(this Project project) => !string.IsNullOrEmpty(project.GetProperty("AssemblyVersion"));

    public static string GetVersion(this Project project)
    {
        var version = project.GetProperty("Version");

        if (string.IsNullOrEmpty(version))
        {
            // TODO: Create custom exception name MissingVersionException
            // TODO: In the exception, explain how to set the version
            throw new Exception($"The version for project '{project.Name}' is not set.");
        }

        return version;
    }

    public static string GetFileVersion(this Project project)
    {
        var version = project.GetProperty("FileVersion");

        if (string.IsNullOrEmpty(version))
        {
            // TODO: Create custom exception name MissingFileVersionException
            // TODO: In the exception, explain how to set the version
            throw new Exception($"The file version for project '{project.Name}' is not set.");
        }

        return version;
    }

    public static string GetAssemblyVersion(this Project project)
    {
        var version = project.GetProperty("AssemblyVersion");

        if (string.IsNullOrEmpty(version))
        {
            // TODO: Create custom exception name MissingAssemblyVersionException - **Do you still need this?**
            // TODO: In the exception, explain how to set the version - **Do you still need this?**
            throw new Exception($"The assembly version for project '{project.Name}' is not set.");
        }

        return version;
    }

    /// <summary>
    /// Returns a string that represents the most amount of time that was spent on any of the given <paramref name="issues"/>.
    /// </summary>
    /// <param name="issues">The issues to analyze.</param>
    /// <returns>The most time spent on an issue in string format.</returns>
    /// <remarks>
    ///     Format is <c>'#d #h #m'</c>.
    /// </remarks>
    public static string MostTimeSpentOnIssue(this IEnumerable<Issue> issues)
    {
        var mostTimeSpent = issues.Max(i => i.ClosedAt - i.CreatedAt);

        return mostTimeSpent is null
            ? "Unknown"
            : $"{mostTimeSpent.Value.Days}d {mostTimeSpent.Value.Hours}h {mostTimeSpent.Value.Minutes}m";
    }

    /// <summary>
    /// Returns a string that represents the total amount of time that the given list of <paramref name="issues"/> was completed.
    /// </summary>
    /// <param name="issues">The issues to analyze.</param>
    /// <returns>The total time spend on all of the issues.</returns>
    /// <remarks>
    ///     Format is <c>'#d #h #m'</c>.
    /// </remarks>
    public static string TotalTimeToComplete(this IEnumerable<Issue> issues)
    {
        var enumeratedIssues = issues.ToArray();
        var oldest = enumeratedIssues.Min(i => enumeratedIssues.Length <= 1 ? i.CreatedAt : i.ClosedAt);
        var newest = enumeratedIssues.Max(i => i.ClosedAt);

        var timeToComplete = newest - oldest;

        return timeToComplete is null
            ? "Unknown"
            : $"{timeToComplete.Value.Days}d {timeToComplete.Value.Hours}h {timeToComplete.Value.Minutes}m";
    }

    /// <summary>
    /// Returns a distinct list of all the labels in the given list of <paramref name="issues"/>.
    /// </summary>
    /// <param name="issues">The list of issues.</param>
    /// <returns>A distinct list of label names.</returns>
    public static IEnumerable<string> GetDistinctLabelNames(this IEnumerable<Issue> issues)
    {
        var allLabels = new List<string>();

        foreach (var issue in issues)
        {
            allLabels.AddRange(issue.Labels.Select(l => l.Name));
        }

        return allLabels.Distinct();
    }

    /// <summary>
    /// Gets a distinct list of all the assignee URL's for all of the issues.
    /// </summary>
    /// <param name="issues">The list of issues.</param>
    /// <returns>The distinct list of URL's.</returns>
    public static IEnumerable<(string login, string url)> GetDistinctAssigneeLoginAndUrl(this IEnumerable<Issue> issues)
    {
        var assignees = issues.Where(i => i.Assignee is not null).Select(i => i.Assignee).ToArray();

        var data = assignees.Select(a => (a.Login, a.HtmlUrl)).ToArray();

        return data.DistinctBy(i => i.Login);
    }

    public static async Task<bool> TagExists(
        this IRepositoriesClient client,
        string repoOwner,
        string repoName,
        string tag)
    {
        var tags = await client.GetAllTags(repoOwner, repoName);

        var foundTag = (from t in tags
            where t.Name == tag
            select t).FirstOrDefault();

        return foundTag is not null;
    }

    public static async Task<bool> IssueExists(
        this IIssuesClient client,
        string owner,
        string name,
        int issueNumber)
    {
        try
        {
            var result = await client.Get(owner, name, issueNumber);

            return result.PullRequest is null;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    public static async Task<bool> HasLabels(
        this IIssuesClient client,
        string owner,
        string name,
        int issueNumber)
    {
        try
        {
            var issue = await client.Get(owner, name, issueNumber);

            return issue is not null && issue.PullRequest is null && issue.Labels.Count >= 1;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns all of the issues assigned to a milestone that matches the given <paramref name="milestoneTitle"/>,
    /// for the given repository <paramref name="owner"/> and for the repository that matches the given <paramref name="repoName"/>.
    /// </summary>
    /// <param name="client">Calls out to the GitHub API to get issues.</param>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="milestoneTitle">The title of the milestone.</param>
    /// <returns>The list of issues.</returns>
    public static async Task<Issue[]> IssuesForMilestone(
        this IIssuesClient client,
        string owner,
        string repoName,
        string milestoneTitle)
    {
        var issueRequest = new RepositoryIssueRequest
        {
            Filter = IssueFilter.All,
            State = ItemStateFilter.All,
        };

        var issues = (await client.GetAllForRepository(owner, repoName, issueRequest))
            .Where(i =>
                i.PullRequest is null &&
                i.Milestone is not null &&
                i.Milestone.Title == milestoneTitle).ToArray();

        return issues;
    }

    public static async Task<Issue[]> PullRequestsForMilestone(
        this IIssuesClient client,
        string owner,
        string name,
        string mileStoneName)
    {
        var issueRequest = new RepositoryIssueRequest();
        issueRequest.Filter = IssueFilter.All;
        issueRequest.State = ItemStateFilter.All;

        var pullRequests = (await client.GetAllForRepository(owner, name, issueRequest))
            .Where(i =>
                i.PullRequest is not null &&
                i.Milestone is not null &&
                i.Milestone.Title == mileStoneName).ToArray();

        return pullRequests;
    }

    public static async Task<bool> Exists(
        this IPullRequestsClient client,
        string repoOwner,
        string repoName,
        int prNumber)
    {
        var pr = await client.Get(repoOwner, repoName, prNumber);

        return pr is not null;
    }

    public static async Task<bool> HasAssignees(
        this IPullRequestsClient client,
        string owner,
        string name,
        int prNumber)
    {
        try
        {
            var pr = await client.Get(owner, name, prNumber);

            return pr is not null && pr.Assignees.Count >= 1;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    public static async Task<bool> HasLabels(
        this IPullRequestsClient client,
        string owner,
        string name,
        int prNumber)
    {
        try
        {
            var pr = await client.Get(owner, name, prNumber);

            return pr is not null && pr.Labels.Count >= 1;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    /// <summary>
    /// Returns all of the pull requests assigned to a milestone that matches the given <paramref name="milestoneTitle"/>,
    /// for the given repository <paramref name="owner"/> and for the repository that matches the given <paramref name="repoName"/>.
    /// </summary>
    /// <param name="client">Calls out to the GitHub API to get issues.</param>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="milestoneTitle">The title of the milestone.</param>
    /// <returns>The list of issues.</returns>
    public static async Task<PullRequest[]> PullRequestsForMilestone(
        this IPullRequestsClient client,
        string owner,
        string repoName,
        string milestoneTitle)
    {
        var issueRequest = new PullRequestRequest
        {
            State = ItemStateFilter.All,
        };

        var pullRequests = (await client.GetAllForRepository(owner, repoName, issueRequest))
            .Where(i => i.Milestone is not null && i.Milestone.Title == milestoneTitle).ToArray();

        return pullRequests;
    }

    /// <summary>
    /// Returns a value indicating whether or not a pull request is assigned to a milestone.
    /// </summary>
    /// <param name="client">Calls out to the GitHub API to get a pull request.</param>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="prNumber">The pull request number.</param>
    /// <returns>An asynchronous <c>boolean</c> of <c>true</c> if assigned.</returns>
    /// <remarks>
    ///     <para>
    ///         Closed milestones are considered released so opened pull requests cannot be assigned to a closed milestone.
    ///     </para>
    ///
    ///     <para>
    ///         Open milestones are not considered released so closed pull requests cannot be assigned to an opened milestone.
    ///     </para>
    /// </remarks>
    public static async Task<(bool isAssigned, Milestone? milestone)> AssignedToMilestone(
        this IPullRequestsClient client,
        string owner,
        string repoName,
        int prNumber)
    {
        try
        {
            var pr = await client.Get(owner, repoName, prNumber);

            var isAssigned = pr.State == ItemState.Open
                ? pr.Milestone is not null && pr.Milestone.State == ItemState.Open
                : pr.Milestone is not null && pr.Milestone.State == ItemState.Closed;

            return (isAssigned, pr.Milestone ?? null);
        }
        catch (NotFoundException)
        {
            return (false, null);
        }
    }

    public static async Task<bool> ReleaseExists(
        this IReleasesClient client,
        string owner,
        string repoName,
        string tag) =>
        (from r in await client.GetAll(owner, repoName)
            where r.TagName == tag
            select r).ToArray().Any();

    public static async Task UploadTextFileAsset(this IReleasesClient client, Release release, string filePath)
    {
        if (File.Exists(filePath) is false)
        {
            var errorMsg = "Could not upload text file asset to release '{Value1}'.";
            errorMsg += $"{Environment.NewLine}The path to the asset file '{{Value2}} does not exist.";
            Log.Error(errorMsg, release.Name, filePath);
            throw new Exception($"The asset file '{filePath}' does not exist.");
        }

        await using var fileAsset = File.OpenRead(filePath);

        var assetUpload = new ReleaseAssetUpload()
        {
            FileName = Path.GetFileName(filePath),
            ContentType = "application/zip",
            RawData = fileAsset,
        };

        try
        {
            var result = await client.UploadAsset(release, assetUpload);

            Log.Information("Id: {Value}", result.Id);
            Log.Information("Label: {Value}", result.Label);
            Log.Information("Name: {Value}", result.Name);
        }
        catch (Exception e)
        {
            var errorMsg = "Uploading the text file asset '{Value1}' for release '{Value2}' failed.";
            errorMsg += $"{Environment.NewLine}{e.Message}";
            Log.Error(errorMsg);
            throw;
        }
    }

    /// <summary>
    /// Returns a value indicating whether or not a milestone with a title matches the given <paramref name="version"/> string.
    /// </summary>
    /// <param name="client">Calls out to the GitHub API to get milestones.</param>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="version">The version string that the milestone title should match.</param>
    /// <returns><c>true</c> if the milestone exists.</returns>
    public static async Task<bool> MilestoneExists(
        this IMilestonesClient client,
        string owner,
        string repoName,
        string version) =>
        (from m in await client.GetAllForRepository(owner, repoName)
            where m.Title == version
            select m).Any();

    /// <summary>
    /// Gets a milestone that contains a title that matches the given <paramref name="title"/>, that belongs to the
    /// given <paramref name="owner"/> and repository with the name <paramref name="repoName"/>.
    /// </summary>
    /// <param name="client">Calls out to the GitHub API to get milestones.</param>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="title">The title of the milestone.</param>
    /// <returns>The milestone if it was found.</returns>
    /// <remarks>
    ///     Returns <c>null</c> if the milestone is not found.
    /// </remarks>
    public static async Task<Milestone?> GetByTitle(
        this IMilestonesClient client,
        string owner,
        string repoName,
        string title)
    {
        var milestones = (from m in await client.GetAllForRepository(owner, repoName)
            where m.Title == title
            select m).ToArray();

        return milestones.Length <= 0 ? null : milestones[0];
    }

    /// <summary>
    /// Gets the HTML URL of a release that matches the given <paramref name="title"/>, that belongs to the
    /// given <paramref name="owner"/> and repository with the name <paramref name="repoName"/>.
    /// </summary>
    /// <param name="client">Calls out to the GitHub API to get milestones.</param>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="title">The title of the milestone.</param>
    /// <returns>The HTML URL of the milestone.</returns>
    public static async Task<string> GetHtmlUrl(
        this IMilestonesClient client,
        string owner,
        string repoName,
        string title)
    {
        var milestones = (from m in await client.GetAllForRepository(owner, repoName)
            where m.Title == title
            select m).ToArray();

        return milestones.Length <= 0 ? string.Empty : milestones[0].HtmlUrl;
    }

    /// <summary>
    /// Closes an open milestone with a title that matches the given <paramref name="title"/>, that belongs to the
    /// given <paramref name="owner"/> and repository that is named <paramref name="repoName"/>.
    /// </summary>
    /// <param name="client">Calls out to the GitHub API to get milestones.</param>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="title">The title of the milestone.</param>
    /// <returns>The milestone after being updated.</returns>
    /// <exception cref="NotFoundException">Occurs if the milestone is not found.</exception>
    /// <exception cref="Exception">Occurs if something goes wrong with the update process.</exception>
    public static async Task<Milestone> CloseMilestone(this IMilestonesClient client, string owner, string repoName, string title)
    {
        var milestones = await client.GetAllForRepository(owner, repoName);

        var foundMilestone = milestones.FirstOrDefault(m => m.Title == title);

        if (foundMilestone is null)
        {
            throw new NotFoundException($"A milestone with the title/name '{title}' was not found.", HttpStatusCode.NotFound);
        }

        var mileStoneUpdate = new MilestoneUpdate()
        {
            State = ItemState.Closed,
        };

        var updatedMilestone = await client.Update(owner, repoName, foundMilestone.Number, mileStoneUpdate);

        if (updatedMilestone is null)
        {
            throw new Exception($"The milestone '{title}' could not be updated.");
        }

        return updatedMilestone;
    }

    /// <summary>
    /// Updates the <paramref name="description"/> of a milestone that has the given milestone <paramref name="title"/>
    /// and belongs to the given <paramref name="owner"/> and is part of a repository that matches the given <paramref name="repoName"/>.
    /// </summary>
    /// <param name="client">Calls out to the GitHub API to get milestones.</param>
    /// <param name="owner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="title">The title of the milestone.</param>
    /// <param name="description">The description to update the milestone.</param>
    /// <returns>The updated milestone.</returns>
    /// <exception cref="NotFoundException">Occurs if the milestone is not found.</exception>
    /// <exception cref="Exception">Occurs if something goes wrong with the update process.</exception>
    public static async Task<Milestone> UpdateMilestoneDescription(
        this IMilestonesClient client,
        string owner,
        string repoName,
        string title,
        string description)
    {
        var request = new MilestoneRequest() { State = ItemStateFilter.All };
        var milestones = await client.GetAllForRepository(owner, repoName, request);

        var foundMilestone = milestones.FirstOrDefault(m => m.Title == title);

        if (foundMilestone is null)
        {
            throw new NotFoundException($"A milestone with the title/name '{title}' was not found.", HttpStatusCode.NotFound);
        }

        var mileStoneUpdate = new MilestoneUpdate()
        {
            Description = description,
        };

        var updatedMilestone = await client.Update(owner, repoName, foundMilestone.Number, mileStoneUpdate);

        if (updatedMilestone is null)
        {
            throw new Exception($"The milestone '{title}' description could not be updated.");
        }

        return updatedMilestone;
    }

    /// <summary>
    /// Returns a value indicating whether or not the GitHub issue is a standard issue.
    /// </summary>
    /// <param name="issue">The GitHub issue to check.</param>
    /// <returns><c>true</c> if the issue is standard.</returns>
    public static bool IsIssue(this Issue issue) => issue.PullRequest is null;

    /// <summary>
    /// Returns a value indicating whether or not the GitHub issue is a pull request.
    /// </summary>
    /// <param name="issue">The GitHub issue to check.</param>
    /// <returns><c>true</c> if a standard issue.</returns>
    public static bool IsPullRequest(this Issue issue) => issue.PullRequest is not null;

    /// <summary>
    /// Returns a value indicating whether or not a GitHub issue is a release to do issue.
    /// </summary>
    /// <param name="issue">The GitHub issue to check.</param>
    /// <param name="releaseType">The type of release issue.</param>
    /// <returns><c>true</c> if it's a release pull request.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="releaseType"/> is out of range.</exception>
    public static bool IsReleaseToDoIssue(this Issue issue, ReleaseType releaseType)
    {
        var releaseLabelOrTitle = releaseType switch
        {
            ReleaseType.Preview => "🚀Preview Release",
            ReleaseType.Production => "🚀Production Release",
            ReleaseType.HotFix => "🚀Hot Fix Release",
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null),
        };

        var isIssue = issue.IsPullRequest() is false;
        var validTitle = issue.Title == releaseLabelOrTitle;
        var validLabelType = issue.Labels.Any(l => l.Name == releaseLabelOrTitle);

        return isIssue && validTitle && validLabelType;
    }

    /// <summary>
    /// Returns a value indicating whether or not a GitHub issue is a QA testing issue.
    /// </summary>
    /// <param name="issue">The GitHub issue to check.</param>
    /// <returns><c>true</c> if a QA testing issue.</returns>
    public static bool IsQATestingIssue(this Issue issue)
    {
        const string qaTestingLabel = "🧪qa testing";
        var isIssue = issue.PullRequest is null;
        var validLabelState = (issue.Labels?.Any(l => l.Name == qaTestingLabel) ?? false)
                              && issue.Labels.Count == 1;

        return isIssue && validLabelState;
    }

    /// <summary>
    /// Returns a value indicating whether or not a GitHub issue is a release pull request.
    /// </summary>
    /// <param name="issue">The GitHub issue to check.</param>
    /// <param name="releaseType">The type of release pull request.</param>
    /// <returns><c>true</c> if a release pull request.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="releaseType"/> is out of range.</exception>
    public static bool IsReleasePullRequest(this Issue issue, ReleaseType releaseType)
    {
        var releaseLabelOrTitle = releaseType switch
        {
            ReleaseType.Preview => "🚀Preview Release",
            ReleaseType.Production => "🚀Production Release",
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null),
        };

        var hasValidTitle = issue.Title == releaseLabelOrTitle;
        var hasSingleLabel = issue.Labels.Count == 1;
        var isPullRequest = issue.IsPullRequest();
        var validLabelType = issue.Labels.Count == 1 && issue.Labels[0].Name == releaseLabelOrTitle;

        return hasValidTitle && hasSingleLabel && isPullRequest && validLabelType;
    }

    /// <summary>
    /// Gets the text for logging issue data to the console.
    /// </summary>
    /// <param name="issue">The issue to log.</param>
    /// <param name="totalSpaces">The total number of spaces to indent the content.</param>
    /// <returns>The text to log to the console.</returns>
    public static string GetLogText(this Issue issue, int totalSpaces = 0)
    {
        var indent = string.Empty;
        for (var i = 0; i < totalSpaces; i++)
        {
            indent += " ";
        }

        var text = string.Empty;

        var prOrIssuePrefix = issue.PullRequest is null ? "Issue" : "PR";
        text += $"{Environment.NewLine}{indent}{prOrIssuePrefix} Number: {issue.Number}";
        text += $"{Environment.NewLine}{indent}{prOrIssuePrefix} Title: {issue.Title}";
        text += $"{Environment.NewLine}{indent}{prOrIssuePrefix} State: {issue.State}";
        text += $"{Environment.NewLine}{indent}{prOrIssuePrefix} Url: {issue.HtmlUrl}";
        text += $"{Environment.NewLine}{indent}Labels ({(issue.Labels.Count <= 0 ? "None" : issue.Labels.Count.ToString())}):";
        issue.Labels.ForEach(l => text += $"{Environment.NewLine}{indent}\t  - `{l.Name}`");

        return text;
    }

    public static string GetLogText(this IReadOnlyList<Issue> issues, int totalIndentSpaces = 0)
    {
        var spaces = string.Empty;
        for (var i = 0; i < totalIndentSpaces; i++)
        {
            spaces += " ";
        }

        var errorMsg = $"{spaces}---------------------Issue(s)---------------------";

        for (var i = 0; i < issues.Count; i++)
        {
            var issue = issues[i];
            errorMsg += issue.GetLogText(totalIndentSpaces);
            errorMsg += i >= issues.Count ? string.Empty : $"{Environment.NewLine}{spaces}----------------------------------------------";
        }

        return errorMsg;
    }

    public static void LogAsError(this IReadOnlyList<Issue> issues, int totalIndentSpaces = 0)
        => Log.Error(issues.GetLogText(totalIndentSpaces));

    public static void LogAsInfo(this IReadOnlyList<Issue> issues, int totalIndentSpaces = 0)
        => Log.Information(issues.GetLogText(totalIndentSpaces));

    /// <summary>
    /// Prints all of the given <paramref name="errors"/> and fails the build with the given <paramref name="failMsg"/>.
    /// </summary>
    /// <param name="errors">The list of error messages to print.</param>
    /// <param name="failMsg">The message to print when failing the build.</param>
    [ExcludeFromCodeCoverage]
    public static void PrintErrors(this IEnumerable<string>? errors, string? failMsg = null)
    {
        var errorList = errors is null
            ? Array.Empty<string>()
            : errors.ToArray();
        if (errorList.Length <= 0)
        {
            return;
        }

        foreach (var error in errorList)
        {
            Log.Error($"{error}{Environment.NewLine}");
        }

        if (failMsg is not null)
        {
            Assert.Fail(failMsg);
        }
    }

    /// <summary>
    /// Returns a value indicating whether or not the current <c>string</c> matches the given <paramref name="pattern"/>.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="pattern">The pattern to check against the <c>string</c> <paramref name="value"/>.</param>
    /// <returns><c>true</c> if the <paramref name="pattern"/> is equal to the branch name.</returns>
    /// <remarks>
    ///     The comparison is case sensitive.
    /// </remarks>
    public static bool EqualTo(this string value, string pattern)
    {
        pattern = string.IsNullOrEmpty(pattern) ? string.Empty : pattern;

        var hasGlobbingSyntax = pattern.Contains(MatchNumbers) || pattern.Contains(MatchAnything);
        var isEqual = hasGlobbingSyntax
            ? Match(value, pattern)
            : (string.IsNullOrEmpty(pattern) && string.IsNullOrEmpty(value)) || pattern == value;

        return isEqual;
    }

    /// <summary>
    /// Converts the given <see cref="ApiValidationException"/> to a string with
    /// all of the internal error details.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <returns>The detailed exception message.</returns>
    [ExcludeFromCodeCoverage]
    public static string ToLogMessage(this ApiValidationException exception)
    {
        var errorsInfo = new StringBuilder();

        errorsInfo.AppendLine($"\t{exception.ApiError.Message}");

        foreach (ApiErrorDetail errorDetail in exception.ApiError.Errors)
        {
            errorsInfo.AppendLine($"\t{errorDetail.Message}");
            errorsInfo.AppendLine($"\t{errorDetail.Code}");
            errorsInfo.AppendLine($"\t{errorDetail.Field}");
            errorsInfo.AppendLine($"\t{errorDetail.Resource}");
        }

        var msgBuilder = new StringBuilder();

        msgBuilder.AppendLine($"Message: {exception.Message}");
        msgBuilder.AppendLine($"Status Code: {exception.StatusCode}");
        msgBuilder.AppendLine($"Error Details: {errorsInfo.ToString()}");

        return msgBuilder.ToString();
    }

    /// <summary>
    /// Returns a value indicating whether or not the given <paramref name="globbingPattern"/> contains a match
    /// to the given <c>string</c> <paramref name="value"/>.
    /// </summary>
    /// <param name="value">The <c>string</c> to match.</param>
    /// <param name="globbingPattern">The globbing pattern and text to search.</param>
    /// <returns>
    ///     <c>true</c> if the globbing pattern finds a match in the given <c>string</c> <paramref name="value"/>.
    /// </returns>
    private static bool Match(string value, string globbingPattern)
    {
        // NOTE: Refer to this website for more regex information -> https://regex101.com/
        const char regexMatchStart = '^';
        const char regexMatchEnd = '$';
        const string regexMatchNumbers = @"\d+";
        const string regexMatchAnything = ".+";

        // Remove any consecutive '#' and '*' symbols until no more consecutive symbols exist
        globbingPattern = RemoveConsecutiveCharacters(new[] { MatchNumbers, MatchAnything }, globbingPattern);

        // Replace the '#' symbol with
        globbingPattern = globbingPattern.Replace(MatchNumbers.ToString(), regexMatchNumbers);

        // Prefix all '.' symbols with '\' to match the '.' literally in regex
        globbingPattern = globbingPattern.Replace(".", @"\.");

        // Replace all '*' character with '.+'
        globbingPattern = globbingPattern.Replace(MatchAnything.ToString(), regexMatchAnything);

        globbingPattern = $"{regexMatchStart}{globbingPattern}{regexMatchEnd}";

        return Regex.Matches(value, globbingPattern).Count > 0;
    }

    /// <summary>
    /// Removes any consecutive occurrences of the given <paramref name="characters"/> from the given <c>string</c> <paramref name="value"/>.
    /// </summary>
    /// <param name="characters">The <c>char</c> to check.</param>
    /// <param name="value">The value that contains the consecutive characters to remove.</param>
    /// <returns>The original <c>string</c> value with the consecutive characters removed.</returns>
    private static string RemoveConsecutiveCharacters(IEnumerable<char> characters, string value)
    {
        foreach (var c in characters)
        {
            while (value.Contains($"{c}{c}"))
            {
                value = value.Replace($"{c}{c}", c.ToString());
            }
        }

        return value;
    }
}
