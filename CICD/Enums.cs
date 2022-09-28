// <copyright file="Enums.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem;

/// <summary>
/// The different types of releases that can be performed.
/// </summary>
public enum ReleaseType
{
    /// <summary>
    /// A production release.
    /// </summary>
    Production,

    /// <summary>
    /// A preview release.
    /// </summary>
    Preview,

    /// <summary>
    /// A hot fix release.
    /// </summary>
    HotFix,
}

/// <summary>
/// Represents if the context of a pull request branch is the source or target branch.
/// </summary>
public enum PRBranchContext
{
    /// <summary>
    /// The pull request <c>source</c> branch that is merging <c>into</c> a <c>target</c> branch.
    /// </summary>
    Source,

    /// <summary>
    /// The pull request <c>destination</c> branch that a <c>source</c> branch is merging into.
    /// </summary>
    Target,
}

/// <summary>
/// The different types of branches used in a branch model.
/// </summary>
public enum BranchType
{
    /// <summary>
    /// The master branch.  This is for production.
    /// </summary>
    Master,

    /// <summary>
    /// The development branch.  Used for the majority of development.
    /// </summary>
    Develop,

    /// <summary>
    /// The feature branch.  The majority of development is performed using feature branches that merge into develop.
    /// </summary>
    Feature,

    /// <summary>
    /// The preview feature branch.  This is for development of preview changes and gets merged into preview branches.
    /// </summary>
    PreviewFeature,

    /// <summary>
    /// The release branch.  This is for encapsulating an arbitrary amount of changes to be released as a production release.
    /// </summary>
    /// <remarks>This gets merged into the master branch.</remarks>
    Release,

    /// <summary>
    /// The preview branch.  This is to hold an arbitrary amount of changes to be released as a preview release.
    /// </summary>
    /// <remarks>This gets merged into release branches.</remarks>
    Preview,

    /// <summary>
    /// The hot fix branch.  This is to hold necessary changes for hot fix releases.
    /// </summary>
    /// <remarks>Intended to fix something quickly with as minimum changes as possible.</remarks>
    HotFix,

    /// <summary>
    /// Any other branch besides the main branching model branches.
    /// </summary>
    Other,
}

/// <summary>
/// The types of GitHub items.
/// </summary>
public enum ItemType
{
    /// <summary>
    /// An standard GitHub issue item.
    /// </summary>
    Issue,

    /// <summary>
    /// A pull request GitHub issue item.
    /// </summary>
    PullRequest,
}

/// <summary>
/// The different types of projects.
/// </summary>
public enum ProjectTypes
{
    /// <summary>
    /// Regular projects.
    /// </summary>
    Regular,

    /// <summary>
    /// Unit test projects.
    /// </summary>
    Test,

    /// <summary>
    /// All regular and unit testing projects.
    /// </summary>
    All,
}
