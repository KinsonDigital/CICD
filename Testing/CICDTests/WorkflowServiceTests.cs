// <copyright file="WorkflowServiceTests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDTests;

using System.IO.Abstractions;
using System.Reflection;
using FluentAssertions;
using Moq;
using CICDSystem.Services;
using Xunit;

/// <summary>
/// Tests the <see cref="WorkflowService"/> class.
/// </summary>
public class WorkflowServiceTests
{
    private readonly string templateDirPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/WorkflowTemplates/";
    private readonly Mock<IDirectory> mockDirectory;
    private readonly Mock<IFile> mockFile;
    private readonly Mock<IPath> mockPath;

    public WorkflowServiceTests()
    {
        this.mockDirectory = new Mock<IDirectory>();
        this.mockFile = new Mock<IFile>();
        this.mockPath = new Mock<IPath>();

        this.templateDirPath = this.templateDirPath.Replace('\\', '/');
    }

    #region Method Tests
    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void GenerateWorkflows_WithNullOrEmptyDestinationDirParam_ThrowsException(string destDir)
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.GenerateWorkflows(destDir);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithMessage("The parameter must not be null or empty for generating workflows. (Parameter 'destinationDir')");
    }

    [Fact]
    public void GenerateWorkflows_WhenTemplateDirectoryDoesNotExist_ThrowsException()
    {
        // Arrange
        var service = CreateService();

        // Act
        var act = () => service.GenerateWorkflows("test-dir");

        // Assert
        act.Should().Throw<DirectoryNotFoundException>()
            .WithMessage($"The workflow templates directory '{this.templateDirPath}' was not found.");
    }

    [Theory]
    [InlineData(@"C:/dest-dir/")]
    [InlineData(@"C:/dest-dir")]
    [InlineData(@"C:\dest-dir\")]
    public void GenerateWorkflows_WhenInvoked_ReturnsCorrectResult(string destinationDir)
    {
        // Arrange
        const string fileName1 = "file-1.yml";
        const string fileName2 = "file-2.yml";
        const string filePath1 = $"C:/src-dir/{fileName1}";
        const string filePath2 = $"C:/src-dir/{fileName2}";
        var srcFiles = new[] { filePath1, filePath2, };
        var destFilePath1 = $"{destinationDir.TrimEnd('/').TrimEnd('\\')}/{fileName1}";
        var destFilePath2 = $"{destinationDir.TrimEnd('/').TrimEnd('\\')}/{fileName2}";
        this.mockDirectory.Setup(m => m.Exists(this.templateDirPath)).Returns(true);
        this.mockDirectory.Setup(m =>
                m.GetFiles(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<SearchOption>()))
            .Returns(srcFiles);

        this.mockPath.Setup(m => m.GetFileName(filePath1)).Returns(fileName1);
        this.mockPath.Setup(m => m.GetFileName(filePath2)).Returns(fileName2);

        this.mockFile.Setup(m => m.Exists(destFilePath1)).Returns(false);
        this.mockFile.Setup(m => m.Exists(destFilePath2)).Returns(false);

        var service = CreateService();

        // Act
        service.GenerateWorkflows(destinationDir);

        // Assert
        this.mockDirectory.Verify(m =>
                m.GetFiles(this.templateDirPath, "*.yml", SearchOption.TopDirectoryOnly), Times.Once);

        this.mockFile.Verify(m => m.Copy(filePath1, destFilePath1.Replace('\\', '/')), Times.Once());
        this.mockFile.Verify(m => m.Copy(filePath2, destFilePath2.Replace('\\', '/')), Times.Once());
    }
    #endregion

    /// <summary>
    /// Creates a new instance of <see cref="WorkflowService"/> for the purpose of testing.
    /// </summary>
    /// <returns>The instance to test.</returns>
    private WorkflowService CreateService()
        => new (this.mockDirectory.Object, this.mockFile.Object, this.mockPath.Object);
}
