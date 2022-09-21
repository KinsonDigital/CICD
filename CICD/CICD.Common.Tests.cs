using System.Linq;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;

public partial class CICD // Common.Tests
{
    private Target RunAllUnitTests => _ => _
        .DependsOn(RestoreSolution, BuildAllTestProjects)
        .After(BuildAllTestProjects)
        .Executes(() =>
        {
            Log.Information($"🧪 Executing All Tests 🧪");
            RunTests();
        });

    private void RunTests()
    {
        var projects = this.Solution.AllProjects.Where(p => p.Path.Name.EndsWith("Tests.csproj"));

        foreach (var project in projects)
        {
            DotNetTasks.DotNetTest(s => DotNetTestSettingsExtensions.SetProjectFile<DotNetTestSettings>(s, project.Path)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        }
    }
}
