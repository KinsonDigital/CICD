// <copyright file="BuildInfoData.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Guards;
using CICDSystem.Reactables.Core;

namespace CICDSystem.Reactables.ReactableData;

// TODO: Create unit test

/// <summary>
/// Holds data for an <see cref="IReactable{T}"/>.
/// </summary>
internal readonly struct BuildInfoData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BuildInfoData"/> struct.
    /// </summary>
    /// <param name="repoOwner">The owner of the repository.</param>
    /// <param name="repoName">The name of the repository.</param>
    /// <param name="projectName">The name of the csharp project.</param>
    /// <param name="token">The GitHub API token.</param>
    public BuildInfoData(string repoOwner, string repoName, string projectName, string token)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(repoOwner, nameof(RepoOwner));
        EnsureThat.StringParamIsNotNullOrEmpty(repoName, nameof(RepoName));
        EnsureThat.StringParamIsNotNullOrEmpty(projectName, nameof(ProjectName));
        EnsureThat.StringParamIsNotNullOrEmpty(token, nameof(Token));

        RepoOwner = repoOwner;
        RepoName = repoName;
        ProjectName = projectName;
        Token = token;
    }

    /// <summary>
    /// Gets the owner of the repository.
    /// </summary>
    public string RepoOwner { get; }

    /// <summary>
    /// Gets the name of the repository.
    /// </summary>
    public string RepoName { get; }

    /// <summary>
    /// Gets the project name.
    /// </summary>
    public string ProjectName { get; }

    /// <summary>
    /// Gets the GitHub token.
    /// </summary>
    public string Token { get; }
}
