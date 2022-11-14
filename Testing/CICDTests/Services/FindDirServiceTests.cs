// <copyright file="FindDirServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.IO.Abstractions;
using CICDSystem.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="FindDirService"/> class.
/// </summary>
public class FindDirServiceTests
{
    private const string ProjectName = "MyProject";
    private const string SolutionDir = $@"C:/{ProjectName}";
    private const string DebugDir = $@"C:/{ProjectName}/{ProjectName}/bin/debug";
    private const string BinDir = $@"C:/{ProjectName}/{ProjectName}/bin";
    private const string ProjectDir = $@"C:/{ProjectName}/{ProjectName}";
    private const string RootRepoDir = $@"C:/{ProjectName}";
    private readonly Mock<IDirectory> mockDirectory;
    private readonly Mock<IPath> mockPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="FindDirServiceTests"/> class.
    /// </summary>
    public FindDirServiceTests()
    {
        this.mockDirectory = new Mock<IDirectory>();
        this.mockDirectory.Setup(m => m.GetCurrentDirectory()).Returns(DebugDir);

        // Starting at the execution dir
        this.mockDirectory.Setup(m => m.GetDirectories(DebugDir))
            .Returns(new[]
            {
                $"{DebugDir}/debug-child-1",
                $"{DebugDir}/debug-child-2",
            });

        // If at the bin directory
        this.mockDirectory.Setup(m => m.GetDirectories(BinDir))
            .Returns(new[]
            {
                DebugDir,
                $"{BinDir}/bin-child-1",
            });

        // If at the project directory
        this.mockDirectory.Setup(m => m.GetDirectories(ProjectDir))
            .Returns(new[]
            {
                BinDir,
                $"{ProjectDir}/proj-child-1",
                $"{ProjectDir}/proj-child-2",
            });

        // If at the solution directory
        this.mockDirectory.Setup(m => m.GetDirectories(SolutionDir))
            .Returns(new[]
            {
                ProjectDir,
                $"{SolutionDir}/.github",
                $"{SolutionDir}/solution-child-1",
                $"{SolutionDir}/solution-child-2",
            });

        this.mockPath = new Mock<IPath>();

        // If getting the dir name of the debug dir
        this.mockPath.Setup(m => m.GetDirectoryName(DebugDir))
            .Returns(BinDir);

        // If getting the dir name of the bin dir
        this.mockPath.Setup(m => m.GetDirectoryName(BinDir))
            .Returns(ProjectDir);

        // If getting the dir name of the project dir
        this.mockPath.Setup(m => m.GetDirectoryName(ProjectDir))
            .Returns(SolutionDir);
    }

    #region Method Tests
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void FindDescendentDir_WithNullOrEmptyStartPath_ReturnsEmptyString(string value)
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.FindDescendentDir(value, ".github");

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void FindDescendentDir_WhenDirectoryExists_ReturnsDescendentDirPath()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.FindDescendentDir(DebugDir, ProjectName);

        // Assert
        actual.Should().Be(ProjectDir);
    }

    [Fact]
    public void FindDescendentDir_WhenUsingRootPath_ReturnsCorrectResult()
    {
        // Arrange
        var sut = CreateSystemUnderTest();

        // Act
        var actual = sut.FindDescendentDir(RootRepoDir, "non-existent-dir");

        // Assert
        actual.Should().BeEmpty();
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="FindDirService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private FindDirService CreateSystemUnderTest()
        => new (this.mockDirectory.Object,
            this.mockPath.Object);
}
