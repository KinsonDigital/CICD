// <copyright file="SolutionReactable.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables.Core;
using Nuke.Common.ProjectModel;

namespace CICDSystem.Reactables;

/// <summary>
/// Provides push notifications on the solution for the build system.
/// </summary>
internal sealed class SolutionReactable : Reactable<Solution>
{
}
