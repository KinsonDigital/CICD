using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.Git;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Octokit;
using Serilog;
using Project = Nuke.Common.ProjectModel.Project;

public static class ExtensionMethods
{
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

    private static readonly char[] SnakableCharacters = { ' ', '-' };

    private const char MatchNumbers = '#';
    private const char MatchAnything = '*';

    public static string AddIndents(this string value, int count, bool addBefore = true)
    {
        var tabs = string.Empty;

        for (var i = 0; i < count; i++)
        {
            tabs += "  ";
        }

        return addBefore ? $"{tabs}{value}" : $"{value}{tabs}";
    }

    public static string AddNewLine(this string value, int count, bool addAfter = true)
    {
        var newLines = string.Empty;

        for (var i = 0; i < count; i++)
        {
            newLines += Environment.NewLine;
        }

        return addAfter ? $"{value}{newLines}" : $"{newLines}{value}";
    }

    public static string ToSnakeCase(this string value)
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
        return value.Replace(" ", "_").Replace("-", "_").ToLower();
    }

    public static string ToPascalCase(this string value)
    {
        /*
         * Build Project
         * BuildProject
         * Build-Project
         * Build_Project
         * build project
         */
        var allUpperCase = value.All(c => UpperCaseLetters.Contains(c));
        var allLowerCase = value.All(c => LowerCaseLetters.Contains(c));
        var noSpaces = value.All(c => c != ' ');
        if (string.IsNullOrEmpty(value) ||
            (noSpaces && (allUpperCase || allLowerCase)))
        {
            return value;
        }

        var characters = value.ToArray();

        for (var i = 0; i < characters.Length; i++)
        {
            if (i == 0)
            {
                characters[i] = LowerCaseLetters.Contains(characters[i])
                    ? characters[i].ToString().ToUpper()[0]
                    : characters[i];
                continue;
            }

            var current = characters[i];
            var previous = characters[i >= 1 ? i - 1 : i];

            if (LowerCaseLetters.Contains(current) && previous == ' ')
            {
                characters[i] = current.ToString().ToUpper()[0];
            }
        }

        return string.Join("", characters)
            .Replace("-", " ")
            .Replace("_", " ")
            .ToSpaceDelimitedSections();
    }

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
                var stuff = word[1..];
                var newWord = $"{word[0].ToString().ToUpper()}{word[1..]}";
                result += $" {newWord} ";
            }
        }

        return result.Trim().Replace("  ", " ");
    }

    public static bool IsNotNullOrEmpty(this string value) => !string.IsNullOrEmpty(value);

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

    public static bool IsOnFeatureBranch(this GitRepository repo)
    {
        const string namingSyntax = "feature/#-*";
        var errorMsg = "The feature branch '{Value}' does not follow the correct naming syntax.";
        errorMsg += $"{Environment.NewLine}Use the naming syntax '{namingSyntax}' for feature branches.";
        errorMsg += "Example: feature/123-my-feature-branch";

        var result = IsOnCorrectBranch(repo, namingSyntax);

        if (result is false)
        {
            Log.Error(errorMsg, repo.Branch);
        }

        return result;
    }

    public static bool IsFeatureBranch(this string branch) => IsCorrectBranch(branch, "feature/#-*");

    public static bool IsOnPreviewReleaseBranch(this GitRepository repo)
    {
        const string namingSyntax = "preview/v*.*.*-preview.#";
        var errorMsg = "The preview release branch '{Value}' does not follow the correct naming syntax.";
        errorMsg += $"{Environment.NewLine}Use the naming syntax '{namingSyntax}' for feature branches.";
        errorMsg += "Example: preview/v1.2.3-preview.4";

        var result = IsOnCorrectBranch(repo, namingSyntax);

        if (result is false)
        {
            Log.Error(errorMsg, repo.Branch);
        }

        return result;
    }

    public static bool IsPreviewBranch(this string branch) => IsCorrectBranch(branch, "preview/v*.*.*-preview.#");

    public static bool IsOnReleaseBranch(this GitRepository repo)
    {
        const string namingSyntax = "release/v*.*.*";
        var errorMsg = "The release branch '{Value}' does not follow the correct naming syntax.";
        errorMsg += $"{Environment.NewLine}Use the naming syntax '{namingSyntax}' for feature branches.";
        errorMsg += "Example: release/v1.2.3";

        var result = IsOnCorrectBranch(repo, namingSyntax);

        if (result is false)
        {
            Log.Error(errorMsg, repo.Branch);
        }

        return result;
    }

    public static bool IsReleaseBranch(this string branch) => IsCorrectBranch(branch, "release/v*.*.*");

    public static bool IsOnPreviewFeatureBranch(this GitRepository repo)
    {
        const string namingSyntax = "preview/feature/#-*";
        var errorMsg = "The preview feature branch '{Value}' does not follow the correct naming syntax.";
        errorMsg += $" {Environment.NewLine}Use the naming syntax '{namingSyntax}' for feature branches.";
        errorMsg += "Example: preview/feature/123-my-preview-branch";

        var result = IsOnCorrectBranch(repo, namingSyntax);

        if (result is false)
        {
            Log.Error(errorMsg, repo.Branch);
        }

        return result;
    }

    public static bool IsPreviewFeatureBranch(this string branch) => IsCorrectBranch(branch, "preview/feature/#-*");

    public static bool IsOnHotFixBranch(this GitRepository repo)
    {
        const string namingSyntax = "hotfix/#-*";
        var errorMsg = "The hotfix branch '{Value}' does not follow the correct naming syntax.";
        errorMsg += $" {Environment.NewLine}Use the naming syntax '{namingSyntax}' for feature branches.";
        errorMsg += "Example: hotfix/123-my-hotfix-branch";

        var result = IsOnCorrectBranch(repo, namingSyntax);

        if (result is false)
        {
            Log.Error(errorMsg, repo.Branch);
        }

        return result;
    }

    public static void CreateTag(this GitRepository _, string tag)
    {
        GitTasks.Git($"tag {tag}");
        GitTasks.Git($"push origin {tag}");
    }

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

    public static bool AllVersionsExist(this Project project)
    {
        return project.VersionExists() && project.FileVersionExists() && project.AssemblyVersionExists();
    }

    public static bool VersionExists(this Project project)
    {
        return !string.IsNullOrEmpty(project.GetProperty("Version"));
    }

    public static bool FileVersionExists(this Project project)
    {
        return !string.IsNullOrEmpty(project.GetProperty("FileVersion"));
    }

    public static bool AssemblyVersionExists(this Project project)
    {
        return !string.IsNullOrEmpty(project.GetProperty("AssemblyVersion"));
    }

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
            // TODO: Create custom exception name MissingAssemblyVersionException
            // TODO: In the exception, explain how to set the version
            throw new Exception($"The assembly version for project '{project.Name}' is not set.");
        }

        return version;
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

    public static async Task<bool> LabelExists(
        this IIssuesLabelsClient client,
        string repoOwner,
        string repoName,
        int issueNumber,
        string labelName)
    {
        var issueLabels = await client.GetAllForIssue(repoOwner, repoName, issueNumber);

        return issueLabels.Any(l => l.Name == labelName);
    }

    public static async Task<bool> LabelExists(
        this IPullRequestsClient client,
        string repoOwner,
        string repoName,
        int prNumber,
        string labelName)
    {
        var pr = await client.Get(repoOwner, repoName, prNumber);

        return pr.Labels.Any(l => l.Name == labelName);
    }

    public static async Task<bool> TagDoesNotExist(
        this IRepositoriesClient client,
        string repoOwner,
        string repoName,
        string tag)
    {
        return !await TagExists(client, repoOwner, repoName, tag);
    }

    public static bool IsManuallyExecuted(this GitHubActions gitHubActions)
        => gitHubActions.IsPullRequest is false && gitHubActions.EventName == "workflow_dispatch";

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
        catch (NotFoundException e)
        {
            return false;
        }

        return true;
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
        catch (NotFoundException e)
        {
            return false;
        }
    }

    public static async Task<Issue[]> IssuesForMilestone(
        this IIssuesClient client,
        string owner,
        string repoName,
        string milestoneName)
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
                i.Milestone.Title == milestoneName).ToArray();

        return issues;
    }

    public static async Task<Issue[]> IssuesForMilestones(
        this IIssuesClient client,
        string owner,
        string name,
        string[] milestoneNames)
    {
        var issueRequest = new RepositoryIssueRequest
        {
            Filter = IssueFilter.All,
            State = ItemStateFilter.All,
        };

        var issues = (await client.GetAllForRepository(owner, name, issueRequest))
            .Where(i =>
                i.PullRequest is null &&
                i.Milestone is not null &&
                milestoneNames.Contains(i.Milestone.Title)).ToArray();

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

    public static async Task<bool> HasReviewers(
        this IPullRequestsClient client,
        string owner,
        string name,
        int prNumber)
    {
        try
        {
            var pr = await client.Get(owner, name, prNumber);

            return pr is not null && pr.RequestedReviewers.Count >= 1;
        }
        catch (NotFoundException e)
        {
            return false;
        }
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
        catch (NotFoundException e)
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
        catch (NotFoundException e)
        {
            return false;
        }
    }

    public static async Task<bool> ReleaseExists(
        this IReleasesClient client,
        string owner,
        string repoName,
        string tag)
    {
        return (from r in await client.GetAll(owner, repoName)
            where r.TagName == tag
            select r).ToArray().Any();
    }

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
            RawData = fileAsset
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

    public static async Task<bool> MilestoneExists(
        this IMilestonesClient client,
        string owner,
        string repoName,
        string version)
    {
        return (from m in await client.GetAllForRepository(owner, repoName)
            where m.Title == version
            select m).Any();
    }

    public static async Task<Milestone?> GetByTitle(
        this IMilestonesClient client,
        string owner,
        string repoName,
        string name)
    {
        var milestones = (from m in await client.GetAllForRepository(owner, repoName)
            where m.Title == name
            select m).ToArray();

        if (milestones.Length <= 0)
        {
            return null;
        }

        return milestones[0];
    }

    public static async Task<string> GetHtmlUrl(
        this IMilestonesClient client,
        string owner,
        string repoName,
        string name)
    {
        var milestones = (from m in await client.GetAllForRepository(owner, repoName)
            where m.Title == name
            select m).ToArray();

        return milestones.Length <= 0 ? string.Empty : milestones[0].HtmlUrl;
    }

    public static async Task<Milestone> CloseMilestone(this IMilestonesClient client, string owner, string repoName, string name)
    {
        var milestones = await client.GetAllForRepository(owner, repoName);

        var foundMilestone = milestones.FirstOrDefault(m => m.Title == name);

        if (foundMilestone is null)
        {
            throw new NotFoundException($"A milestone with the title/name '{name}' was not found", HttpStatusCode.NotFound);
        }

        var mileStoneUpdate = new MilestoneUpdate()
        {
            State = ItemState.Closed,
        };

        var updatedMilestone = await client.Update(owner, repoName, foundMilestone.Number, mileStoneUpdate);

        if (updatedMilestone is null)
        {
            throw new Exception($"The milestone '{name}' could not be updated.");
        }

        return updatedMilestone;
    }

    public static async Task<Milestone> UpdateMilestoneDescription(
        this IMilestonesClient client,
        string owner,
        string repoName,
        string milestoneName,
        string description)
    {
        var request = new MilestoneRequest() { State = ItemStateFilter.All };
        var milestones = await client.GetAllForRepository(owner, repoName, request);

        var foundMilestone = milestones.FirstOrDefault(m => m.Title == milestoneName);

        if (foundMilestone is null)
        {
            throw new NotFoundException($"A milestone with the title/name '{milestoneName}' was not found", HttpStatusCode.NotFound);
        }

        var mileStoneUpdate = new MilestoneUpdate()
        {
            Description = description,
        };

        var updatedMilestone = await client.Update(owner, repoName, foundMilestone.Number, mileStoneUpdate);

        if (updatedMilestone is null)
        {
            throw new Exception($"The milestone '{milestoneName}' description could not be updated.");
        }

        return updatedMilestone;
    }

    public static bool IsReleaseToDoIssue(this Issue issue, ReleaseType releaseType)
    {
        var releaseLabelOrTitle = releaseType switch
        {
            ReleaseType.Preview => "🚀Preview Release",
            ReleaseType.Production => "🚀Production Release",
            ReleaseType.HotFix => "🚀Hot Fix Release",
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null),
        };

        var isIssue = issue.PullRequest is null;
        var validTitle = issue.Title == releaseLabelOrTitle;
        var validLabelType = issue.Labels.Any(l => l.Name == releaseLabelOrTitle);

        return isIssue && validTitle && validLabelType;
    }

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
        var isPullRequest = issue.PullRequest is not null;
        var validLabelType = issue.Labels.Count == 1 && issue.Labels[0].Name == releaseLabelOrTitle;

        return hasValidTitle  && hasSingleLabel && isPullRequest && validLabelType;
    }

    public static string GetLogText(this Issue issue, int tabCount = 0)
    {
        var tabs = string.Empty;
        for (var i = 0; i < tabCount; i++)
        {
            tabs += " ";
        }

        var text = string.Empty;

        var prOrIssuePrefix = issue.PullRequest is null ? "Issue" : "PR";
        text += $"{Environment.NewLine}{tabs}{prOrIssuePrefix} Number: {issue.Number}";
        text += $"{Environment.NewLine}{tabs}{prOrIssuePrefix} Title: {issue.Title}";
        text += $"{Environment.NewLine}{tabs}{prOrIssuePrefix} State: {issue.State}";
        text += $"{Environment.NewLine}{tabs}{prOrIssuePrefix} Url: {issue.HtmlUrl}";
        text += $"{Environment.NewLine}{tabs}Labels ({issue.Labels.Count}):";
        issue.Labels.ForEach(l => text += $"{Environment.NewLine}{tabs}\t  - `{l.Name}`");

        return text;
    }

    public static string GetLogText(this IReadOnlyList<Issue> issues, int totalIndentSpaces = 0)
    {
        var spaces = string.Empty;
        for (var i = 0; i < totalIndentSpaces; i++)
        {
            spaces += " ";
        }

        var errorMsg = "---------------------Issue(s)---------------------";

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

    public static string BuildReleaseNotesFilePath(this Solution solution, ReleaseType releaseType, string version)
    {
        const string relativeDir = "Documentation/ReleaseNotes";

        var notesDirPath = releaseType switch
        {
            ReleaseType.Preview => $"{solution.Directory}/{relativeDir}/PreviewReleases",
            ReleaseType.Production => $"{solution.Directory}/{relativeDir}/ProductionReleases",
            _ => throw new ArgumentOutOfRangeException(nameof(releaseType), releaseType, null)
        };

        version = version.StartsWith("v")
            ? version.TrimStart("v")
            : version;

        var fileName = $"Release-Notes-v{version}.md";

        return $"{notesDirPath}/{fileName}";
    }

    public static string GetReleaseNotes(this Solution solution, ReleaseType releaseType, string version)
    {
        var fullFilePath = solution.BuildReleaseNotesFilePath(releaseType, version);

        if (File.Exists(fullFilePath) is false)
        {
            var errorMsg = "The {Value1} release notes for version '{Value2}' at file path '{Value3}'";
            errorMsg += " could not be found.";
            Log.Error(errorMsg,
                releaseType.ToString().ToLower(),
                version,
                fullFilePath.Replace(solution.Directory, "~"));
            Assert.Fail("The release notes file could not be found.");
        }

        return File.ReadAllText(fullFilePath);
    }

    public static string[] GetReleaseNotesAsLines(this Solution solution, ReleaseType releaseType, string version)
    {
        var fullFilePath = solution.BuildReleaseNotesFilePath(releaseType, version);

        if (File.Exists(fullFilePath) is false)
        {
            var errorMsg = "The {Value1} release notes for version '{Value2}' at file path '{Value3}'";
            errorMsg += " could not be found.";
            Log.Error(errorMsg,
                releaseType.ToString().ToLower(),
                version,
                fullFilePath.Replace(solution.Directory, "~"));
            Assert.Fail("The release notes file could not be found.");
        }

        return File.ReadAllLines(fullFilePath);
    }

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
    /// Returns a value indicating whether or not a branch with the given branch name
    /// matches the given <paramref name="pattern"/>.
    /// </summary>
    /// <param name="pattern">The value to check against the branch name.</param>
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

        // Remove any consecutive '#' and '*' symbols until no more consecutive symbols exists anymore
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