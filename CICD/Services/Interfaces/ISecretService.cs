// <copyright file="ISecretService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Generates workflow files for the targets in the Nuke build system.
/// </summary>
internal interface ISecretService
{
    /// <summary>
    /// Loads a secret with the given <paramref name="secretName"/> from the <c>local-secrets.json</c>
    /// file located in the <c>./.github</c> directory.
    /// </summary>
    /// <param name="secretName">The name of the secret to load.</param>
    /// <returns>The secret.</returns>
    string LoadSecret(string secretName);
}
