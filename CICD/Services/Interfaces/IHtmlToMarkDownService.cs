// <copyright file="IHtmlToMarkDownService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services.Interfaces;

/// <summary>
/// Transpiles HTML to markdown.
/// </summary>
internal interface IHtmlToMarkDownService
{
    /// <summary>
    /// Converts the given link tag HTML to an equivalent markdown version.
    /// </summary>
    /// <param name="linkHtml">The link HTML.</param>
    /// <returns>The markdown equivalent.</returns>
    string LinkToMarkDown(string linkHtml);

    /// <summary>
    /// Converts the given tag tag HTML to an equivalent markdown version.
    /// </summary>
    /// <param name="imgHtml">The tag HTML.</param>
    /// <returns>The markdown equivalent.</returns>
    string ImgToMarkDown(string imgHtml);

    /// <summary>
    /// Converts the given header tag HTML to an equivalent markdown version.
    /// </summary>
    /// <param name="headerHtml">The header HTML.</param>
    /// <returns>The markdown equivalent.</returns>
    string HeaderToMarkDown(string headerHtml);
}
