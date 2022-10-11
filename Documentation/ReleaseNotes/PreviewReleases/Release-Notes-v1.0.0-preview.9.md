<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.9
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div algn="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">New Features ✨</h2>

1. [#65](https://github.com/KinsonDigital/CICD/issues/65) - Created a new target to display the version of the CICD project.
   - The name of the target is called `Version`.  Just type `dotnet cicd Version` to display the version in the console.

---

<h2 style="font-weight:bold" align="center">Bug Fixes 🐛</h2>

1. [#67](https://github.com/KinsonDigital/CICD/issues/67) - Fixed a bug where release todo issues and pull requests contained in milestones were not being skipped during milestone state checks.
2. [#69](https://github.com/KinsonDigital/CICD/issues/69) - Fixed a bug when checking if a NuGet package exists that has never existed before, would throw an exception instead of returning `true` or `false`.
   - This would occur if the package does not exist in **_nuget.org_**.

---

<h2 style="font-weight:bold" align="center">Internal Changes ⚙️</h2>
<h5 align="center">(Changes that do not affect users.  Not breaking changes, new features, or bug fixes.)</h5>

1. [#27](https://github.com/KinsonDigital/CICD/issues/27) - Created a new internal service to provide functionality with solutions.
2. [#27](https://github.com/KinsonDigital/CICD/issues/27) - Created a thin wrapper around the [NUKE](https://nuke.build/) solution functionality.
3. [#27](https://github.com/KinsonDigital/CICD/issues/27) - Simple cleanup and code refactoring to maintain project coding style policies.
4. [#53](https://github.com/KinsonDigital/CICD/issues/53) - Added unit tests for the extension methods below:
   - `ReleaseExists()`
   - `MilestoneExists()`
   - `GetByTitle()`
   - `GetHtmlUrl()`
   - `CloseMilestone()`
   - `UpdateMilestoneDescription()`
   - `GetLogText()`
