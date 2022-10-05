// <copyright file="Reactable.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CICDSystem.Guards;

namespace CICDSystem.Reactables.Core;

/// <summary>
/// Defines a provider for push-based notifications.
/// </summary>
/// <typeparam name="TData">The data to send with the notification.</typeparam>
internal abstract class Reactable<TData> : IReactable<TData>
{
    private readonly List<IReactor<TData>> reactors = new ();
    private bool isDisposed;

    /// <summary>
    /// Gets the list of reactors that are subscribed to this <see cref="Reactable{TData}"/>.
    /// </summary>
    public ReadOnlyCollection<IReactor<TData>> Reactors => new (this.reactors);

    /// <inheritdoc/>
    public bool NotificationsEnded { get; private set; }

    /// <inheritdoc/>
    public virtual IDisposable Subscribe(IReactor<TData> reactor)
    {
        EnsureThat.ParamIsNotNull(reactor, nameof(reactor));

        if (!this.reactors.Contains(reactor))
        {
            this.reactors.Add(reactor);
        }

        return new ReactorUnsubscriber<TData>(this.reactors, reactor);
    }

    /// <inheritdoc/>
    public abstract void PushNotification(TData data);

    /// <inheritdoc/>
    public void EndNotifications()
    {
        if (NotificationsEnded)
        {
            return;
        }

        /* Keep this loop as a for-loop.  Do not convert to for-each.
         * This is due to the Dispose() method possibly being called during
         * iteration of the reactors list which will cause an exception.
        */
        for (var i = this.reactors.Count - 1; i >= 0; i--)
        {
            this.reactors[i].OnCompleted();
        }

        NotificationsEnded = true;
    }

    /// <inheritdoc/>
    public void UnsubscribeAll() => this.reactors.Clear();

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// <inheritdoc cref="IDisposable.Dispose"/>
    /// </summary>
    /// <param name="disposing">Disposes managed resources when <c>true</c>.</param>
    private void Dispose(bool disposing)
    {
        if (this.isDisposed)
        {
            return;
        }

        if (disposing)
        {
            this.reactors.Clear();
        }

        this.isDisposed = true;
    }
}
