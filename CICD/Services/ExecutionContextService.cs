// <copyright file="ExecutionContextService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace CICDSystem.Services;

/// <inheritdoc/>
[ExcludeFromCodeCoverage]
internal class ExecutionContextService : IExecutionContextService
{
    /// <inheritdoc/>
    public bool IsLocalBuild => Nuke.Common.NukeBuild.IsLocalBuild;

    /// <inheritdoc/>
    public bool IsServerBuild => Nuke.Common.NukeBuild.IsServerBuild;
}
