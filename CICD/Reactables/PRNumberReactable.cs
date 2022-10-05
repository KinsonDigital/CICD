// <copyright file="PRNumberReactable.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables.Core;

namespace CICDSystem.Reactables;

/// <summary>
/// Provides push notifications on the product name.
/// </summary>
internal sealed class PRNumberReactable : Reactable<int>
{
    /// <summary>
    /// Sends a push notification of a pull request number.
    /// </summary>
    /// <param name="data">The data to send with the push notification.</param>
    public override void PushNotification(int data)
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
