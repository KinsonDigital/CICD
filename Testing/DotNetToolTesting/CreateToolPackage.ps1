Clear-Host;

$toolProjName = "CICD";
$projectFile = "$toolProjName.csproj";
$toolTestingDirPath = $PSScriptRoot;
$nugetPackageOutputDirPath = "$toolTestingDirPath/packages";

if (Test-Path -Path $nugetPackageOutputDirPath) {
    # Delete all NuGet packages
    Remove-Item -Path "$nugetPackageOutputDirPath" -Recurse -Confirm:$false;
}

[string]$repoRootDirPath = $PSScriptRoot.Replace("\", "/");

# Split the path into sections
[System.Collections.ArrayList]$sections = $repoRootDirPath.Split("/");

# Remove the last 2 directories
$sections.RemoveRange($sections.Count - 2, 2);

# Back up 2 directories to the repo root
$repoRootDirPath = "";
$sections | ForEach-Object -Process { $repoRootDirPath = "$repoRootDirPath/$_" };
$repoRootDirPath = $repoRootDirPath.TrimStart("/");

$toolProjectPath = "$repoRootDirPath/$toolProjName/$projectFile";

# Build the project
dotnet build "$toolProjectPath" -c "Debug";

# Create the NuGet package
dotnet pack $toolProjectPath -c "Debug" -o $nugetPackageOutputDirPath;;
