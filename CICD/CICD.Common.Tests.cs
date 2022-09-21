using System.Linq;
using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Serilog;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace CICD;

public partial class CICD // Common.Tests
{
    Target RunAllUnitTests => _ => _
        .DependsOn(RestoreSolution, BuildAllTestProjects)
        .After(BuildAllTestProjects)
        .Executes(() =>
        {
            Log.Information($"🧪 Executing All Tests 🧪");
            RunTests();
        });

    private void RunTests()
    {
        var projects = Solution.AllProjects.Where(p => p.Path.Name.EndsWith("Tests.csproj"));

        foreach (var project in projects)
        {
            DotNetTest(s => s
                .SetProjectFile(project.Path)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        }
    }
}
