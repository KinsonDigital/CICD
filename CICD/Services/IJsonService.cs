﻿// <copyright file="IJsonService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services;

/// <summary>
/// Performs JSON services.
/// </summary>
public interface IJsonService
{
    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <typeparam name="T">The object type to serialize.</typeparam>
    /// <returns>A JSON string representation of the object.</returns>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <returns>The deserialized object from the JSON string.</returns>
    T? Deserialize<T>(string value);
}
