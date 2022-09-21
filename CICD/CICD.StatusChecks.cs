using System;
using System.Threading.Tasks;
using Nuke.Common;
using Serilog;
using Services;

public partial class CICD // StatusChecks
{
    public Target BuildStatusCheck => _ => _
        .Before(BuildAllProjects)
        .Triggers(BuildAllProjects)
        .Executes(async () =>
        {
            Log.Information("✅Starting Status Check . . .");

            PrintPullRequestInfo();
            await ValidateBranchForStatusCheck();

            Log.Information("Branch Is Valid!!");
        });


    private Target UnitTestStatusCheck => _ => _
        .Before(RunAllUnitTests)
        .Triggers(RunAllUnitTests)
        .Executes(async () =>
        {
            var msg = "💡Purpose: Verifies that all unit tests for all of the solution projects pass.";
            Log.Information(msg);
            Console.WriteLine();
            Log.Information("✅Starting Status Check . . .");

            PrintPullRequestInfo();
            await ValidateBranchForStatusCheck();

            Log.Information("Branch Is Valid!!");
        });


    private Target FeaturePRStatusCheck => _ => _
        .Requires(
            () => ThatThisIsExecutedFromPullRequest(BranchType.Develop),
            () => ThatThePRSourceBranchIsValid(BranchType.Feature),
            () => ThatFeaturePRIssueNumberExists(),
            () => ThatFeaturePRIssueHasLabel(BranchType.Feature),
            () => ThatThePRTargetBranchIsValid(BranchType.Develop),
            () => ThatThePRHasBeenAssigned(),
            () => ThatPRHasLabels()
        );


    private Target PreviewFeaturePRStatusCheck => _ => _
        .Requires(
            () => ThatThisIsExecutedFromPullRequest(BranchType.PreviewFeature),
            () => ThatThePRSourceBranchIsValid(BranchType.PreviewFeature),
            () => ThatPreviewFeaturePRIssueNumberExists(),
            () => ThatFeaturePRIssueHasLabel(BranchType.PreviewFeature),
            () => ThatThePRTargetBranchIsValid(BranchType.Preview),
            () => ThatThePRHasBeenAssigned(),
            () => ThatPRHasLabels()
        );


    private Target HotFixPRStatusCheck => _ => _
        .Requires(
            () => ThatThisIsExecutedFromPullRequest(BranchType.Master),
            () => ThatThePRSourceBranchIsValid(BranchType.HotFix),
            () => ThatPreviewFeaturePRIssueNumberExists(),
            () => ThatFeaturePRIssueHasLabel(BranchType.HotFix),
            () => ThatThePRTargetBranchIsValid(BranchType.Master),
            () => ThatThePRHasBeenAssigned(),
            () => ThatPRHasLabels()
        );


    private Target PrevReleasePRStatusCheck => _ => _
        .Requires(
            () => ThatThisIsExecutedFromPullRequest(BranchType.Release),
            () => ThatThePRSourceBranchIsValid(BranchType.Preview),
            () => ThatThePRTargetBranchIsValid(BranchType.Release),
            () => ThatThePRHasBeenAssigned(),
            () => ThatThePRHasTheLabel("🚀Preview Release"),
            () => ThatTheProjectVersionsAreValid(ReleaseType.Preview),
            () => ThatThePreviewPRBranchVersionsMatch(ReleaseType.Preview),
            () => ThatThePRSourceBranchVersionSectionMatchesProjectVersion(ReleaseType.Preview),
            () => ThatTheReleaseMilestoneExists(),
            () => ThatTheReleaseMilestoneContainsIssues(),
            () => ThatTheReleaseTagDoesNotAlreadyExist(ReleaseType.Preview),
            () => ThatAllMilestoneIssuesHaveLabels(),
            () => ThatAllOfTheReleaseMilestoneIssuesAreClosed(ReleaseType.Preview, true),
            () => ThatAllOfTheReleaseMilestonePullRequestsAreClosed(ReleaseType.Preview, true),
            () => ThatTheReleaseMilestoneOnlyContainsSingle(ReleaseType.Preview, ItemType.Issue),
            () => ThatTheReleaseMilestoneOnlyContainsSingle(ReleaseType.Preview, ItemType.PullRequest),
            () => ThatTheReleaseNotesExist(ReleaseType.Preview),
            () => ThatTheReleaseNotesTitleIsCorrect(ReleaseType.Preview),
            () => ThatMilestoneIssuesExistInReleaseNotes(ReleaseType.Preview),
            () => ThatGitHubReleaseDoesNotExist(ReleaseType.Preview),
            () => NugetPackageDoesNotExist()
        );


    private Target ProdReleasePRStatusCheck => _ => _
        .Requires(
            () => ThatThisIsExecutedFromPullRequest(BranchType.Master, BranchType.Develop),
            () => ThatThePRSourceBranchIsValid(BranchType.Release),
            () => ThatThePRTargetBranchIsValid(BranchType.Master),
            () => ThatThePRHasBeenAssigned(),
            () => ThatThePRHasTheLabel("🚀Production Release"),
            () => ThatTheProjectVersionsAreValid(ReleaseType.Production),
            () => ThatThePreviewPRBranchVersionsMatch(ReleaseType.Production),
            () => ThatThePRSourceBranchVersionSectionMatchesProjectVersion(ReleaseType.Production),
            () => ThatTheReleaseMilestoneExists(),
            () => ThatTheReleaseMilestoneContainsIssues(),
            () => ThatTheReleaseTagDoesNotAlreadyExist(ReleaseType.Production),
            () => ThatAllMilestoneIssuesHaveLabels(),
            () => ThatAllOfTheReleaseMilestoneIssuesAreClosed(ReleaseType.Production, true),
            () => ThatAllOfTheReleaseMilestonePullRequestsAreClosed(ReleaseType.Production, true),
            () => ThatTheReleaseMilestoneOnlyContainsSingle(ReleaseType.Production, ItemType.Issue),
            () => ThatTheReleaseMilestoneOnlyContainsSingle(ReleaseType.Production, ItemType.PullRequest),
            () => ThatTheReleaseNotesExist(ReleaseType.Production),
            () => ThatTheReleaseNotesTitleIsCorrect(ReleaseType.Production),
            () => ThatTheProdReleaseNotesContainsPreviewReleaseSection(),
            () => ThatTheProdReleaseNotesContainsPreviewReleaseItems(),
            () => ThatMilestoneIssuesExistInReleaseNotes(ReleaseType.Production),
            () => ThatGitHubReleaseDoesNotExist(ReleaseType.Production),
            () => NugetPackageDoesNotExist()
        );


    private Target DebugTask => _ => _
        .Executes(async () =>
        {
            Log.Information("Execution of debug task");
            // var service = new WorkflowService();

            // // var buildStatusCheckWorkflow = service.CreateBuildStatusCheckWorkflow();
            // var prodReleaseWorkflow = service.CreateProdReleaseCheckWorkflow();
        });


    private Target GenerateSettingsFile => _ => _
        .Executes(() =>
        {
            var buildSettingsService = new BuildSettingsService();
            buildSettingsService.CreateDefaultBuildSettingsFile();
        });


    private async Task ValidateBranchForStatusCheck()
    {
        var validBranch = false;
        var branch = string.Empty;

        // This is if the workflow is execution locally or manually in GitHub using workflow_dispatch
        bool ValidBranchForManualExecution()
        {
            return (this.Repo.Branch?.IsMasterBranch() ?? false) ||
                   (this.Repo.Branch?.IsDevelopBranch() ?? false) ||
                   (this.Repo.Branch?.IsFeatureBranch() ?? false) ||
                   (this.Repo.Branch?.IsPreviewFeatureBranch() ?? false) ||
                   (this.Repo.Branch?.IsPreviewBranch() ?? false) ||
                   (this.Repo.Branch?.IsReleaseBranch() ?? false) ||
                   (this.Repo.Branch?.IsHotFixBranch() ?? false);
        }

        // If the build is on the server and the GitHubActions object exists
        if (IsServerBuild && GitHubActions is not null)
        {
            validBranch = IsPullRequest()
                ? GitHubActions.BaseRef.IsPreviewBranch() || GitHubActions.BaseRef.IsReleaseBranch() ||
                  GitHubActions.BaseRef.IsDevelopBranch() || GitHubActions.BaseRef.IsMasterBranch()
                : ValidBranchForManualExecution(); // Manual execution

            branch = IsPullRequest() ? GitHubActions.BaseRef : this.Repo.Branch;
        }
        else if (IsLocalBuild || GitHubActions is null)
        {
            validBranch = ValidBranchForManualExecution();
            branch = this.Repo.Branch;
        }

        if (validBranch)
        {
            var validIssueNumber = await ValidBranchIssueNumber(branch);

            validBranch = validIssueNumber;

            if (validIssueNumber is false)
            {
                Log.Error($"The issue number '{ParseIssueNumber(branch)}' in branch '{branch}' does not exist.");
            }
        }

        if (validBranch is false)
        {
            Assert.Fail($"The branch '{branch}' is invalid.");
        }
    }

    private async Task<bool> ValidBranchIssueNumber(string branch)
    {
        // If the branch is not a branch with an issue number, return as valid
        if (!branch.IsFeatureBranch() && !branch.IsPreviewFeatureBranch() && !branch.IsHotFixBranch())
        {
            return true;
        }

        var issueNumber = ParseIssueNumber(branch);

        var issueClient = GitHubClient.Issue;

        return await issueClient.IssueExists(Owner, MainProjName, issueNumber);
    }

    private int ParseIssueNumber(string branch)
    {
        if (string.IsNullOrEmpty(branch))
        {
            return 0;
        }

        if (branch.IsFeatureBranch())
        {
            // feature/123-my-branch
            var mainSections = branch.Split("/");
            var number = mainSections[1].Split('-')[0];
            return int.Parse(number);
        }

        if (branch.IsPreviewFeatureBranch())
        {
            // preview/feature/123-my-preview-branch
            var mainSections = branch.Split("/");
            var number = mainSections[2].Split('-')[0];
            return int.Parse(number);
        }

        if (branch.IsHotFixBranch())
        {
            // hotfix/123-my-hotfix
            var mainSections = branch.Split("/");
            var number = mainSections[1].Split('-')[0];
            return int.Parse(number);
        }

        return 0;
    }

    // TODO: Check to see if the 'tasks' in an issue can be seen in the returned JSON data, or with the Octokit issue object
    // If so, we can check to make sure that all checkboxes (tasks) are complete in all of the issues in a milestone
}
