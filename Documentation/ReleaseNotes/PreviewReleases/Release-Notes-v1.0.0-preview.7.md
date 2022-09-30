<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.7
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div algn="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">New Features ✨</h2>

---

<h2 style="font-weight:bold" align="center">Bug Fixes 🐛</h2>

1. [#43](https://github.com/KinsonDigital/CICD/issues/43) - Fixed a bug where checking if a pull request exists was only working for local builds.
2. [#42](https://github.com/KinsonDigital/CICD/issues/42) - Fixed a bug where [NUKE](https://nuke.build/) was being allowed to pull the pull request number for non-pull request builds.
    - This was causing an error within NUKE itself and breaking the build.

---

<h2 style="font-weight:bold" align="center">Breaking Changes 🧨</h2>

---

<h2 style="font-weight:bold" align="center">Internal Changes ⚙️</h2>
<h5 align="center">(Changes that do not affect users.  Not breaking changes, new features, or bug fixes.)</h5>

---

<h2 style="font-weight:bold" align="center">Nuget/Library Updates 📦</h2>

---

<h2 style="font-weight:bold" align="center">Other 🪧</h2>
<h5 align="center">(Includes anything that does not fit into the categories above)</h5>

1. [#42](https://github.com/KinsonDigital/CICD/issues/42) - Refactored code to follow project coding style policies.
2. [#42](https://github.com/KinsonDigital/CICD/issues/42) - Removed dead code.
