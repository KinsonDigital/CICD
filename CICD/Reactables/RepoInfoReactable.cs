// <copyright file="RepoInfoReactable.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables.Core;

namespace CICDSystem.Reactables;

/// <summary>
/// Provides push notifications on repository information.
/// </summary>
internal sealed class RepoInfoReactable : Reactable<(string repoOwner, string repoName)>
{
}
