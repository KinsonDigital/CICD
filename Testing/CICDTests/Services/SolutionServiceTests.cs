// <copyright file="SolutionServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Reflection;
using CICDSystem;
using CICDSystem.Services;
using FluentAssertions;
using Moq;
using Nuke.Common.ProjectModel;
using Xunit;
using Project = Nuke.Common.ProjectModel.Project;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="SolutionService"/> class.
/// </summary>
[SuppressMessage("StyleCop.CSharp.OrderingRules",
    "SA1202:Elements should be ordered by access",
    Justification = "Not required for unit tests.")]
public class SolutionServiceTests
{
    private readonly Mock<ISolutionWrapper> mockSolutionWrapper;
    private readonly Mock<IFile> mockFile;

    /// <summary>
    /// Initializes a new instance of the <see cref="SolutionServiceTests"/> class.
    /// </summary>
    public SolutionServiceTests()
    {
        this.mockSolutionWrapper = new Mock<ISolutionWrapper>();
        this.mockFile = new Mock<IFile>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullSolutionWrapperParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new SolutionService(
                null,
                this.mockFile.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'solutionWrapper')");
    }

    [Fact]
    public void Ctor_WithNullFileParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new SolutionService(
                this.mockSolutionWrapper.Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'file')");
    }
    #endregion

    #region Prop Tests
    [Fact]
    public void AllProjects_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        var project = CreateProjectObj("TestProject");
        var expected = new ReadOnlyCollection<Project>(new[] { project });

        this.mockSolutionWrapper.SetupGet(p => p.AllProjects)
            .Returns(new ReadOnlyCollection<Project>(new[] { project }));

        var sut = CreateService();

        // Act
        var actual = sut.AllProjects;

        // Assert
        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void Directory_WhenGettingValue_ReturnsCorrectResult()
    {
        // Arrange
        this.mockSolutionWrapper.SetupGet(p => p.Directory).Returns("test-dir-path");

        var sut = CreateService();

        // Act
        var act = sut.Directory;

        // Assert
        act.Should().Be("test-dir-path");
    }
    #endregion

    #region Method Tests
    [Fact]
    public void GetProject_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        var expected = CreateProjectObj("Test-Project");

        this.mockSolutionWrapper.Setup(m => m.GetProject(It.IsAny<string>()))
            .Returns(expected);

        var sut = CreateService();

        // Act
        var actual = sut.GetProject("Test-Project");

        // Assert
        this.mockSolutionWrapper.Verify(m => m.GetProject("Test-Project"), Times.Once);
        actual.Should().BeEquivalentTo(expected);
        actual.Name.Should().Be("Test-Project");
    }

    [Fact]
    public void GetProjects_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        var projectA = CreateProjectObj("ProjectA");
        var projectB = CreateProjectObj("ProjectB");

        var projects = new[] { projectA, projectB };

        this.mockSolutionWrapper.Setup(m => m.GetProjects(It.IsAny<string>()))
            .Returns(projects);

        var sut = CreateService();

        // Act
        var actual = sut.GetProjects("*.*").ToArray();

        // Assert
        this.mockSolutionWrapper.Verify(m => m.GetProjects("*.*"), Times.Once);
        actual.Should().NotBeNull();
        actual.Should().HaveCount(2);
        actual.Should().BeEquivalentTo(projects);
    }

    [Fact]
    public void BuildReleaseNotesFilePath_WithInvalidReleaseType_ThrowsException()
    {
        // Arrange
        var sut = CreateService();

        // Act
        var act = () => sut.BuildReleaseNotesFilePath((ReleaseType)1234, It.IsAny<string>());

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("The enumeration is out of range. (Parameter 'releaseType')Actual value was 1234.");
    }

    [Theory]
    [InlineData(ReleaseType.Preview, "", "1.2.3-preview.4", "PreviewReleases")]
    [InlineData(ReleaseType.Preview, "v", "1.2.3-preview.4", "PreviewReleases")]
    [InlineData(ReleaseType.Production, "", "1.2.3", "ProductionReleases")]
    [InlineData(ReleaseType.HotFix, "", "1.2.3", "ProductionReleases")]
    internal void BuildReleaseNotesFilePath_WhenInvoked_ReturnsCorrectResult(
        ReleaseType releaseType,
        string versionPrefix,
        string version,
        string releaseTypeDirName)
    {
        // Arrange
        var versionWithPrefix = $"{versionPrefix}{version}";
        var expected = $"C:/solution-dir/Documentation/ReleaseNotes/{releaseTypeDirName}/Release-Notes-v{version}.md";
        this.mockSolutionWrapper.SetupGet(p => p.Directory).Returns("C:/solution-dir");

        var sut = CreateService();

        // Act
        var actual = sut.BuildReleaseNotesFilePath(releaseType, versionWithPrefix);

        // Assert
        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("", "1.2.3")]
    [InlineData("v", "1.2.3")]
    public void GetReleaseNotes_WhenFileDoesNotExist_ThrowsException(string versionPrefix, string version)
    {
        // Arrange
        var versionWithPrefix = $"{versionPrefix}{version}";
        var expected = $"The {ReleaseType.Production.ToString().ToLower()} release notes for version 'v{version}' could not be found.";

        this.mockSolutionWrapper.SetupGet(p => p.Directory).Returns("C:/solution-dir");
        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(false);

        var sut = CreateService();

        // Act
        var act = () => sut.GetReleaseNotes(ReleaseType.Production, versionWithPrefix);

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage(expected);
    }

    [Fact]
    public void GetReleaseNotes_WhenFileExists_ReturnsCorrectResult()
    {
        // Arrange
        const string fullFilePath = $"C:/solution-dir/Documentation/ReleaseNotes/ProductionReleases/Release-Notes-v1.2.3.md";

        this.mockSolutionWrapper.SetupGet(p => p.Directory).Returns("C:/solution-dir");
        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);
        this.mockFile.Setup(m => m.ReadAllText(It.IsAny<string>())).Returns("test-release-notes");

        var sut = CreateService();

        // Act
        var actual = sut.GetReleaseNotes(ReleaseType.Production, "1.2.3");

        // Assert
        this.mockFile.Verify(m => m.Exists(fullFilePath), Times.Once);
        this.mockFile.Verify(m => m.ReadAllText(fullFilePath), Times.Once);
        actual.Should().Be("test-release-notes");
    }

    [Theory]
    [InlineData("", "1.2.3")]
    [InlineData("v", "1.2.3")]
    public void GetReleaseNotesAsLines_WhenFileDoesNotExist_ThrowsException(string versionPrefix, string version)
    {
        // Arrange
        var versionWithPrefix = $"{versionPrefix}{version}";
        var expected = $"The {ReleaseType.Production.ToString().ToLower()} release notes for version 'v{version}' could not be found.";

        this.mockSolutionWrapper.SetupGet(p => p.Directory).Returns("C:/solution-dir");
        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(false);

        var sut = CreateService();

        // Act
        var act = () => sut.GetReleaseNotesAsLines(ReleaseType.Production, versionWithPrefix);

        // Assert
        act.Should().Throw<FileNotFoundException>()
            .WithMessage(expected);
    }

    [Fact]
    public void GetReleaseNotesAsLines_WhenFileExists_ReturnsCorrectResult()
    {
        // Arrange
        var expected = new[] { "line 1", "line 2" };
        const string fullFilePath = $"C:/solution-dir/Documentation/ReleaseNotes/ProductionReleases/Release-Notes-v1.2.3.md";

        this.mockSolutionWrapper.SetupGet(p => p.Directory).Returns("C:/solution-dir");
        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);
        this.mockFile.Setup(m => m.ReadAllLines(It.IsAny<string>())).Returns(new[] { "line 1", "line 2" });

        var sut = CreateService();

        // Act
        var actual = sut.GetReleaseNotesAsLines(ReleaseType.Production, "1.2.3");

        // Assert
        this.mockFile.Verify(m => m.Exists(fullFilePath), Times.Once);
        this.mockFile.Verify(m => m.ReadAllLines(fullFilePath), Times.Once);
        actual.Should().BeEquivalentTo(expected);
    }
    #endregion

    /// <summary>
    /// Creates a new <see cref="Project"/> object instance using reflection for the purpose of testing.
    /// </summary>
    /// <param name="name">The name to give to the project.</param>
    /// <returns>The instance to use in the test.</returns>
    private static Project CreateProjectObj(string name)
    {
        /* Constructor Parameters
            Solution solution,
            Guid projectId,
            string name,
            Func<string> pathProvider,
            Guid typeId,
            IDictionary<string, string> configurations
         */
        var ctor = typeof(Project)
            .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public).First();

        var instance = ctor.Invoke(new object[]
        {
            new Solution(),
            Guid.NewGuid(),
            name,
            () => $"C:/solution-dir/TestProject.csproj",
            Guid.NewGuid(),
            new Dictionary<string, string> { { "config-key", "config-value" } },
        }) as Project;

        if (instance is null)
        {
            Assert.True(false, $"The '{nameof(Project)}' object creation failed using reflection.");
        }

        return instance;
    }

    /// <summary>
    /// Creates a new instance of <see cref="SolutionService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private SolutionService CreateService()
        => new (this.mockSolutionWrapper.Object, this.mockFile.Object);
}
