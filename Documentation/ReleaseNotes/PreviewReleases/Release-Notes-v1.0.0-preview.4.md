<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.4
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div algn="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">New Features ✨</h2>

1. [#19](https://github.com/KinsonDigital/CICD/issues/19) - Created the list of services below to help abstract the way local vs server builds.
   - `CurrentDirService` - Gets the current directory of where the application is executing.  This increases testability.
   - `ExecutionContextService` - Determines if the build is running locally or on the server.  This increases testability.
   - `GitHubClientService` - Creates a GitHubClient singleton with authorized requests.  This makes it much easier to be able to deal with the `GitHubClient` locally and on the server.
   - `JsonService` - Serializes and deserializes JSON data using abstraction to increase testability.
   - `SecretService` - Loads secrets locally or on the server depending on where the build is running.
2. [#19](https://github.com/KinsonDigital/CICD/issues/19) - Created factories to help generate HTTP clients and tokens.
   - `HttpClientFactory` - Abstracts away the process of creating HTTP clients.  Increases testability.
   - `TokenFactory` - Abstracts the retrieval of a token depending on if the build is running locally or on the server.
3. [#19](https://github.com/KinsonDigital/CICD/issues/19) - Created PowerShell script to quickly create a NuGet package dotnet tool for testing purposes.
4. [#17](https://github.com/KinsonDigital/CICD/issues/17) - Created a new build parameter named `PreviewReleaseNotesDirName`.
   - This can be used in the `.nuke/parameters.json` file or the CLI using `--preview-release-notes-dir-name` to set the name of the preview release notes directory.
   - The build parameters are optional.
5. [#17](https://github.com/KinsonDigital/CICD/issues/17) - Created a new build parameter named `ProductionReleaseNotesDirName`.
   - This can be used in the `.nuke/parameters.json` file or the CLI using `--production-release-notes-dir-name` to set the name of the production release notes directory.
   - The build parameter is optional.
6. [#17](https://github.com/KinsonDigital/CICD/issues/17) - Created a new build parameter named `ReleaseNotesBaseDirPath`.
   - This can be used in the `.nuke/parameters.json` file or the CLI using --release-notes-base-dir-path` to set the base directory where preview and production release notes are located.
   - The build parameter is optional.
7. [#15](https://github.com/KinsonDigital/CICD/issues/15) - Created a new build parameter named `ProjectName`.
   - This can be used in the `.nuke/parameters.json` file or the CLI using `--project-name` to set the name of the C# project.
   - The build parameter is mandatory.
8. [#15](https://github.com/KinsonDigital/CICD/issues/15) - Created a new build parameter named `RepoName`.
   - This can be used in the `.nuke/parameters.json` file or the CLI using `--repo-name` to set the name of the repository for the project.
   - The build parameter is mandatory.
9. [#15](https://github.com/KinsonDigital/CICD/issues/15) - Created a new build parameter named `RepoOwner`.
   - This can be used in the `.nuke/parameters.json` file or the CLI using `--repo-owner` to set the name of the repository for the project.
   - The build parameter is mandatory.

---

<h2 style="font-weight:bold" align="center">Bug Fixes 🐛</h2>

1. [#14](https://github.com/KinsonDigital/CICD/issues/14) - Fix bugs in all of the workflow templates.
   - This fixed an issue where the NUKE target execution was being executed with standard PowerShell instead of in the context of a dotnet tool.

---

<h2 style="font-weight:bold" align="center">Internal Changes ⚙️</h2>
<h5 align="center">(Changes that do not affect users.  Not breaking changes, new features, or bug fixes.)</h5>

1. [#19](https://github.com/KinsonDigital/CICD/issues/19) - Implemented dependency injection.
2. [#19](https://github.com/KinsonDigital/CICD/issues/19) - Created unit tests for various services.
3. [#19](https://github.com/KinsonDigital/CICD/issues/19) - Created a guard for checking null or empty parameters.

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. [#19](https://github.com/KinsonDigital/CICD/issues/19) - Cleanup of many areas of the code base.
    - This includes removing dead code as well as refactoring code to make it more readable and to follow project code styling policies.
2. [#13](https://github.com/KinsonDigital/CICD/issues/13) - Added default shell to all workflow templates to be PowerShell core.
