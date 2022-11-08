// <copyright file="IGitHubTokenService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Creates tokens depending whether or not the execution context is local or on the server.
/// </summary>
internal interface IGitHubTokenService
{
    /// <summary>
    /// Gets the token for GitHub API requests.
    /// </summary>
    /// <returns>The token.</returns>
    string GetToken();
}
