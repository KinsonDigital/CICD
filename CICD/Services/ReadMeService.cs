// <copyright file="ReadMeService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CICDSystem.Guards;
using CICDSystem.Services.Interfaces;

namespace CICDSystem.Services;

/// <inheritdoc/>
internal class ReadMeService : IReadMeService
{
    private const string GitHubDirName = ".github";
    private const string ReadMeFileName = "README.md";
    private readonly IFile file;
    private readonly IHtmlToMarkDownService htmlToMarkDownService;
    private readonly string baseDirPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadMeService"/> class.
    /// </summary>
    /// <param name="directory">Manages directories.</param>
    /// <param name="file">Manages files.</param>
    /// <param name="path">Manages paths.</param>
    /// <param name="findDirService">Searches for directories.</param>
    /// <param name="htmlToMarkDownService">Transpiles HTML to markdown.</param>
    internal ReadMeService(
        IDirectory directory,
        IFile file,
        IPath path,
        IFindDirService findDirService,
        IHtmlToMarkDownService htmlToMarkDownService)
    {
        EnsureThat.ParamIsNotNull(directory, nameof(directory));
        EnsureThat.ParamIsNotNull(file, nameof(file));
        EnsureThat.ParamIsNotNull(path, nameof(path));
        EnsureThat.ParamIsNotNull(findDirService, nameof(findDirService));
        EnsureThat.ParamIsNotNull(htmlToMarkDownService, nameof(htmlToMarkDownService));

        this.file = file;

        this.baseDirPath = findDirService.FindDescendentDir(directory.GetCurrentDirectory(), GitHubDirName)
            .Replace('\\', '/');

        this.baseDirPath = (path.GetDirectoryName(this.baseDirPath) ?? string.Empty)
            .Replace('\\', '/');

        this.htmlToMarkDownService = htmlToMarkDownService;
    }

    /// <inheritdoc/>
    public void RunPreProcessing()
    {
        var filePath = $"{this.baseDirPath}/{ReadMeFileName}";

        if (this.file.Exists(filePath) is false)
        {
            throw new FileNotFoundException($"The README file '{filePath}' does not exist.");
        }

        var fileContent = this.file.ReadAllText(filePath);

        fileContent = RemoveSoloDivStartTags(fileContent);
        fileContent = RemoveDivStartTags(fileContent);

        fileContent = RemoveSoloDivEndTags(fileContent);
        fileContent = RemoveDivEndTags(fileContent);

        fileContent = RemoveSoloBreakTags(fileContent);
        fileContent = RemoveBreakTags(fileContent);

        // Process all links
        var linkMatches = HtmlRegex.GetAllHtmlLinksWithContent(fileContent);

        foreach (var linkHtml in linkMatches)
        {
            var markdownEquivalent = this.htmlToMarkDownService.LinkToMarkDown(linkHtml);

            fileContent = fileContent.Replace(linkHtml, markdownEquivalent);
        }

        // Process all markdown links or images by removing preceding
        // whitespace as long as they are not bulleted
        var mrkDwnLinkOrImgMatches = MarkDownRegex.GetAllLinksOrImages(fileContent);

        foreach (var linkOrImgMrkDwn in mrkDwnLinkOrImgMatches)
        {
            var trimmedLinkOrImgMrkDwn = linkOrImgMrkDwn.TrimWhitespaceStart();

            fileContent = fileContent.Replace(linkOrImgMrkDwn, trimmedLinkOrImgMrkDwn);
        }

        // Process all HTML headers
        var htmlHeaderMatches = HtmlRegex.GetAllHtmlHeadersWithContent(fileContent);

        foreach (var headerHtml in htmlHeaderMatches)
        {
            var markdownEquivalent = this.htmlToMarkDownService.HeaderToMarkDown(headerHtml);

            fileContent = fileContent.Replace(headerHtml, markdownEquivalent);
        }

        // Process all markdown headers by removing preceding whitespace
        var markDownHeaderMatches = MarkDownRegex.GetAllHeaders(fileContent);

        foreach (var headerMarkDown in markDownHeaderMatches)
        {
            var trimmedHeaderMarkDown = headerMarkDown.TrimWhitespaceStart();

            fileContent = fileContent.Replace(headerMarkDown, trimmedHeaderMarkDown);
        }

        var resultFilePath = $"{this.baseDirPath}/{ReadMeFileName}";
        this.file.WriteAllText(resultFilePath, fileContent);
    }

    private string RemoveSoloDivStartTags(string content)
    {
        var contentLines = content.Split(Environment.NewLine).ToList();

        for (var i = 0; i < contentLines.Count; i++)
        {
            var line = contentLines[i].TrimWhitespaceStart().TrimWhitespaceEnd();

            if (HtmlRegex.IsSoloDivStartTag(line))
            {
                contentLines.RemoveAt(i);
                i--;
            }
        }

        return string.Join(Environment.NewLine, contentLines);
    }

    private string RemoveDivStartTags(string content)
    {
        var tagsToRemove = HtmlRegex.GetAllDivStartTags(content);

        foreach (var tag in tagsToRemove)
        {
            content = content.Replace(tag, string.Empty);
        }

        return content;
    }

    private string RemoveSoloDivEndTags(string content)
    {
        var contentLines = content.Split(Environment.NewLine).ToList();

        for (var i = 0; i < contentLines.Count; i++)
        {
            var line = contentLines[i].TrimWhitespaceStart().TrimWhitespaceEnd();

            if (HtmlRegex.IsSoloDivEndTag(line))
            {
                contentLines.RemoveAt(i);
            }
        }

        return string.Join(Environment.NewLine, contentLines);
    }

    private string RemoveDivEndTags(string content)
    {
        var tagsToRemove = HtmlRegex.GetAllDivEndTags(content);

        foreach (var tag in tagsToRemove)
        {
            content = content.Replace(tag, string.Empty);
        }

        return content;
    }

    private string RemoveSoloBreakTags(string content)
    {
        var contentLines = content.Split(Environment.NewLine).ToList();

        for (var i = 0; i < contentLines.Count; i++)
        {
            var line = contentLines[i].TrimWhitespaceStart().TrimWhitespaceEnd();

            if (HtmlRegex.IsSoloBreakTag(line))
            {
                contentLines.RemoveAt(i);
            }
        }

        return string.Join(Environment.NewLine, contentLines);
    }

    private string RemoveBreakTags(string content)
    {
        var tagsToRemove = HtmlRegex.GetAllBreakTags(content);

        foreach (var tag in tagsToRemove)
        {
            content = content.Replace(tag, string.Empty);
        }

        return content;
    }
}
