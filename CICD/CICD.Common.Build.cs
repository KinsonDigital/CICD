// <copyright file="CICD.Common.Build.cs" company="KinsonDigital">
// Copyright (c) KinsonDigital. All rights reserved.
// </copyright>

namespace CICDSystem;

using System;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;

/// <summary>
/// Contains all of the build related targets.
/// </summary>
public partial class CICD // Common.Build
{
    private Target BuildAllProjects => _ => _
        .DependsOn(RestoreSolution)
        .Executes(() =>
        {
            Log.Information($"✅ Building All Projects ✅");
            BuildProjects(ProjectTypes.All);
        });

    private Target BuildAllTestProjects => _ => _
        .DependsOn(RestoreSolution)
        .Executes(() =>
        {
            Log.Information($"✅ Building All Test Projects ✅");
            BuildProjects(ProjectTypes.Test);
        });

    private void BuildProjects(ProjectTypes projectTypes)
    {
        var projects = Solution?.AllProjects;

        if (projects is null || projects.Count <= 0)
        {
            throw new Exception("No projects could be found.");
        }

        foreach (var project in projects)
        {
            if (project.Path.Name == "CICD.csproj")
            {
                continue;
            }

            var runnable = projectTypes switch
            {
                ProjectTypes.Regular => project.Path.Name.EndsWith("Tests.csproj") is false,
                ProjectTypes.Test => project.Path.Name.EndsWith("Tests.csproj"),
                ProjectTypes.All => true,
                _ => throw new ArgumentOutOfRangeException(nameof(projectTypes), projectTypes, null),
            };

            if (runnable)
            {
                DotNetTasks.DotNetBuild(s => s.SetProjectFile<DotNetBuildSettings>(project.Path)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore());
            }
        }
    }
}
