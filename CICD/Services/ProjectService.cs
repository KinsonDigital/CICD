// <copyright file="ProjectService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CICDSystem.Guards;
using CICDSystem.Reactables.Core;
using CICDSystem.Services.Interfaces;

namespace CICDSystem.Services;

/// <inheritdoc/>
internal sealed class ProjectService : IProjectService
{
    private const string PackageIdTagName = "PackageId";
    private const string VersionTagName = "Version";
    private const string ProjFileExtension = ".csproj";
    private const string GitHubDirName = ".github";
    private const char PosixDirSeparator = '/';
    private readonly IDisposable unsubscriber;
    private readonly ISolutionWrapper solutionWrapper;
    private readonly IFindDirService findDirService;
    private readonly IDirectory directory;
    private readonly IPath path;
    private readonly IXmlService xmlService;
    private string projectName = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectService"/> class.
    /// </summary>
    /// <param name="solutionWrapper">
    ///     Wraps the <see cref="Nuke.Common"/>.
    ///     <see cref="Nuke.Common.ProjectModel"/>.
    ///     <see cref="Nuke.Common.ProjectModel.Solution"/> functionality.
    /// </param>
    /// <param name="repoInfoReactable">Provides push notifications about repository information.</param>
    /// <param name="findDirService">Searches for directories.</param>
    /// <param name="directory">Manages directories.</param>
    /// <param name="path">Manages paths.</param>
    /// <param name="xmlService">Pulls data from XML.</param>
    public ProjectService(
        IReactable<(string, string)> repoInfoReactable,
        ISolutionWrapper solutionWrapper,
        IFindDirService findDirService,
        IDirectory directory,
        IPath path,
        IXmlService xmlService)
    {
        EnsureThat.ParamIsNotNull(repoInfoReactable, nameof(repoInfoReactable));
        EnsureThat.ParamIsNotNull(solutionWrapper, nameof(solutionWrapper));
        EnsureThat.ParamIsNotNull(findDirService, nameof(findDirService));
        EnsureThat.ParamIsNotNull(directory, nameof(directory));
        EnsureThat.ParamIsNotNull(path, nameof(path));
        EnsureThat.ParamIsNotNull(xmlService, nameof(xmlService));

        this.unsubscriber = repoInfoReactable.Subscribe(new Reactor<(string repoOwner, string repoName)>(
            onNext: repoInfoData =>
            {
                this.projectName = repoInfoData.repoName;
            }, () => this.unsubscriber?.Dispose()));
        this.solutionWrapper = solutionWrapper;
        this.findDirService = findDirService;
        this.directory = directory;
        this.path = path;
        this.xmlService = xmlService;
    }

    /// <inheritdoc/>
    [ExcludeFromCodeCoverage]
    public string GetVersion()
    {
        var projFilePath = GetProjectFilePath();

        return this.xmlService.GetTagValue(projFilePath, VersionTagName);
    }

    /// <inheritdoc/>
    public string GetPackageId()
    {
        var projFilePath = GetProjectFilePath();

        return this.xmlService.GetTagValue(projFilePath, PackageIdTagName);
    }

    /// <summary>
    /// Gets the full file path to the project file.
    /// </summary>
    /// <returns>The full file path.</returns>
    private string GetProjectFilePath()
    {
        var findDirPath = this.directory.GetCurrentDirectory().ToCrossPlatPath();
        var startDirPath = this.findDirService.FindDescendentDir(findDirPath, GitHubDirName)
            .ToCrossPlatPath();

        startDirPath = startDirPath.ToCrossPlatPath();
        var dirPathSections = startDirPath.Split(PosixDirSeparator).Where(d => d.ToLower() != GitHubDirName).ToArray();
        startDirPath = string.Join(PosixDirSeparator, dirPathSections);

        var files = this.directory.GetFiles(startDirPath, $"*{ProjFileExtension}", SearchOption.AllDirectories)
            .ToCrossPlatPaths();

        return files.FirstOrDefault(f =>
        {
            var fileName = this.path.GetFileNameWithoutExtension(f).ToLower();

            return fileName == this.projectName.ToLower();
        }) ?? string.Empty;
    }
}
