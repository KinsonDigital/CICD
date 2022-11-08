// <copyright file="SecretServiceTests.cs" company="KinsonDigital">
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
/// Tests the <see cref="SecretService"/> class.
/// </summary>
public class SecretServiceTests
{
    private const string ProjectName = "MyProject";
    private const string SolutionDir = $@"C:/{ProjectName}";
    private const string DebugDir = $@"C:/{ProjectName}/{ProjectName}/bin/debug";
    private const string BinDir = $@"C:/{ProjectName}/{ProjectName}/bin";
    private const string ProjectDir = $@"C:/{ProjectName}/{ProjectName}";
    private const string RootRepoDir = $@"C:/{ProjectName}";
    private const string SecretFileName = "local-secrets.json";
    private readonly Mock<IDirectory> mockDirectory;
    private readonly Mock<IFile> mockFile;
    private readonly Mock<IPath> mockPath;
    private readonly Mock<IJsonService> mockJsonService;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretServiceTests"/> class.
    /// </summary>
    public SecretServiceTests()
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

        this.mockFile = new Mock<IFile>();
        this.mockFile.Setup(m => m.Exists($"{RootRepoDir}/.github/{SecretFileName}")).Returns(true);

        this.mockJsonService = new Mock<IJsonService>();
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullDirectoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new SecretService(
                null,
                this.mockFile.Object,
                this.mockPath.Object,
                this.mockJsonService.Object);
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
            _ = new SecretService(
                this.mockDirectory.Object,
                null,
                this.mockPath.Object,
                this.mockJsonService.Object);
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
            _ = new SecretService(
                this.mockDirectory.Object,
                this.mockFile.Object,
                null,
                this.mockJsonService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'path')");
    }

    [Fact]
    public void Ctor_WithNullJsonServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new SecretService(
                this.mockDirectory.Object,
                this.mockFile.Object,
                this.mockPath.Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'jsonService')");
    }

    [Fact]
    public void Ctor_WithNullOrEmptyRootPath_ThrowsException()
    {
        // Arrange
        this.mockPath.Setup(m => m.GetDirectoryName(It.IsAny<string>()))
            .Returns(string.Empty);

        // Act
        var act = CreateService;

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("The root repository directory path could not be found.  Could not load local secrets.");
    }

    [Fact]
    public void Ctor_WithNullStartPath_ThrowsException()
    {
        // Arrange & Act
        this.mockDirectory.Setup(m => m.GetCurrentDirectory()).Returns(string.Empty);
        var act = () => _ = CreateService();

        // Assert
        act.Should().Throw<Exception>()
            .WithMessage("The root repository directory path could not be found.  Could not load local secrets.");
    }

    [Fact]
    public void Ctor_WhenSecretFileExists_DoesNotThrowException()
    {
        // Arrange & Act
        var act = CreateService;

        // Assert
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData($@"C:/{ProjectName}/{ProjectName}/bin/debug")]
    [InlineData($@"C:/{ProjectName}/{ProjectName}/bin/debug/")]
    [InlineData($@"C:\{ProjectName}\{ProjectName}\bin\debug")]
    [InlineData($@"C:\{ProjectName}\{ProjectName}\bin\debug/")]
    public void Ctor_WhenSecretsFileDoesNotExist_CreatesNewFile(string debugDir)
    {
        // Arrange
        const string expectedSecretFilePath = $"{RootRepoDir}/.github/{SecretFileName}";

        this.mockDirectory.Setup(m => m.GetCurrentDirectory()).Returns(debugDir);

        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(false);
        this.mockJsonService.Setup(m =>
                m.Serialize(It.IsAny<KeyValuePair<string, string>[]>()))
                .Returns(string.Empty);

        // Act
        _ = CreateService();

        // Assert
        this.mockJsonService.Verify(m =>
            m.Serialize(new KeyValuePair<string, string>[] { new (string.Empty, string.Empty) }), Times.Once);
        this.mockFile.Verify(m => m.WriteAllText(expectedSecretFilePath, string.Empty), Times.Once);
    }
    #endregion

    #region Method Tests
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void LoadSecret_WithNullOrEmptySecretName_ThrowsException(string secret)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.LoadSecret(secret);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The string parameter must not be null or empty. (Parameter 'secretName')");
    }

    [Fact]
    public void LoadSecret_WhenSecretsFileDoesNotExist_ThrowException()
    {
        // Arrange
        const string json = "json";
        const string secretFilePath = $"{RootRepoDir}/.github/{SecretFileName}";
        this.mockJsonService.Setup(m => m.Deserialize<KeyValuePair<string, string>[]>(It.IsAny<string>()))
            .Throws(new ArgumentNullException(paramName: json));
        this.mockFile.Setup(m => m.Exists(secretFilePath)).Returns(false);

        var service = CreateService();

        // Act
        var actual = service.LoadSecret("test-secret");

        // Assert
        actual.Should().BeNullOrEmpty();
    }

    [Fact]
    public void LoadSecret_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        const string jsonData = "fake-json-data";
        const string expected = "test-secret-value";
        const string secretFilePath = $"{RootRepoDir}/.github/{SecretFileName}";
        this.mockFile.Setup(m => m.ReadAllText(secretFilePath)).Returns(jsonData);
        this.mockJsonService.Setup(m => m.Deserialize<KeyValuePair<string, string>[]>(jsonData))
            .Returns(() => new KeyValuePair<string, string>[]
            {
                new ("secret-A", "secret-A-value"),
                new ("test-secret", expected),
            });

        var service = CreateService();

        // Act
        var actual = service.LoadSecret("test-secret");

        // Assert
        actual.Should().Be(expected);
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="SecretService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private SecretService CreateService()
        => new (this.mockDirectory.Object,
            this.mockFile.Object,
            this.mockPath.Object,
            this.mockJsonService.Object);
}
