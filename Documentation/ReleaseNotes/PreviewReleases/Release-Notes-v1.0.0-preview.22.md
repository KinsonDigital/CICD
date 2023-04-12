<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.22
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div align="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">Bug Fixes 🐛</h2>

1. [#199](https://github.com/KinsonDigital/CICD/issues/199) - Fixed an issue with sending tweets when running releases.
   > **Note:** This was due to Twitter deprecated the v1.1 API. This API was being used by NUKE itself. To solve the issue, the `TwitterService` class was rewritten to not use the Tweet Task from NUKE.
