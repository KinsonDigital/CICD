<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.19
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div align="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. [#179](https://github.com/KinsonDigital/CICD/issues/179) - Updated workflows. Refer to the list below:
   - Updated all workflows to use the environment files instead of the `set-output` command.
     > **Note** This was due to the `set-output` workflow command being deprecated by GitHub. Click [here](https://github.blog/changelog/2022-10-11-github-actions-deprecating-save-state-and-set-output-commands/) for more information.
   - Updated the reusable workflow versions in the `build-status-check.yml` and `unit-testing-status-check.yml` from version _**v3.0.1**_ to _**v3.0.2**_
   - Restructured the dependencies between jobs in the `preview-release.yml` workflow.
2. [#180](https://github.com/KinsonDigital/CICD/issues/180) - Enhanced how a project _**README.md**_ file is transformed for compatible markdown for nuget.org.
