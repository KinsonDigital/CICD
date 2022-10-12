<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.10
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div align="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">New Features ✨</h2>

1. [#66](https://github.com/KinsonDigital/CICD/issues/66) - Added the ability for the status checks below to check whether or not the pull request has a milestone assigned.
   - FeaturePRStatusCheck
   - PreviewFeaturePRStatusCheck
   - HotFixPRStatusCheck
   - PrevReleasePRStatusCheck
   - ProdReleasePRStatusCheck

---

<h2 style="font-weight:bold" align="center">Bug Fixes 🐛</h2>

1. [#81](https://github.com/KinsonDigital/CICD/issues/81) - Fixed a bug where an exception was being thrown during builds and unit test executions when determining the build configuration.  This was only occurring when being executed in the **_GitHub_** environment.
2. [#86](https://github.com/KinsonDigital/CICD/issues/86) - Fixed an issue where the milestone description was incorrectly constructed as well as not being utilized for preview releases.

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. [#78](https://github.com/KinsonDigital/CICD/issues/78) - Changed requirement method text printed to the console when using the `ThatFeaturePRIssueHasLabel()` requirement method.
2. [#80](https://github.com/KinsonDigital/CICD/issues/80) - Adjusted positioning of the log output for the `ThatTheProjectVersionsAreValid()` requirement method.
