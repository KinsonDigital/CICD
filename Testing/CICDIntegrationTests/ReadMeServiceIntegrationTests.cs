// <copyright file="ReadMeServiceIntegrationTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.IO.Abstractions;
using System.Reflection;
using CICDIntegrationTests.Helpers;
using CICDSystem.Services;
using CICDSystem.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CICDIntegrationTests;

/// <summary>
/// Performs integration tests for the <see cref="ReadMeService"/>.
/// </summary>
public class ReadMeServiceIntegrationTests
{
    private const string ExpectedReadMeFileName = "readme-expected.md";
    private const string ActualReadMeFileName = "readme-actual.md";
    private readonly string baseDirPath =
        $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/TestData";
    private readonly ReadMeService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReadMeServiceIntegrationTests"/> class.
    /// </summary>
    public ReadMeServiceIntegrationTests()
    {
        var fileSystem = new FileSystem();
        var file = fileSystem.File;

        var mockFile = new Mock<IFile>();
        mockFile.Name = nameof(mockFile);

        // Map the mock methods to the real implementations
        mockFile.Setup(m => m.Exists(It.IsAny<string?>()))
            .Returns<string?>(path => file.Exists(path));
        mockFile.Setup(m => m.ReadAllText(It.IsAny<string>()))
            .Returns<string>(path => file.ReadAllText(path));

        // Mock the write all text to control where the test results get saved
        mockFile.Setup(m => m.WriteAllText(It.IsAny<string>(), It.IsAny<string?>()))
            .Callback<string, string?>((_, contents) => TestData.WriteResult("readme-actual.md", contents));

        var mockPath = new Mock<IPath>();
        mockPath.Setup(m => m.GetDirectoryName(It.IsAny<string?>()))
            .Returns<string?>(_ => this.baseDirPath);

        var mockFindDirService = new Mock<IFindDirService>();
        mockFindDirService
            .Setup(m => m.FindDescendentDir(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((_, _) => this.baseDirPath);

        this.service = new ReadMeService(
            fileSystem.Directory,
            mockFile.Object,
            mockPath.Object,
            mockFindDirService.Object,
            new HtmlToMarkDownService());
    }

    #region Method Tests
    [Fact]
    public void RunPreProcessing_WhenProcessingFile_CorrectlyProcessesFile()
    {
        // Arrange
        var expectedFileContent = TestData.LoadTestData(ExpectedReadMeFileName);

        // Act
        this.service.RunPreProcessing();
        var actual = TestData.LoadTestResults(ActualReadMeFileName);

        // Assert
        AssertEachLine(actual, expectedFileContent);
    }
    #endregion

    /// <summary>
    /// Splits the given <paramref name="actual"/> and <paramref name="expected"/> values
    /// into lines and compares each line individually.
    /// </summary>
    /// <param name="actual">The actual value.</param>
    /// <param name="expected">The expected value.</param>
    private static void AssertEachLine(string actual, string expected)
    {
        var actualLines = actual.Split(Environment.NewLine);
        var expectedLines = expected.Split(Environment.NewLine);

        int lenToProcess;

        if (actualLines.Length != expectedLines.Length)
        {
            lenToProcess = actualLines.Length < expectedLines.Length
                ? actualLines.Length
                : expectedLines.Length;
        }
        else
        {
            lenToProcess = actualLines.Length;
        }

        for (var i = 0; i < lenToProcess; i++)
        {
            var actualLine = actualLines[i];
            var expectedLine = expectedLines[i];

            actualLine.Should().Be(expectedLine, $"on line number {i + 1}.");
        }
    }
}
