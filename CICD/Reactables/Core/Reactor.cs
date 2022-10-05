﻿// <copyright file="Reactor.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

// ReSharper disable InconsistentNaming
using System;

namespace CICDSystem.Reactables.Core;

/// <summary>
/// Provides a mechanism for receiving push-based notifications.
/// </summary>
/// <typeparam name="T">The information related to the notification.</typeparam>
internal sealed class Reactor<T> : IReactor<T>
{
    private readonly Action<T>? onNext;
    private readonly Action? onCompleted;
    private readonly Action<Exception>? onError;
    private bool completed;

    /// <summary>
    /// Initializes a new instance of the <see cref="Reactor{T}"/> class.
    /// </summary>
    /// <param name="onNext">Executed when a push notification occurs.</param>
    /// <param name="onCompleted">Executed when the provider has finished sending push-based notifications.</param>
    /// <param name="onError">Executed when the provider experiences an error.</param>
    public Reactor(Action<T>? onNext = null, Action? onCompleted = null, Action<Exception>? onError = null)
    {
        this.onNext = onNext;
        this.onCompleted = onCompleted;
        this.onError = onError;
    }

    /// <inheritdoc />
    public void OnNext(T value)
    {
        if (this.completed)
        {
            return;
        }

        this.onNext?.Invoke(value);
    }

    /// <inheritdoc />
    public void OnCompleted()
    {
        if (this.completed)
        {
            return;
        }

        this.onCompleted?.Invoke();
        this.completed = true;
    }

    /// <inheritdoc />
    public void OnError(Exception error) => this.onError?.Invoke(error);
}
