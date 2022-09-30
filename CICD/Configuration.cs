// <copyright file="Configuration.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem;

using System.ComponentModel;
using Nuke.Common.Tooling;

/// <summary>
/// Provides the various build configurations for the projects.
/// </summary>
[TypeConverter(typeof(TypeConverter<Configuration>))]
internal sealed class Configuration : Enumeration
{
#pragma warning disable SA1401
#pragma warning disable CA2211
    /// <summary>
    /// Gets the debug build configuration.
    /// </summary>
    public static Configuration Debug => new () { Value = nameof(Debug) };

    /// <summary>
    /// Gets the release build configuration.
    /// </summary>
    public static Configuration Release => new () { Value = nameof(Release) };
#pragma warning restore SA1401
#pragma warning restore CA2211

    public static implicit operator string(Configuration configuration) => configuration.Value;
}
