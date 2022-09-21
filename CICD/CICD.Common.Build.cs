using System;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;

public partial class CICD // Common.Build
{
    private Target BuildAllProjects => _ => _
        .DependsOn(RestoreSolution)
        .Executes(() =>
        {
            Log.Information($"✅ Building All Projects ✅");
            BuildProjects(ProjectTypes.All);
        });

    private Target BuildAllRegularProjects => _ => _
        .DependsOn(RestoreSolution)
        .Executes(() =>
        {
            Log.Information($"✅ Building All Regular Projects ✅");
            BuildProjects(ProjectTypes.Regular);
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
        var projects = this.Solution.AllProjects;

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
                DotNetTasks.DotNetBuild(s => DotNetBuildSettingsExtensions.SetProjectFile<DotNetBuildSettings>(s, project.Path)
                    .SetConfiguration(Configuration)
                    .EnableNoRestore());
            }
        }
    }
}
