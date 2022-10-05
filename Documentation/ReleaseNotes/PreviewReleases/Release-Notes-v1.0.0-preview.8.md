<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.8
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div algn="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">Bug Fixes 🐛</h2>

1. [#60](https://github.com/KinsonDigital/CICD/issues/60) - Fixed bug where branches were not being properly validated.

---

<h2 style="font-weight:bold" align="center">Internal Changes ⚙️</h2>
<h5 align="center">(Changes that do not affect users.  Not breaking changes, new features, or bug fixes.)</h5>

1. [#59](https://github.com/KinsonDigital/CICD/issues/59) - Moved the `InternalVisibleTo` attributes from the C# code to the _**.csproj**_ file.
   - This is a better solution for exposing the `internals` of a project to a unit test project.
2. [#60](https://github.com/KinsonDigital/CICD/issues/60) - Updated all of the workflow templates to improve readability and to inject the pull request number into the [NUKE](https://nuke.build/) system.
3. [#60](https://github.com/KinsonDigital/CICD/issues/60) - Added an observable API to help pass around necessary data to other parts of the build system.
4. [#60](https://github.com/KinsonDigital/CICD/issues/60) - Created a new service named `BranchValidatorService` to help validate branches.
5. [#60](https://github.com/KinsonDigital/CICD/issues/60) - Created a new service named `PullRequestService` to help deal with pull request information.
6. [#60](https://github.com/KinsonDigital/CICD/issues/60) - Removed the following services below:
   - `ServiceFactory`
   - `GitHubActionsService`
   - `GitHubClientService`
7. [#60](https://github.com/KinsonDigital/CICD/issues/60) - Increased unit tests and code coverage.
8. [#60](https://github.com/KinsonDigital/CICD/issues/60) - Cleaned up and refactored various areas of the code base.

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. [#55](https://github.com/KinsonDigital/CICD/issues/55) - Refactored various documentation throughout the code base.
