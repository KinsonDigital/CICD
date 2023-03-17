// <copyright file="HtmlRegex.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CICDSystem;

/// <summary>
/// Provides HTML specific <see cref="Regex"/>.
/// </summary>
internal static class HtmlRegex
{
    private static readonly Regex LinkRegEx = new ("<a(\\s+href\\s*=\\s*(\"|').+(\"|'))*>((.|\n)*?)<\\/a>");
    private static readonly Regex HeaderHtmlRegEx = new ("<h[1-6](.*>{1}).+<\\/h[1-6]>");
    private static readonly Regex ImgRegEx = new ("<img (.*)src=(\"|').+(\"|').*>", RegexOptions.None);
    private static readonly Regex HeaderStartTag = new ("<h[1-6](.*?>)");
    private static readonly Regex HeaderEndTag = new ("<\\/h[1-6]>", RegexOptions.None);
    private static readonly Regex ContainsBoldRegex = new ("style=(\"|').*(font-weight:\\s*bold).*(\"|')");
    private static readonly Regex ImgSrcAttrValueRegex = new ("src\\s*=\\s*(\"|').+?(\"|')");
    private static readonly Regex TagContentRegex = new (">.+<");
    private static readonly Regex DivStartTagRegex = new ("<div.*?>");
    private static readonly Regex DivEndTagRegex = new ("<\\/div\\s*>");
    private static readonly Regex BreakTagRegex = new ("<br\\s*\\/>");
    private static readonly Regex SoloDivStartTag = new ("^<div.*>$", RegexOptions.None);
    private static readonly Regex SoloDivEndTagRegex = new ("^<\\/div\\s*>$");
    private static readonly Regex SoloBreakTagRegex = new ("^<br\\s*\\/>$");

    /// <summary>
    /// Returns a value indicating whether or not the given <paramref name="content"/> is an HTML image.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns><b>true</b> if the <paramref name="content"/> is an image.</returns>
    public static bool IsImage(string content) => ImgRegEx.IsMatch(content);

    /// <summary>
    /// Returns all occurrences of HTML image tags and their content in the given <paramref name="htmlContent"/>.
    /// </summary>
    /// <param name="htmlContent">The content to check.</param>
    /// <returns>The list of images.</returns>
    public static string[] GetAllImages(string htmlContent)
    {
        var matches = ImgRegEx.Matches(htmlContent);

        return matches.Select(m => m.Value).ToArray();
    }

    /// <summary>
    /// Returns the value of the <b>src</b> attribute of the given <paramref name="imgHtml"/>.
    /// </summary>
    /// <param name="imgHtml">The img HTML that possibly contains the <b>src</b> attribute and value.</param>
    /// <returns>The <b>src</b> attribute value.</returns>
    public static string GetImgSrcAttrValue(string imgHtml)
    {
        var matches = ImgSrcAttrValueRegex.Matches(imgHtml);

        return matches.Count <= 0 ? string.Empty : matches.ToArray()[0].Value;
    }

    /// <summary>
    /// Returns all HTML header start tags contained in the given <paramref name="htmlContent"/>.
    /// </summary>
    /// <param name="htmlContent">The content to check.</param>
    /// <returns>The list of HTML header start tags.</returns>
    public static string[] GetHeaderStartTags(string htmlContent)
    {
        var matches = HeaderStartTag.Matches(htmlContent);

        return matches.Select(m => m.Value).ToArray();
    }

    /// <summary>
    /// Returns all HTML header end tags contained in the given <paramref name="htmlContent"/>.
    /// </summary>
    /// <param name="htmlContent">The content to check.</param>
    /// <returns>The list of HTML header end tags.</returns>
    public static string[] GetHeaderEndTags(string htmlContent)
    {
        var matches = HeaderEndTag.Matches(htmlContent);

        return matches.Select(m => m.Value).ToArray();
    }

    /// <summary>
    /// Returns a value indicating whether or not the given <paramref name="htmlTag"/> has
    /// a <b>style</b> attribute that contains a font weight of bold.
    /// </summary>
    /// <param name="htmlTag">The HTML tag.</param>
    /// <returns><b>true</b> if the style is set to bold.</returns>
    public static bool ContainsBoldStyle(string htmlTag) => ContainsBoldRegex.IsMatch(htmlTag);

    /// <summary>
    /// Returns the content between the start and end of the given <paramref name="htmlTag"/>.
    /// </summary>
    /// <param name="htmlTag">The HTML tag.</param>
    /// <returns>The content between the start and end tag.</returns>
    public static string GetTagContent(string htmlTag)
    {
        var matches = TagContentRegex.Matches(htmlTag);

        return matches.Count <= 0 ? string.Empty : matches.ToArray()[0].Value;
    }

    /// <summary>
    /// Returns a list of all the HTML links with contained content from the given <paramref name="content"/>.
    /// </summary>
    /// <param name="content">The content that might contain zero or more HTML link tags.</param>
    /// <returns>The list of HTML links.</returns>
    public static string[] GetAllHtmlLinksWithContent(string content)
    {
        var matches = LinkRegEx.Matches(content);

        return matches.Select(m => m.Value).ToArray();
    }

    /// <summary>
    /// Returns a list of all the HTML headers with contained content from the given <paramref name="content"/>.
    /// </summary>
    /// <param name="content">The content that might contain zero or more HTML header tags.</param>
    /// <returns>The list of HTML headers.</returns>
    public static string[] GetAllHtmlHeadersWithContent(string content)
    {
        var matches = HeaderHtmlRegEx.Matches(content);

        return matches.Select(m => m.Value).ToArray();
    }

    /// <summary>
    /// Returns a value indicating whether or not the given <paramref name="content"/> is a solo div start tag.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns><b>true</b> if a solo tag.</returns>
    public static bool IsSoloDivStartTag(string content) => SoloDivStartTag.IsMatch(content);

    /// <summary>
    /// Returns a value indicating whether or not the given <paramref name="content"/> is a solo div end tag.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns><b>true</b> if a solo tag.</returns>
    public static bool IsSoloDivEndTag(string content) => SoloDivEndTagRegex.IsMatch(content);

    /// <summary>
    /// Returns a value indicating whether or not the given <paramref name="content"/> is a solo div break tag.
    /// </summary>
    /// <param name="content">The content to check.</param>
    /// <returns><b>true</b> if a solo tag.</returns>
    public static bool IsSoloBreakTag(string content) => SoloBreakTagRegex.IsMatch(content);

    /// <summary>
    /// Returns a list of all <b>div</b> start tags in the given <paramref name="htmlContent"/>.
    /// </summary>
    /// <param name="htmlContent">The content to check.</param>
    /// <returns>The list of start tags.</returns>
    public static IEnumerable<string> GetAllDivStartTags(string htmlContent)
    {
        var matches = DivStartTagRegex.Matches(htmlContent);

        return matches.Select(m => m.Value).ToArray();
    }

    /// <summary>
    /// Returns a list of all <b>div</b> end tags in the given <paramref name="htmlContent"/>.
    /// </summary>
    /// <param name="htmlContent">The content to check.</param>
    /// <returns>The list of start tags.</returns>
    public static IEnumerable<string> GetAllDivEndTags(string htmlContent)
    {
        var matches = DivEndTagRegex.Matches(htmlContent);

        return matches.Select(m => m.Value).ToArray();
    }

    /// <summary>
    /// Returns a list of all <b>break</b> tags in the given <paramref name="htmlContent"/>.
    /// </summary>
    /// <param name="htmlContent">The content to check.</param>
    /// <returns>The list of start tags.</returns>
    public static IEnumerable<string> GetAllBreakTags(string htmlContent)
    {
        var matches = BreakTagRegex.Matches(htmlContent);

        return matches.Select(m => m.Value).ToArray();
    }
}
