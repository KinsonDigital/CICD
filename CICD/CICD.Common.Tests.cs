// <copyright file="CICD.Common.Tests.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;

namespace CICDSystem;

/// <summary>
/// Contains all of the unit test related targets.
/// </summary>
public partial class CICD // Common.Tests
{
    /// <summary>
    /// Gets the target that runs all of the unit tests for the solution.
    /// </summary>
    private Target RunAllUnitTests => _ => _
        .DependsOn(RestoreSolution, BuildAllTestProjects)
        .After(BuildAllTestProjects)
        .Executes(() =>
        {
            Log.Information($"🧪 Executing All Tests 🧪");
            RunTests();
        });

    /// <summary>
    /// Runs all of the unit test projects that end with 'Tests' naming convention.
    /// </summary>
    private void RunTests()
    {
        var projects = SolutionService.AllProjects.Where(p => p.Path.Name.EndsWith("Tests.csproj")).ToArray();

        if (projects.Any() is false)
        {
            var logMsg = "The solution does not contain any unit test projects to execute.";
            logMsg += $"{Environment.NewLine}{ConsoleTab}Unit test projects have to end in 'Tests' to be discovered.";
            Log.Warning(logMsg);
            return;
        }

        foreach (var project in projects)
        {
            DotNetTasks.DotNetTest(s => s
                .SetProjectFile<DotNetTestSettings>(project.Path)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        }
    }
}
