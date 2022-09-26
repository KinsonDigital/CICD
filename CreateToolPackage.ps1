$basePath = "$PSScriptRoot/CICD";
$projectFile = "CICD.csproj";
$projPath = "$basePath/$projectFile";
$packagePaths = Get-ChildItem -Path "$basePath/**/*.nupkg" -Recurse | ForEach-Object{$_.FullName};


if ($packagePaths -and $packagePaths.Length -ge 1) {
    # Delete all nuget packages
    foreach ($path in $packagePaths) {
        Remove-Item -Path $path;
        Write-Host "✅The NuGet package '$path' has been deleted.";
    }
}

# Build the project
dotnet build $projPath -c "Debug"

# Create the nuget package
dotnet pack $projPath -c "Debug" -o $basePath;
