// <copyright file="Enums.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem;

/// <summary>
/// The different types of releases that can be performed.
/// </summary>
internal enum ReleaseType
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
/// Represents whether or not the context of a pull request branch is the source or target branch.
/// </summary>
internal enum PRBranchContext
{
    /// <summary>
    /// The pull request <c>source</c> branch that is merging <c>into</c> a <c>target</c> branch.
    /// </summary>
    Source,

    /// <summary>
    /// The pull request <c>destination</c> branch where a <c>source</c> branch is merging.
    /// </summary>
    Target,
}

/// <summary>
/// The different types of branches used in a branch model.
/// </summary>
internal enum BranchType
{
    /// <summary>
    /// The branch for production.
    /// </summary>
    Master,

    /// <summary>
    /// The branch that contains the majority of development work.
    /// </summary>
    Develop,

    /// <summary>
    /// The branch that contains smaller pieces of development work that merges into the <see cref="Develop"/> branch.
    /// </summary>
    Feature,

    /// <summary>
    /// The branch that contains smaller pieces of development work that merges into <see cref="Preview"/> branches.
    /// </summary>
    /// <remarks>This gets merged into <see cref="Preview"/> branches.</remarks>
    PreviewFeature,

    /// <summary>
    /// This branch that contains the majority of development work for a release.
    /// </summary>
    /// <remarks>This gets merged into the <see cref="Master"/> branch.</remarks>
    Release,

    /// <summary>
    /// The branch that contains changes for a preview release.
    /// </summary>
    /// <remarks>This gets merged into <see cref="Release"/> branches.</remarks>
    Preview,

    /// <summary>
    /// This branch that contains a hot fix.
    /// </summary>
    /// <remarks>
    ///     Intended to fix something quickly with as few changes as possible.
    ///     This gets merged into the <see cref="Master"/> branch.
    /// </remarks>
    HotFix,

    /// <summary>
    /// Any other branch not previously mentioned.
    /// </summary>
    Other,
}

/// <summary>
/// The types of GitHub items.
/// </summary>
internal enum ItemType
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
internal enum ProjectTypes
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
