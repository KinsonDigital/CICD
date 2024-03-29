﻿// <copyright file="EnsureThat.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Guards;

using System;

/// <summary>
/// Performs analysis on particular values to ensure that a value meets a criteria,
/// then invokes behavior based on the result.
/// </summary>
internal static class EnsureThat
{
    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the given <paramref name="value"/> is null.
    /// </summary>
    /// <param name="value">The value to check.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <typeparam name="T">The restricted class type of the <paramref name="value"/>.</typeparam>
    /// <remarks>
    /// <para>
    ///     This method is intended to have the value <paramref name="paramName"/> to be the
    ///     name of the item that is null.
    /// </para>
    /// <para>
    ///     Example:  A parameter being injected into a constructor.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="value"/> is null.
    /// </exception>
    public static void ParamIsNotNull<T>(
        T? value,
        string paramName = "")
        where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName, "The parameter must not be null.");
        }
    }

    /// <summary>
    /// Throws an <see cref="ArgumentNullException"/> if the given string <paramref name="value"/>
    /// is null or empty.
    /// </summary>
    /// <param name="value">The string value to check.</param>
    /// <param name="paramName">The name of the parameter being checked.</param>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if the <paramref name="value"/> is null or empty.
    /// </exception>
    public static void StringParamIsNotNullOrEmpty(
        string value,
        string paramName = "")
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(paramName, "The string parameter must not be null or empty.");
        }
    }
}
