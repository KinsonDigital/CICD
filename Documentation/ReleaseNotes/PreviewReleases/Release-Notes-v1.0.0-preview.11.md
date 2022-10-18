<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.11
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div align="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">New Features ✨</h2>

1. [#96](https://github.com/KinsonDigital/CICD/issues/96) - Added new checks to hot fix pull request status checks.  The checks added are listed below:
   - Check to validate if the pull request has certain labels.
     - Uses the `ThatThePRHasTheLabel()` requirement method
   - Check to validate if the version is added to the **_csproj_** file and is valid.
     - Uses the `ThatTheProjectVersionsAreValid()` requirement method
   - Check to validate that the version embedded into the source branch name matches the version set for the project.
     - Uses the `ThatThePRSourceBranchVersionSectionMatchesProjectVersion()` requirement method
   - Check to validate that the GIT release tag does not already exist.
     - Uses the `ThatTheReleaseTagDoesNotAlreadyExist()` requirement method
   - Check to validate that the release milestone exists.
     - Uses the `ThatTheReleaseMilestoneExists()` requirement method
   - Check to validate that the milestone only contains a single standard issue and pull request.
     - Uses the `ThatTheMilestoneContainsOnlySingleItemOfType()` requirement method
   - Check to validate that all issues in the set milestone have labels.
     - Uses the `ThatAllMilestoneIssuesHaveLabels()` requirement method
   - Check to validate that all issues and pull requests in the set milestone are assigned.
     - Uses the `ThatAllMilestoneItemsAreAssigned()` requirement method
   - Check to validate that the release notes exist.
     - Uses the `ThatTheReleaseNotesExist()` requirement method
   - Check to validate that the title of the release notes is correct.
     - Uses the `ThatTheReleaseNotesTitleIsCorrect()` requirement method
   - Check to validate that all of the issues in the milestone are called out in the release notes.
     - Uses the `ThatMilestoneIssuesExistInReleaseNotes()` requirement method
   - Check to validate that the GitHub release does not already exist.
     - Uses the `ThatGitHubReleaseDoesNotExist()` requirement method
   - Check to Validate that the NuGet package does not already exist.
     - Uses the `ThatTheNugetPackageDoesNotExist()` requirement method

---

<h2 style="font-weight:bold" align="center">Bug Fixes 🐛</h2>

1. [#95](https://github.com/KinsonDigital/CICD/issues/95) - Fixed a bug where the **_Hot Fix PR Status Check_** target was checking if the preview feature issue number existed.
   - A hotfix uses a hot-fix branch, not a preview feature branch, which is why this was a bug.

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. [#90](https://github.com/KinsonDigital/CICD/issues/90) - Fixed a logging issue when checking if the release notes title was correct.
2. [#91](https://github.com/KinsonDigital/CICD/issues/91) - Fixed a logging issue when an error is checking the release notes content.
   - This was happening when a production release was being executed that had previous preview releases associated with it, but the preview releases were not mentioned in the release notes.
3. [#92](https://github.com/KinsonDigital/CICD/issues/92) - Fixed a logging issue when the checking of the release notes was successful.
   - This was a simple change to the content included with the message being logged.
4. [#93](https://github.com/KinsonDigital/CICD/issues/93) - Refactored the summary section of the milestone description when performing **_preview_** or **_production_** releases.
5. [#94](https://github.com/KinsonDigital/CICD/issues/94) - Fixed a logging issue when checking if a pull request is assigned to a milestone.