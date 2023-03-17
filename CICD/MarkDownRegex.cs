// <copyright file="MarkDownRegex.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CICDSystem;

/// <summary>
/// Contains markdown specific <see cref="Regex"/>.
/// </summary>
internal static class MarkDownRegex
{
    // Matches markdown links or images or combo of the two, that are preceded with content
    private static readonly Regex MrkDwnPrecededByWhitespace = new ("(?<![a-z-*])( |\t)+!*\\[.+\\]\\(.+\\)");
    private static readonly Regex MrkDwnHeaderRegex = new ("[ \t]+#{1,6} .+");

    /// <summary>
    /// Returns a list of all the markdown links and/or images.
    /// </summary>
    /// <param name="content">The content that might contain the links and/or images.</param>
    /// <returns>The list of markdown links and/or images.</returns>
    public static IEnumerable<string> GetAllLinksOrImages(string content)
    {
        var matches = MrkDwnPrecededByWhitespace.Matches(content);

        return matches.Select(m => m.Value).ToArray();
    }

    /// <summary>
    /// Returns a list of all the markdown headers.
    /// </summary>
    /// <param name="content">The content that might contain the headers.</param>
    /// <returns>The list of markdown headers.</returns>
    public static IEnumerable<string> GetAllHeaders(string content)
    {
        var matches = MrkDwnHeaderRegex.Matches(content);

        return matches.Select(m => m.Value).ToArray();
    }
}
