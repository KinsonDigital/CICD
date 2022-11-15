// <copyright file="ReadmeService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CICDSystem.Exceptions;
using CICDSystem.Guards;
using CICDSystem.Services.Interfaces;

// ReSharper disable InconsistentNaming
namespace CICDSystem.Services;

/// <inheritdoc/>
internal sealed class ReadmeService : IReadmeService
{
    private const string MultiCommentStart = "<!--";
    private const string MultiCommentStop = "-->";
    private const string GitHubDirName = ".github";
    private const string ReadMeFileName = "README.md";
    private const string ProcessingCommentStart = $"{MultiCommentStart}PRE-PROCESSING-COMMENT-START{MultiCommentStop}";
    private const string ProcessingCommentStop = $"{MultiCommentStart}PRE-PROCESSING-COMMENT-STOP{MultiCommentStop}";
    private const string ProcessingUncommentStart = $"{MultiCommentStart}PRE-PROCESSING-UNCOMMENT-START{MultiCommentStop}";
    private const string ProcessingUncommentStop = $"{MultiCommentStart}PRE-PROCESSING-UNCOMMENT-STOP{MultiCommentStop}";
    private readonly IFile file;
    private readonly string baseDirPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadmeService"/> class.
    /// </summary>
    /// <param name="directory">Manages directories.</param>
    /// <param name="file">Manages files.</param>
    /// <param name="path">Manages paths.</param>
    /// <param name="findDirService">Searches for directories.</param>
    public ReadmeService(
        IDirectory directory,
        IFile file,
        IPath path,
        IFindDirService findDirService)
    {
        EnsureThat.ParamIsNotNull(directory, nameof(directory));
        EnsureThat.ParamIsNotNull(file, nameof(file));
        EnsureThat.ParamIsNotNull(path, nameof(path));
        EnsureThat.ParamIsNotNull(findDirService, nameof(findDirService));

        this.file = file;

        this.baseDirPath = findDirService.FindDescendentDir(directory.GetCurrentDirectory(), GitHubDirName)
            .Replace('\\', '/');

        this.baseDirPath = path.GetDirectoryName(this.baseDirPath)
            .Replace('\\', '/');
    }

    /// <inheritdoc/>
    /// <exception cref="FileNotFoundException">
    ///     Thrown when the README file is not found.
    /// </exception>
    /// <exception cref="ReadmeProcessingException">
    ///     Thrown when there is an issue processing the file.
    /// </exception>
    public void RunPreProcessing()
    {
        var filePath = $"{this.baseDirPath}/{ReadMeFileName}";

        if (this.file.Exists(filePath) is false)
        {
            throw new FileNotFoundException($"The README file '{filePath}' does not exist.");
        }

        var fileLines = this.file.ReadAllLines(filePath).ToList();

        var totalCommentStarts = fileLines.Count(l => l.Trim() == ProcessingCommentStart);
        var totalCommentStops = fileLines.Count(l => l.Trim() == ProcessingCommentStop);

        var totalUncommentStarts = fileLines.Count(l => l.Trim() == ProcessingUncommentStart);
        var totalUncommentStops = fileLines.Count(l => l.Trim() == ProcessingUncommentStop);

        if (totalCommentStarts != totalCommentStops)
        {
            throw new ReadmeProcessingException("Readme file processing must have equal comment block starts and stops.");
        }

        if (totalUncommentStarts != totalUncommentStops)
        {
            throw new ReadmeProcessingException("Readme file processing must have equal uncomment block starts and stops.");
        }

        var containsCommentBlock = totalCommentStarts > 0 && totalCommentStops > 0;

        if (containsCommentBlock)
        {
            for (var i = 0; i < fileLines.Count; i++)
            {
                var currentLine = fileLines[i].Trim();

                fileLines[i] = fileLines[i].Trim();

                switch (currentLine)
                {
                    case ProcessingCommentStart:
                        fileLines.Insert(i == fileLines.Count - 1 ? fileLines.Count - 1 : i + 1, MultiCommentStart);
                        i++;
                        break;
                    case ProcessingCommentStop:
                        fileLines.Insert(i, MultiCommentStop);
                        i++;
                        break;
                }
            }
        }

        var containsUncommentBlock = totalUncommentStarts > 0 && totalUncommentStops > 0;

        if (containsUncommentBlock)
        {
            for (var i = 0; i < fileLines.Count; i++)
            {
                var currentLine = fileLines[i].Trim();
                var nextLineIndex = i >= fileLines.Count - 1 ? fileLines.Count - 1 : i + 1;
                var prevLineIndex = i <= 0 ? 0 : i - 1;

                fileLines[i] = fileLines[i].Trim();

                switch (currentLine)
                {
                    case ProcessingUncommentStart when fileLines[nextLineIndex].Trim() == MultiCommentStart:
                        fileLines.RemoveAt(nextLineIndex);
                        break;
                    case ProcessingUncommentStop when fileLines[prevLineIndex].Trim() == MultiCommentStop:
                        fileLines.RemoveAt(i - 1);
                        i--;
                        break;
                }
            }
        }

        if (containsCommentBlock || containsUncommentBlock)
        {
            this.file.WriteAllLines(filePath, fileLines.ToArray());
        }
    }
}
