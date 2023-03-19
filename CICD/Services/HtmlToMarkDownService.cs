// <copyright file="HtmlToMarkDownService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;
using System.Linq;
using System.Text;
using CICDSystem.Guards;
using CICDSystem.Services.Interfaces;

namespace CICDSystem.Services;

/// <inheritdoc/>
internal class HtmlToMarkDownService : IHtmlToMarkDownService
{
    /// <inheritdoc/>
    public string LinkToMarkDown(string linkHtml)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(linkHtml, nameof(linkHtml));

        var matches = HtmlRegex.GetAllHtmlLinksWithContent(linkHtml);

        switch (matches.Length)
        {
            case <= 0:
                throw new ArgumentException("The HTML does not contain a valid link tag.", nameof(linkHtml));
            case > 1:
                throw new ArgumentException("Contains too many link tags.", nameof(linkHtml));
        }

        var startTag = $"{linkHtml.Split(">")[0]}>";

        var url = startTag.Replace("<a href=", string.Empty);
        url = url.Replace(">", string.Empty);
        url = url.Replace("\"", string.Empty);
        url = url.Replace("'", string.Empty);

        var noUrl = url is "#" || url == string.Empty;
        var content = GetLinkHtmlContent(linkHtml);

        content = HtmlRegex.IsImage(content)
            ? ImgToMarkDown(content)
            : content;

        return noUrl ? content : $"[{content}]({url})";
    }

    /// <inheritdoc/>
    public string ImgToMarkDown(string imgHtml)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(imgHtml, nameof(imgHtml));

        var matches = HtmlRegex.GetAllImages(imgHtml);

        switch (matches.Length)
        {
            case <= 0:
                throw new ArgumentException("The HTML does not contain a valid image tag.", nameof(imgHtml));
            case > 1:
                throw new ArgumentException("Contains too many image tags.", nameof(imgHtml));
        }

        imgHtml = imgHtml.Replace("\"", "'");

        var attrValueMatches = HtmlRegex.GetImgSrcAttrValue(imgHtml);

        if (string.IsNullOrEmpty(attrValueMatches))
        {
            throw new ArgumentException("The image tag must contain a 'src' attribute with a value.", nameof(imgHtml));
        }

        var url = attrValueMatches;
        url = url.Replace("src=", string.Empty);
        url = url.Replace("'", string.Empty);
        url = url.Replace("\"", string.Empty);

        return $"![image]({url})";
    }

    /// <inheritdoc/>
    public string HeaderToMarkDown(string headerHtml)
    {
        EnsureThat.StringParamIsNotNullOrEmpty(headerHtml, nameof(headerHtml));

        var matches = HtmlRegex.GetAllHtmlHeadersWithContent(headerHtml);

        switch (matches.Length)
        {
            case <= 0:
                throw new ArgumentException("The HTML does not contain a valid image tag.", nameof(headerHtml));
            case > 1:
                throw new ArgumentException("Contains too many image tags.", nameof(headerHtml));
        }

        var startTagMatches = HtmlRegex.GetHeaderStartTags(headerHtml);

        var startTag = string.Empty;
        var endTag = string.Empty;

        if (startTagMatches.Any())
        {
            startTag = startTagMatches[0];
        }

        var endTagMatches = HtmlRegex.GetHeaderEndTags(headerHtml);

        if (endTagMatches.Any())
        {
            endTag = endTagMatches[0];
        }

        var content = headerHtml.Replace(startTag, string.Empty);
        content = content.Replace(endTag, string.Empty);

        var headerLevel = int.Parse(startTag.Replace("<h", string.Empty)[0].ToString());
        var headerHash = CreateHeaderHash(headerLevel);

        var isBold = HtmlRegex.ContainsBoldStyle(headerHtml);

        var boldChars = isBold ? "**" : string.Empty;

        return $"{headerHash} {boldChars}{content}{boldChars}";
    }

    /// <summary>
    /// Creates a markdown header string of <c>#</c> characters based on the given quantity.
    /// </summary>
    /// <param name="quantity">The quantity of <c>#</c> characters.</param>
    /// <returns>The hash symbols that precede a markdown header.</returns>
    private static string CreateHeaderHash(int quantity)
    {
        var result = new StringBuilder();

        for (var i = 0; i < quantity; i++)
        {
            result.Append('#');
        }

        return result.ToString();
    }

    /// <summary>
    /// Gets the content of an HTML link tag.
    /// </summary>
    /// <param name="linkHtml">The HTML link.</param>
    /// <returns>The content of the link HTML.</returns>
    private static string GetLinkHtmlContent(string linkHtml)
    {
        if (string.IsNullOrEmpty(linkHtml))
        {
            return string.Empty;
        }

        var content = HtmlRegex.GetTagContent(linkHtml);

        return content
            .TrimStart('>')
            .TrimEnd('<');
    }
}
