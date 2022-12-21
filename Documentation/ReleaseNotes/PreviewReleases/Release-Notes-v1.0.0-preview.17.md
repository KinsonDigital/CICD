<h1 align="center" style='color:mediumseagreen;font-weight:bold'>
    CICD Preview Release Notes - v1.0.0-preview.17
</h1>

<h2 align="center" style='font-weight:bold'>Quick Reminder</h2>

<div align="center">

As with all software, there is always a chance for issues and bugs, especially for preview releases, which is why your input is greatly appreciated. 🙏🏼
</div>

---

<h2 style="font-weight:bold" align="center">Bug Fixes 🐛</h2>

1. [#144](https://github.com/KinsonDigital/CICD/issues/144) - Fixed a bug where the build system would crash when using the `--skip-twitter-announcements` flag.
   >💡This occurred with **preview** and **production** releases.

---

<h2 style="font-weight:bold" align="center">Internal Changes ⚙️</h2>
<h5 align="center">(Changes that do not affect users.  Not breaking changes, new features, or bug fixes.)</h5>

1. [#147](https://github.com/KinsonDigital/CICD/issues/147) - Updated the project to dotnet version _**v7.0.0**_
   >💡This comes with overall performance improvements.
2. [#147](https://github.com/KinsonDigital/CICD/issues/147) - Updated the project C# language to version _**v11.0**_
3. [#52](https://github.com/KinsonDigital/CICD/issues/52) - Create unit tests to various internal extension methods.

---

<h2 style="font-weight:bold" align="center">Nuget/Library Updates 📦</h2>

1. [#147](https://github.com/KinsonDigital/CICD/issues/147) - Updated the following NuGet packages:
   - **Nuke.Common** from _**v6.2.1**_ to _**v6.3.0**_
   - **Octokit** from _**v3.0.0**_ to _**v4.0.3**_
   - **SimpleInjector** from _**v5.4.0**_ to _**v5.4.1**_
   - **Microsoft.CodeAnalysis.NetAnalyzers** from _**v6.0.0**_ to _**v7.0.0**_
   - **System.IO.Abstractions** from _**v19.1.1**_ to _**v19.1.5**_
   - **coverlet.msbuild** from _**v3.1.2**_ to _**v3.2.0**_
   - **FluentAssertions** from _**v6.7.0**_ to _**v6.8.0**_
   - **Microsoft.NET.Test.Sdk** from _**v17.3.1**_ to _**v17.4.1**_
   - **Moq** from _**v4.18.2**_ to _**v4.18.3**_
   - **coverlet.collector** from _**v3.1.2**_ to _**v3.2.0**_
