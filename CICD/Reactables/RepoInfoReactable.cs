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
    /// <summary>
    /// Sends a push notification on repository information.
    /// </summary>
    /// <param name="data">The data to send with the push notification.</param>
    public override void PushNotification((string repoOwner, string repoName) data)
    {
        /* Work from the end to the beginning of the list
           just in case the reactable is disposed(removed)
           in the OnNext() method.
         */
        for (var i = Reactors.Count - 1; i >= 0; i--)
        {
            Reactors[i].OnNext(data);
        }
    }
}
