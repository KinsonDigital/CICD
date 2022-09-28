// <copyright file="JsonService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace CICDSystem.Services;

/// <summary>
/// Performs JSON services.
/// </summary>
[ExcludeFromCodeCoverage]
internal class JsonService : IJsonService
{
    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
        };

        return JsonSerializer.Serialize(value, options);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value);
}
