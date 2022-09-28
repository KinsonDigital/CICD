<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.5
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div algn="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">New Features ✨</h2>

1. [#22](https://github.com/KinsonDigital/CICD/issues/22) - Added the new services below to help abstract various types and concepts [NUKE](https://nuke.build/) for the purpose of increased testability as well as running local builds.
   - `GitHubActionsService` - Wraps and enhances GitHub action related services for local and server runs.
   - `GitHubClientService` - Wraps and enhances GitHub HTTP client related services for local and server runs.
   - `GitHubTokenService` - Gets the GitHub token for GitHub API auth requests depending on local or server runs.
   - `GitRepoService` - Wraps and enhances GIT repository services for local and server runs.
2. [#22](https://github.com/KinsonDigital/CICD/issues/22) - Fixed various console log grammar and spelling issues.
3. [#22](https://github.com/KinsonDigital/CICD/issues/22) - Added a new build parameter named `PullRequestNumber` for build targets that need to communicate with the GitHub API to access pull request data when running builds locally.  This enables local runs.
4. [#22](https://github.com/KinsonDigital/CICD/issues/22) - Refactored all status checks to be able to be executed locally.  This enables the status checks to all be executed locally.

---

<h2 style="font-weight:bold" align="center">Bug Fixes 🐛</h2>

1. [#22](https://github.com/KinsonDigital/CICD/issues/22) - Fixed various small issues.

---

<h2 style="font-weight:bold" align="center">Breaking Changes 🧨</h2>

1. [#26](https://github.com/KinsonDigital/CICD/issues/26) - Refactor all classes, interfaces, and enums to `internal`.  Also set all of the classes to `sealed`.
   - This library is meant to only be used for CICD purposes for the [KinsonDigital](https://github.com/KinsonDigital) organization.  
   If for some reason somebody has been using this library in it's so far short life, this would be a breaking change.
2. [#22](https://github.com/KinsonDigital/CICD/issues/22) - Renamed the status checks below.  Workflow changes for your project will be required to invoke the correct target.
   - Target `BuildStatusCheck` renamed to `PRBuildStatusCheck`
   - Target `UnitTestStatusCheck` renamed to `PRUnitTestStatusCheck`
3. [#22](https://github.com/KinsonDigital/CICD/issues/22) - Refactored the names of the types listed below.
   - Renamed the `ITokenFactory` interface to `IGitHubTokenService`
   - Renamed the `TokenFactory` class to `GitHubTokenService`

---

<h2 style="font-weight:bold" align="center">Internal Changes ⚙️</h2>
<h5 align="center">(Changes that do not affect users.  Not breaking changes, new features, or bug fixes.)</h5>

[#7](https://github.com/KinsonDigital/CICD/issues/7) - Added the **_LICENSE_** file to the NuGet package.

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. [#7](https://github.com/KinsonDigital/CICD/issues/7) - Refactoring and code cleanup during issue implementation.
2. [#12](https://github.com/KinsonDigital/CICD/issues/12) - Refactoring and code cleanup during issue implementation.
3. [#22](https://github.com/KinsonDigital/CICD/issues/22) - Refactoring and code cleanup during issue implementation.
4. [#26](https://github.com/KinsonDigital/CICD/issues/26) - Refactoring and code cleanup during issue implementation.
5. [#12](https://github.com/KinsonDigital/CICD/issues/12) - Updated **_README.md_** file to be more in context with the project.
