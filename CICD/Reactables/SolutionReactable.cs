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
    /// <summary>
    /// Sends a push notification of the solution as data for the build system.
    /// </summary>
    /// <param name="data">The data to send with the push notification.</param>
    public override void PushNotification(Solution data)
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
