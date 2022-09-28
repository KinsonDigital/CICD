// <copyright file="WorkflowService.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem.Services;

using System;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;

/// <inheritdoc/>
internal class WorkflowService : IWorkflowService
{
    private readonly IFile file;
    private readonly IDirectory directory;
    private readonly IPath path;
    private string templateDirPath = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/WorkflowTemplates/";

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkflowService"/> class.
    /// </summary>
    /// <param name="directory">Manages directories.</param>
    /// <param name="file">Manages files.</param>
    /// <param name="path">Manages paths.</param>
    public WorkflowService(IDirectory directory, IFile file, IPath path)
    {
        this.directory = directory;
        this.file = file;
        this.path = path;
    }

    /// <inheritdoc/>
    public void GenerateWorkflows(string destinationDir)
    {
        if (string.IsNullOrEmpty(destinationDir))
        {
            throw new ArgumentNullException(nameof(destinationDir), "The parameter must not be null or empty for generating workflows.");
        }

        this.templateDirPath = this.templateDirPath.Replace('\\', '/');
        if (this.directory.Exists(this.templateDirPath) is false)
        {
            throw new DirectoryNotFoundException($"The workflow templates directory '{this.templateDirPath}' was not found.");
        }

        destinationDir = destinationDir.Replace('\\', '/');

        destinationDir = destinationDir.EndsWith('/')
            ? destinationDir
            : $"{destinationDir}/";

        var workflowFilePaths = this.directory.GetFiles(this.templateDirPath, "*.yml", SearchOption.TopDirectoryOnly);

        foreach (var workflowFilePath in workflowFilePaths)
        {
            var workflowFileName = this.path.GetFileName(workflowFilePath);
            var newWorkflowFilePath = $"{destinationDir}{workflowFileName}";

            if (this.file.Exists(newWorkflowFilePath) is false)
            {
                this.file.Copy(workflowFilePath, newWorkflowFilePath);
            }
        }
    }
}
