<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.15
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div align="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">New Features ✨</h2>

1. [#106](https://github.com/KinsonDigital/CICD/issues/106) - Add the ability to skip a release and only run the associated requirement checks.
   > **💡** This can be done by using the `--release-checks-only` switch when running locally via the CLI and with workflows.
   > This can also be accomplished by using the _**ReleaseChecksOnly**_ build parameter in the NUKE _**parameters.json**_ file.

---

<h2 style="font-weight:bold" align="center">Bug Fixes 🐛</h2>

1. [#129](https://github.com/KinsonDigital/CICD/issues/129) - Fixed a bug where the checking of an existing NuGet package of a particular version was not working.

---

<h2 style="font-weight:bold" align="center">Internal Changes ⚙️</h2>
<h5 align="center">(Changes that do not affect users.  Not breaking changes, new features, or bug fixes.)</h5>

1. [#117](https://github.com/KinsonDigital/CICD/issues/117) - Added the ability to catch GitHub API validation request issues due to tag protection rules existing on a repository.
2. [#109](https://github.com/KinsonDigital/CICD/issues/109) - Improved logging to the console when using an incorrect pull request number.
3. [#108](https://github.com/KinsonDigital/CICD/issues/108) - Improved logging and handling of issues with local secrets files.

---

<h2 style="font-weight:bold" align="center">Nuget/Library Updates 📦</h2>

1. [#129](https://github.com/KinsonDigital/CICD/issues/129) - Made the dependency changes below:
   - Added the NuGet package **Flurl.Http** version _**v3.2.4**_
        > **💡** This is used for creating HTTP requests and replaced the **RestSharp** HTTP client library.
   - Removed the NuGet package **RestSharp**.
        > **💡** This was replaced by the **Flurl.Http** library.
   - Updated the NuGet package **System.IO.Abstractions** from version _**v17.2.3**_ to _**v19.1.1**_

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. [#106](https://github.com/KinsonDigital/CICD/issues/106) - Added solution configurations to the solution for [JetBrains Rider](https://www.jetbrains.com/rider/) users.
   > **💡** These configurations are for making it easier to run specific NUKE tasks during development.
