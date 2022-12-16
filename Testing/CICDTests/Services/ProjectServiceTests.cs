// <copyright file="ProjectServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.IO.Abstractions;
using CICDSystem.Reactables.Core;
using CICDSystem.Services;
using CICDSystem.Services.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="ProjectService"/> class.
/// </summary>
public class ProjectServiceTests
{
    private const string RepoName = "MyProject";
    private readonly Mock<IReactable<(string, string)>> mockRepoInfoReactable;
    private readonly Mock<IFindDirService> mockFindDirService;
    private readonly Mock<IDirectory> mockDirectory;
    private readonly Mock<IPath> mockPath;
    private readonly Mock<IXmlService> mockXmlService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProjectServiceTests"/> class.
    /// </summary>
    public ProjectServiceTests()
    {
        this.mockRepoInfoReactable = new Mock<IReactable<(string, string)>>();
        this.mockFindDirService = new Mock<IFindDirService>();
        this.mockDirectory = new Mock<IDirectory>();
        this.mockPath = new Mock<IPath>();
        this.mockXmlService = new Mock<IXmlService>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullRepoInfoReactableParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ProjectService(
                null,
                this.mockFindDirService.Object,
                this.mockDirectory.Object,
                this.mockPath.Object,
                this.mockXmlService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'repoInfoReactable')");
    }

    [Fact]
    public void Ctor_WithNullFindDirServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ProjectService(
                this.mockRepoInfoReactable.Object,
                null,
                this.mockDirectory.Object,
                this.mockPath.Object,
                this.mockXmlService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'findDirService')");
    }

    [Fact]
    public void Ctor_WithNullDirectoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ProjectService(
                this.mockRepoInfoReactable.Object,
                this.mockFindDirService.Object,
                null,
                this.mockPath.Object,
                this.mockXmlService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'directory')");
    }

    [Fact]
    public void Ctor_WithNullPathParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ProjectService(
                this.mockRepoInfoReactable.Object,
                this.mockFindDirService.Object,
                this.mockDirectory.Object,
                null,
                this.mockXmlService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'path')");
    }

    [Fact]
    public void Ctor_WithNullXmlServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new ProjectService(
                this.mockRepoInfoReactable.Object,
                this.mockFindDirService.Object,
                this.mockDirectory.Object,
                this.mockPath.Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'xmlService')");
    }

    [Fact]
    public void Ctor_WhenRepoInfoReactableEndsNotifications_UnsubscribesFromReactable()
    {
        // Arrange
        var mockUnsubscriber = new Mock<IDisposable>();
        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj => reactor = reactorObj)
            .Returns<IReactor<(string, string)>>(_ => mockUnsubscriber.Object);

        _ = CreateSystemUnderTest();

        // Act
        reactor.OnCompleted();
        reactor.OnCompleted();

        // Assert
        mockUnsubscriber.Verify(m => m.Dispose(), Times.Once);
    }
    #endregion

    #region Method Tests
    [Theory]
    [InlineData("MyProject", '\\')]
    [InlineData("MyProject", '/')]
    [InlineData("myproject", '\\')]
    [InlineData("myproject", '/')]
    public void GetVersion_WhenInvoked_ReturnsVersion(string projectName, char dirSeparator)
    {
        // Arrange
        const string projFileExtension = ".csproj";
        var baseDirPath = $"C:{dirSeparator}test-project";
        var currentDirPath = $"{baseDirPath}{dirSeparator}bin{dirSeparator}Debug{dirSeparator}net7.0";

        const string expectedBaseDirPath = "C:/test-project";
        const string expectedCurrentDirPath = "C:/test-project/bin/Debug/net7.0";
        var expectedProjFilePath = $"C:/test-project/{projectName}.csproj";
        var projectFilePath = $"{baseDirPath}{dirSeparator}{projectName}{projFileExtension}";

        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj =>
            {
                reactorObj.Should().NotBeNull("it is required for the test to work properly.");

                reactor = reactorObj;
            });

        MockProjectFilePath(currentDirPath, baseDirPath, projectFilePath, projectName);

        this.mockXmlService.Setup(m => m.GetTagValue(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("1.2.3-preview.4");

        var sut = CreateSystemUnderTest();
        reactor.OnNext((RepoName, RepoName));

        // Act
        var actual = sut.GetVersion();

        // Assert
        actual.Should().Be("1.2.3-preview.4");

        this.mockDirectory.Verify(m => m.GetCurrentDirectory(), Times.Once);
        this.mockFindDirService.Verify(m =>
            m.FindDescendentDir(expectedCurrentDirPath, ".github"), Times.Once);
        this.mockDirectory.Verify(m =>
            m.GetFiles(expectedBaseDirPath, $"*{projFileExtension}", SearchOption.AllDirectories), Times.Once);
        this.mockPath.Verify(m => m.GetFileNameWithoutExtension(expectedProjFilePath), Times.Once);
        this.mockXmlService.Verify(m => m.GetTagValue(expectedProjFilePath, "Version"), Times.Once);
    }

    [Theory]
    [InlineData("MyProject", '\\')]
    [InlineData("MyProject", '/')]
    [InlineData("myproject", '\\')]
    [InlineData("myproject", '/')]
    public void GetPackageId_WhenInvoked_ReturnsPackageId(string projectName, char dirSeparator)
    {
        // Arrange
        const string projFileExtension = ".csproj";
        var baseDirPath = $"C:{dirSeparator}test-project";
        var currentDirPath = $"{baseDirPath}{dirSeparator}bin{dirSeparator}Debug{dirSeparator}net7.0";

        const string expectedBaseDirPath = "C:/test-project";
        const string expectedCurrentDirPath = "C:/test-project/bin/Debug/net7.0";
        var expectedProjFilePath = $"C:/test-project/{projectName}.csproj";
        var projectFilePath = $"{baseDirPath}{dirSeparator}{projectName}{projFileExtension}";

        IReactor<(string, string)>? reactor = null;
        this.mockRepoInfoReactable.Setup(m => m.Subscribe(It.IsAny<IReactor<(string, string)>>()))
            .Callback<IReactor<(string, string)>>(reactorObj =>
            {
                reactorObj.Should().NotBeNull("it is required for the test to work properly.");

                reactor = reactorObj;
            });

        MockProjectFilePath(currentDirPath, baseDirPath, projectFilePath, projectName);

        this.mockXmlService.Setup(m => m.GetTagValue(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(RepoName);

        var sut = CreateSystemUnderTest();
        reactor.OnNext((RepoName, RepoName));

        // Act
        var actual = sut.GetPackageId();

        // Assert
        actual.Should().Be(RepoName);

        this.mockDirectory.Verify(m => m.GetCurrentDirectory(), Times.Once);
        this.mockFindDirService.Verify(m =>
            m.FindDescendentDir(expectedCurrentDirPath, ".github"), Times.Once);
        this.mockDirectory.Verify(m =>
            m.GetFiles(expectedBaseDirPath, $"*{projFileExtension}", SearchOption.AllDirectories), Times.Once);
        this.mockPath.Verify(m => m.GetFileNameWithoutExtension(expectedProjFilePath), Times.Once);
        this.mockXmlService.Verify(m => m.GetTagValue(expectedProjFilePath, "PackageId"), Times.Once);
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="ProjectService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private ProjectService CreateSystemUnderTest()
        => new (this.mockRepoInfoReactable.Object,
            this.mockFindDirService.Object,
            this.mockDirectory.Object,
            this.mockPath.Object,
            this.mockXmlService.Object);

    /// <summary>
    /// Mocks the project file process.
    /// </summary>
    /// <param name="currentDirPath">The current directory path to mock.</param>
    /// <param name="baseDirPath">The base directory path to mock.</param>
    /// <param name="projectFilePath">The current project file path to mock.</param>
    /// <param name="projectName">The current project name to mock.</param>
    private void MockProjectFilePath(string currentDirPath, string baseDirPath, string projectFilePath, string projectName)
    {
        this.mockDirectory.Setup(m => m.GetCurrentDirectory()).Returns(currentDirPath);
        this.mockFindDirService.Setup(m =>
                m.FindDescendentDir(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(baseDirPath);

        this.mockDirectory.Setup(m =>
                m.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
            .Returns(new[] { projectFilePath });

        this.mockPath.Setup(m => m.GetFileNameWithoutExtension(It.IsAny<string?>()))
            .Returns(projectName);
    }
}
