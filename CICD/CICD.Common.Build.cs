using System;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CICD;

public partial class CICD // Common.Build
{
    Target BuildAllProjects => _ => _
        .DependsOn(RestoreSolution)
        .Executes(() =>
        {
            Log.Information($"✅ Building All Projects ✅");
            BuildProjects(ProjectTypes.All);
        });

    Target BuildAllRegularProjects => _ => _
        .DependsOn(RestoreSolution)
        .Executes(() =>
        {
            Log.Information($"✅ Building All Regular Projects ✅");
            BuildProjects(ProjectTypes.Regular);
        });

    Target BuildAllTestProjects => _ => _
        .DependsOn(RestoreSolution)
        .Executes(() =>
        {
            Log.Information($"✅ Building All Test Projects ✅");
            BuildProjects(ProjectTypes.Test);
        });

    private void BuildProjects(ProjectTypes projectTypes)
    {
        var projects = Solution.AllProjects;

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
                DotNetBuild(s => s
                    .SetProjectFile(project.Path)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore());
            }
        }
    }
}
