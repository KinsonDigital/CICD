// <copyright file="SkipReleaseTweetReactable.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables.Core;

namespace CICDSystem.Reactables;

/// <summary>
/// Provides push notifications on repository information.
/// </summary>
internal sealed class SkipReleaseTweetReactable : Reactable<bool>
{
    /// <summary>
    /// Sends a push notification if the release tweet should be skipped.
    /// </summary>
    /// <param name="data">The data to send with the push notification.</param>
    public override void PushNotification(bool data)
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
