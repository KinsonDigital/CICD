// <copyright file="BuildSettings.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

/// <summary>
/// The build settings used for running the build.
/// </summary>
public class BuildSettings
{
    /// <summary>
    /// Gets or sets the GitHub owner of the project.
    /// </summary>
    /// <remarks>This is required for communicating with the GitHub API.</remarks>
    public string Owner { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the GitHub project/repository name.
    /// </summary>
    /// <remarks>This is required for communicating with the GitHub API.</remarks>
    public string MainProjectName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the main project file name.
    /// </summary>
    /// <remarks>
    ///     This is the csproj file name.  This is NOT the file path.
    /// </remarks>
    public string? MainProjectFileName { get; set; }

    /// <summary>
    /// Gets or sets the name of the documentation directory.
    /// </summary>
    public string? DocumentationDirName { get; set; }

    /// <summary>
    /// Gets or sets the name of the release notes directory.
    /// </summary>
    public string? ReleaseNotesDirName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether or not a twitter announcement should be performed.
    /// </summary>
    public bool AnnounceOnTwitter { get; set; }
}
