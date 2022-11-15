// <copyright file="ReadmeServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.IO.Abstractions;
using CICDSystem.Exceptions;
using CICDSystem.Services;
using CICDSystem.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="ReadmeService"/> class.
/// </summary>
public class ReadmeServiceTests
{
    private const string MultiCommentStart = "<!--";
    private const string MultiCommentStop = "-->";
    private const string ProcessingCommentStart = "<!--PRE-PROCESSING-COMMENT-START-->";
    private const string ProcessingCommentStop = "<!--PRE-PROCESSING-COMMENT-STOP-->";
    private const string ProcessingUncommentStart = "<!--PRE-PROCESSING-UNCOMMENT-START-->";
    private const string ProcessingUncommentStop = "<!--PRE-PROCESSING-UNCOMMENT-STOP-->";
    private const string GitHubDirName = ".github";
    private const string Drive = "C:";
    private const string ProjectName = "my-project";
    private const string ReadMeFileName = "README.md";
    private const string BaseDirPath = $"{Drive}/{ProjectName}";
    private const string ReadMeFilePath = $"{Drive}/{ReadMeFileName}";
    private readonly Mock<IDirectory> mockDirectory;
    private readonly Mock<IFile> mockFile;
    private readonly Mock<IPath> mockPath;
    private readonly Mock<IFindDirService> mockFindDirService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadmeServiceTests"/> class.
    /// </summary>
    public ReadmeServiceTests()
    {
        this.mockDirectory = new Mock<IDirectory>();
        this.mockDirectory.Setup(m => m.GetCurrentDirectory())
            .Returns(BaseDirPath);

        this.mockFile = new Mock<IFile>();
        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);

        this.mockPath = new Mock<IPath>();
        this.mockPath.Setup(m => m.GetDirectoryName(It.IsAny<string>()))
            .Returns(Drive);

        this.mockFindDirService = new Mock<IFindDirService>();
        this.mockFindDirService.Setup(m => m.FindDescendentDir(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(BaseDirPath);
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullDirectoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReadmeService(
                null,
                this.mockFile.Object,
                this.mockPath.Object,
                this.mockFindDirService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'directory')");
    }

    [Fact]
    public void Ctor_WithNullFileParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReadmeService(
                this.mockDirectory.Object,
                null,
                this.mockPath.Object,
                this.mockFindDirService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'file')");
    }

    [Fact]
    public void Ctor_WithNullPathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReadmeService(
                this.mockDirectory.Object,
                this.mockFile.Object,
                null,
                this.mockFindDirService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'path')");
    }

    [Fact]
    public void Ctor_WithNullFindDirServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReadmeService(
                this.mockDirectory.Object,
                this.mockFile.Object,
                this.mockPath.Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'findDirService')");
    }

    [Theory]
    [InlineData("C:/test-dir", "C:/test-dir")]
    [InlineData(@"C:\test-dir", "C:/test-dir")]
    public void Ctor_WithDifferentDirectorySeparators_ConvertsToCrossPlatformPath(
        string path,
        string expected)
    {
        // Arrange
        this.mockFindDirService.Setup(m => m.FindDescendentDir(It.IsAny<string?>(), It.IsAny<string>()))
            .Returns(path);

        // Act
        _ = CreateSystemUnderTest();

        // Assert
        this.mockFindDirService.Verify(m =>
            m.FindDescendentDir(BaseDirPath, GitHubDirName), Times.Once);
        this.mockPath.Verify(m => m.GetDirectoryName(expected), Times.Once);
    }
    #endregion

    #region Method Tests
    [Fact]
    public void RunPreProcessing_WhenFileDoesNotExist_ThrowsException()
    {
        // Arrange
        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(false);
        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.RunPreProcessing();

        // Assert
        act.Should().ThrowExactly<FileNotFoundException>()
            .WithMessage($"The README file '{ReadMeFilePath}' does not exist.");
    }

    [Theory]
    [InlineData(" ", "")]
    [InlineData("  ", "")]
    [InlineData("", " ")]
    [InlineData("", "  ")]
    [InlineData(" ", " ")]
    [InlineData("  ", "  ")]
    public void RunPreProcessing_WithNonMatchingCommentStartsAndStops_ThrowsException(
        string preText,
        string postText)
    {
        // Arrange
        var fileContent = new[]
        {
            $"{preText}{ProcessingCommentStart}{postText}",
            "content to comment",
        };

        this.mockFile.Setup(m => m.ReadAllLines(It.IsAny<string>()))
            .Returns(fileContent);

        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.RunPreProcessing();

        // Assert
        act.Should().ThrowExactly<ReadmeProcessingException>()
            .WithMessage("Readme file processing must have equal comment block starts and stops.");
    }

    [Theory]
    [InlineData(" ", "")]
    [InlineData("  ", "")]
    [InlineData("", " ")]
    [InlineData("", "  ")]
    [InlineData(" ", " ")]
    [InlineData("  ", "  ")]
    public void RunPreProcessing_WithNonMatchingUncommentStartsAndStops_ThrowsException(
        string preText,
        string postText)
    {
        // Arrange
        var fileContent = new[]
        {
            $"{preText}{ProcessingUncommentStart}{postText}",
            "content to uncomment",
        };

        this.mockFile.Setup(m => m.ReadAllLines(It.IsAny<string>()))
            .Returns(fileContent);

        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.RunPreProcessing();

        // Assert
        act.Should().ThrowExactly<ReadmeProcessingException>()
            .WithMessage("Readme file processing must have equal uncomment block starts and stops.");
    }

    [Theory]
    [InlineData(" ", "")]
    [InlineData("  ", "")]
    [InlineData("", " ")]
    [InlineData("", "  ")]
    [InlineData(" ", " ")]
    [InlineData("  ", "  ")]
    public void RunPreProcessing_WhenFileDoesExistAndContainsCommentBlock_CommentsContent(
        string preText,
        string postText)
    {
        // Arrange
        var expected = new[]
        {
            ProcessingCommentStart,
            MultiCommentStart,
            "content to comment",
            MultiCommentStop,
            ProcessingCommentStop,
        };

        var fileContent = new[]
        {
            $"{preText}{ProcessingCommentStart}{postText}",
            "content to comment",
            $"{preText}{ProcessingCommentStop}{postText}",
        };

        this.mockFile.Setup(m => m.ReadAllLines(It.IsAny<string>()))
            .Returns(fileContent);
        var sut = CreateSystemUnderTest();

        // Act
        sut.RunPreProcessing();

        // Assert
        this.mockFile.Verify(m => m.ReadAllLines(ReadMeFilePath), Times.Once);
        this.mockFile.Verify(m => m.WriteAllLines(ReadMeFilePath, expected), Times.Once);
    }

    [Theory]
    [InlineData("before-content", "after-content")]
    public void RunPreProcessing_WithContentBeforeAndAfterCommentBlock_CommentsContent(
        string beforeBlockContent,
        string afterBlockContent)
    {
        // Arrange
        var expected = new[]
        {
            beforeBlockContent,
            ProcessingCommentStart,
            MultiCommentStart,
            "content to comment",
            MultiCommentStop,
            ProcessingCommentStop,
            afterBlockContent,
        };

        var fileContent = new[]
        {
            beforeBlockContent,
            ProcessingCommentStart,
            "content to comment",
            ProcessingCommentStop,
            afterBlockContent,
        };

        this.mockFile.Setup(m => m.ReadAllLines(It.IsAny<string>()))
            .Returns(fileContent);
        var sut = CreateSystemUnderTest();

        // Act
        sut.RunPreProcessing();

        // Assert
        this.mockFile.Verify(m => m.ReadAllLines(ReadMeFilePath), Times.Once);
        this.mockFile.Verify(m => m.WriteAllLines(ReadMeFilePath, expected), Times.Once);
    }

    [Theory]
    [InlineData(" ", "")]
    [InlineData("  ", "")]
    [InlineData("", " ")]
    [InlineData("", "  ")]
    [InlineData(" ", " ")]
    [InlineData("  ", "  ")]
    public void RunPreProcessing_WhenFileDoesExistAndContainsUncommentBlock_UnCommentsContent(
        string preText,
        string postText)
    {
        // Arrange
        var expected = new[]
        {
            ProcessingUncommentStart,
            "content to uncomment",
            ProcessingUncommentStop,
        };

        var fileContent = new[]
        {
            $"{preText}{ProcessingUncommentStart}{postText}",
            MultiCommentStart,
            "content to uncomment",
            MultiCommentStop,
            $"{preText}{ProcessingUncommentStop}{postText}",
        };

        this.mockFile.Setup(m => m.ReadAllLines(It.IsAny<string>()))
            .Returns(fileContent);
        var sut = CreateSystemUnderTest();

        // Act
        sut.RunPreProcessing();

        // Assert
        this.mockFile.Verify(m => m.ReadAllLines(ReadMeFilePath), Times.Once);
        this.mockFile.Verify(m => m.WriteAllLines(ReadMeFilePath, expected), Times.Once);
    }

    [Theory]
    [InlineData("before-content", "after-content")]
    public void RunPreProcessing_WithContentBeforeAndAfterUnCommentBlock_UnCommentsContent(
        string beforeBlockContent,
        string afterBlockContent)
    {
        // Arrange
        var expected = new[]
        {
            beforeBlockContent,
            ProcessingUncommentStart,
            "content to uncomment",
            ProcessingUncommentStop,
            afterBlockContent,
        };

        var fileContent = new[]
        {
            beforeBlockContent,
            ProcessingUncommentStart,
            MultiCommentStart,
            "content to uncomment",
            MultiCommentStop,
            ProcessingUncommentStop,
            afterBlockContent,
        };

        this.mockFile.Setup(m => m.ReadAllLines(It.IsAny<string>()))
            .Returns(fileContent);
        var sut = CreateSystemUnderTest();

        // Act
        sut.RunPreProcessing();

        // Assert
        this.mockFile.Verify(m => m.ReadAllLines(ReadMeFilePath), Times.Once);
        this.mockFile.Verify(m => m.WriteAllLines(ReadMeFilePath, expected), Times.Once);
    }

    [Fact]
    public void RunPreProcessing_WithNoBlocks_DoesNotWriteToFile()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        sut.RunPreProcessing();

        // Assert
        this.mockFile.Verify(m => m.WriteAllLines(It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="ReadmeService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private ReadmeService CreateSystemUnderTest()
        => new (this.mockDirectory.Object,
            this.mockFile.Object,
            this.mockPath.Object,
            this.mockFindDirService.Object);
}
