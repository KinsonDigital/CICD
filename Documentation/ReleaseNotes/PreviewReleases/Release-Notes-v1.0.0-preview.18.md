<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.18
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div align="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">New Features ✨</h2>

1. [#168](https://github.com/KinsonDigital/CICD/issues/168) - Added the ability to allow dependabot branches in _**features**_ and _**preview features**_.

---

<h2 style="font-weight:bold" align="center">Breaking Changes 🧨</h2>

1. [#157](https://github.com/KinsonDigital/CICD/issues/157) - Added the ability to skip checks for existing NuGet packages.
   > **Note** By default, checks are not performed.  This is why this is a breaking change.  If your project requires that NuGet checks be done, you need to add the setting `"SkipNuGetChecks": false` to your **parameters.json** file.  The default value of skip nuget checks is true if the setting is not provided.

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. [#166](https://github.com/KinsonDigital/CICD/issues/166) - Set up dependabot to help automate dependency management.
2. [#164](https://github.com/KinsonDigital/CICD/issues/164) - Updated Twitter links in the project readme file to point to the organizational resource.
3. [#161](https://github.com/KinsonDigital/CICD/issues/161) - Renamed license file.
4. [#159](https://github.com/KinsonDigital/CICD/issues/159) - Removed unused documents and images from the repository.
   > **Note** Some of these were simply moved to the organization level.
5. [#154](https://github.com/KinsonDigital/CICD/issues/154) - Updated the workflow templates used when generating them to use the new GitHub syntax for setting step outputs.
   > **Warning** This was required due to the old way of setting outputs being deprecated.  If you have not updated your workflows, make sure to do so.  Click [here](https://github.blog/changelog/2022-10-11-github-actions-deprecating-save-state-and-set-output-commands/) for more information.
6. [#151](https://github.com/KinsonDigital/CICD/issues/151) - Updated the version of the [checkout action](https://github.com/marketplace/actions/checkout) from _**v2**_ to _**v3**_ for generated workflows.
