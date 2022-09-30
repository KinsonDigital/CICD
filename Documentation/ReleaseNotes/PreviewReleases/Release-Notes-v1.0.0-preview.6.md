<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.6
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div algn="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">New Features ✨</h2>

1. [#33](https://github.com/KinsonDigital/CICD/issues/33) - Added descriptions to all of the build parameters.
   - To view the build parameters and the associated descriptions, use the CLI command `dotnet cicd --help`.

---

<h2 style="font-weight:bold" align="center">Bug Fixes 🐛</h2>

1. [#33](https://github.com/KinsonDigital/CICD/issues/33) - Fixed bug where the `SecretService` was incorrectly building the path to the local secrets file resulting in not finding the file and failing running targets.
   - This was only occurring on local runs.

---

<h2 style="font-weight:bold" align="center">Breaking Changes 🧨</h2>

1. [#33](https://github.com/KinsonDigital/CICD/issues/33) - Renamed the build parameter `BuildSettingsDirPath` to `WorkFlowTemplateOutput`. The CLI switch equivalent is `workflow-template-output`.
   - This build parameter functions the same and is still used to determine the directory output path for generating workflows.  The name of the parameter was not renamed in previous releases when it was repurposed.

---

<h2 style="font-weight:bold" align="center">Internal Changes ⚙️</h2>
<h5 align="center">(Changes that do not affect users.  Not breaking changes, new features, or bug fixes.)</h5>

1. [#33](https://github.com/KinsonDigital/CICD/issues/33) - Performed cleanup through various areas of the code base.
2. [#33](https://github.com/KinsonDigital/CICD/issues/33) - Removed the unused types below:
   - `ICurrentDirService` interface
   - `CurrentDirService` class
3. [#33](https://github.com/KinsonDigital/CICD/issues/33) - Created the PowerShell scripts below to help assist with testing the CICD project in a dotnet tool context.
   - CreateToolPackage.ps1
   - InstallDotNetTool.ps1

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. [#36](https://github.com/KinsonDigital/CICD/issues/36) - Created application logo in _png_ and _icon_ format.
   - Set the application icon to use the new logo.
   - Set up the NuGet package to pack and use the logo.
   - Updated the _**README.md**_ file to use the logo.  This has GitHub light and dark mode version setup.
2. [#35](https://github.com/KinsonDigital/CICD/issues/35) - Removed an unused project from the solution.
