// <copyright file="LoadSecretsServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System.IO.Abstractions;
using CICDSystem.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace CICDSystemTests.Services;

/// <summary>
/// Tests the <see cref="LoadSecretsService"/> class.
/// </summary>
public class LoadSecretsServiceTests
{
    private const string ExecutionDir = @"C:/MyProject/bin/debug";
    private const string RootRepoDir = @"C:/MyProject";
    private readonly Mock<IDirectory> mockDirectory;
    private readonly Mock<IFile> mockFile;
    private readonly Mock<IPath> mockPath;
    private readonly Mock<IJsonService> mockJsonService;
    private readonly Mock<ICurrentDirService> mockCurrentDirService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoadSecretsServiceTests"/> class.
    /// </summary>
    public LoadSecretsServiceTests()
    {
        this.mockDirectory = new Mock<IDirectory>();
        this.mockDirectory.Setup(m => m.GetDirectories(RootRepoDir))
            .Returns(new[]
            {
                $"{RootRepoDir}/.git",
                $"{RootRepoDir}/MyProject",
                $"{RootRepoDir}/MyProject/Services",
            });

        this.mockFile = new Mock<IFile>();
        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(true);

        this.mockPath = new Mock<IPath>();
        this.mockPath.Setup(m => m.GetDirectoryName(It.IsAny<string>()))
            .Returns(ExecutionDir);

        this.mockJsonService = new Mock<IJsonService>();

        this.mockCurrentDirService = new Mock<ICurrentDirService>();
        this.mockCurrentDirService.Setup(m => m.GetCurrentDirectory()).Returns(ExecutionDir);
    }

    #region Constructor Tests
    [Fact]
    public void Ctor_WithNullDirectoryParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new LoadSecretsService(
                null,
                this.mockFile.Object,
                this.mockPath.Object,
                this.mockJsonService.Object,
                this.mockCurrentDirService.Object);
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
            _ = new LoadSecretsService(
                this.mockDirectory.Object,
                null,
                this.mockPath.Object,
                this.mockJsonService.Object,
                this.mockCurrentDirService.Object);
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
            _ = new LoadSecretsService(
                this.mockDirectory.Object,
                this.mockFile.Object,
                null,
                this.mockJsonService.Object,
                this.mockCurrentDirService.Object);
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
            _ = new LoadSecretsService(
                this.mockDirectory.Object,
                this.mockFile.Object,
                this.mockPath.Object,
                null,
                this.mockCurrentDirService.Object);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'jsonService')");
    }

    [Fact]
    public void Ctor_WithNullCurrentDirServiceParam_ThrowsException()
    {
        // Arrange & Act
        var act = () =>
        {
            _ = new LoadSecretsService(
                this.mockDirectory.Object,
                this.mockFile.Object,
                this.mockPath.Object,
                this.mockJsonService.Object,
                null);
        };

        // Assert
        act.Should()
            .Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null. (Parameter 'currentDirService')");
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
    public void Ctor_WhenSecretFileExists_DoesNotThrowException()
    {
        // Arrange & Act
        var act = CreateService;

        // Assert
        act.Should().NotThrow();
        this.mockFile.Verify(m => m.Exists($"{RootRepoDir}/.github/local-secrets.json"));
    }

    [Fact]
    public void Ctor_WhenSecretsFileDoesNotExist_CreatesNewFile()
    {
        // Arrange
        var expectedSerializeType = new KeyValuePair<string, string>[] { new (string.Empty, string.Empty) };
        var expectedSecretFilePath = $"{RootRepoDir}/.github/local-secrets.json";
        this.mockFile.Setup(m => m.Exists(It.IsAny<string>())).Returns(false);
        this.mockJsonService.Setup(m => m.Serialize(expectedSerializeType)).Returns(string.Empty);

        // Act
        _ = CreateService();

        // Assert
        this.mockJsonService.Verify(m => m.Serialize(expectedSerializeType));
        this.mockFile.Verify(m => m.WriteAllText(expectedSecretFilePath, string.Empty));
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
    public void LoadSecret_WhenInvoked_ReturnsCorrectResult()
    {
        // Arrange
        const string jsonData = "fake-json-data";
        const string expected = "test-secret-value";
        const string secretFilePath = $"{RootRepoDir}/.github/local-secrets.json";
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
    /// Creates a new instance of <see cref="LoadSecretsService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private LoadSecretsService CreateService()
        => new (this.mockDirectory.Object,
            this.mockFile.Object,
            this.mockPath.Object,
            this.mockJsonService.Object,
            this.mockCurrentDirService.Object);
}
