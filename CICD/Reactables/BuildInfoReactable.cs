// <copyright file="BuildInfoReactable.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using CICDSystem.Reactables.Core;
using CICDSystem.Reactables.ReactableData;

namespace CICDSystem.Reactables;

/// <summary>
/// Creates a reactable to send push notifications to pass along information about a repository and the project.
/// </summary>
internal class BuildInfoReactable : Reactable<BuildInfoData>
{
    /// <summary>
    /// Sends a push notification to pass on repository and project information.
    /// </summary>
    /// <param name="data">The data to send with the push notification.</param>
    public override void PushNotification(BuildInfoData data)
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
