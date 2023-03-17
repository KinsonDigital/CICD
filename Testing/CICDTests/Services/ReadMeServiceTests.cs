// <copyright file="ReadMeServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.IO.Abstractions;
using CICDSystem.Services;
using CICDSystem.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="ReadMeService"/> class.
/// </summary>
public class ReadMeServiceTests
{
    private const string AppDirPath = "C:/app/.github";
    private const string RepoRootDirPath = "C:/app";
    private const string ReadmeFilePath = $"{RepoRootDirPath}/README.md";
    private readonly Mock<IDirectory> mockDir;
    private readonly Mock<IFile> mockFile;
    private readonly Mock<IPath> mockPath;
    private readonly Mock<IFindDirService> mockFindDirService;
    private readonly Mock<IHtmlToMarkDownService> mockHtmlToMrkDwnService;

    public ReadMeServiceTests()
    {
        this.mockDir = new Mock<IDirectory>();

        this.mockFile = new Mock<IFile>();
        this.mockFile.Setup(m => m.Exists(It.IsAny<string?>())).Returns(true);

        this.mockPath = new Mock<IPath>();
        this.mockPath.Setup(m => m.GetDirectoryName(It.IsAny<string?>()))
            .Returns<string?>(_ => RepoRootDirPath);

        this.mockFindDirService = new Mock<IFindDirService>();
        this.mockFindDirService
            .Setup(m => m.FindDescendentDir(It.IsAny<string>(), ".github"))
            .Returns<string, string>((_, _) => AppDirPath);

        this.mockHtmlToMrkDwnService = new Mock<IHtmlToMarkDownService>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullDirectoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReadMeService(
                null,
                this.mockFile.Object,
                this.mockPath.Object,
                this.mockFindDirService.Object,
                this.mockHtmlToMrkDwnService.Object);
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
            _ = new ReadMeService(
                this.mockDir.Object,
                null,
                this.mockPath.Object,
                this.mockFindDirService.Object,
                this.mockHtmlToMrkDwnService.Object);
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
            _ = new ReadMeService(
                this.mockDir.Object,
                this.mockFile.Object,
                null,
                this.mockFindDirService.Object,
                this.mockHtmlToMrkDwnService.Object);
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
            _ = new ReadMeService(
                this.mockDir.Object,
                this.mockFile.Object,
                this.mockPath.Object,
                null,
                this.mockHtmlToMrkDwnService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'findDirService')");
    }

    [Fact]
    public void Ctor_WithNullHtmlToMarkDownServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ReadMeService(
                this.mockDir.Object,
                this.mockFile.Object,
                this.mockPath.Object,
                this.mockFindDirService.Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'htmlToMarkDownService')");
    }
    #endregion

    #region Method Tests
    [Fact]
    public void RunPreProcessing_WhenReadmeFileDoesNotExist_ThrowsException()
    {
        // Arrange
        this.mockFile.Setup(m => m.Exists(It.IsAny<string?>())).Returns(false);

        var sut = CreateSystemUnderTest();

        // Act
        var act = () => sut.RunPreProcessing();

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage($"The README file '{ReadmeFilePath}' does not exist.");

        this.mockFile.Verify(m => m.Exists(ReadmeFilePath), Times.Once);
    }

    [Theory]
    [InlineData("<div align='center'>")]
    [InlineData("<div align=\"center\">")]
    [InlineData("<div>")]
    [InlineData("<div  >")]
    public void RunPreProcessing_WhenRemovingSoloDivStartTags_RemovesStartTags(string divStartTag)
    {
        // Arrange
        var fileContent =
            $"""
            before-content
            {divStartTag}
            after-content
            """;
        const string expected =
            """
            before-content
            after-content
            """;
        var actual = string.Empty;
        this.mockFile.Setup(m => m.ReadAllText(ReadmeFilePath)).Returns(fileContent);

        this.mockFile.Setup(m => m.WriteAllText(ReadmeFilePath, It.IsAny<string?>()))
            .Callback<string, string?>((_, contents) => actual = contents);

        var sut = CreateSystemUnderTest();

        // Act
        sut.RunPreProcessing();

        // Assert
        actual.Should().Be(expected);
    }

    /// <summary>
    /// Creates a new instance of <see cref="FindDirService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private ReadMeService CreateSystemUnderTest()
        => new (this.mockDir.Object,
            this.mockFile.Object,
            this.mockPath.Object,
            this.mockFindDirService.Object,
            this.mockHtmlToMrkDwnService.Object);
    #endregion
}
