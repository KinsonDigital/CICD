
$basePath = "../DotNetToolTesting";
$nugetCacheFilePath = "C:/Users/$env:UserName/.nuget/packages/kinsondigital.cicd";

$packagePaths = Get-ChildItem -Path "$basePath/**/KinsonDigital.CICD*.nupkg" -Recurse | ForEach-Object{$_.FullName};

# Delete the dotnet tool manifest if it exists
if (Test-Path -Path "$basepath/.config") {
    Remove-Item -Path "$basepath/.config" -Force -Recurse -Confirm:$false;
    Write-Host "✅DotNet tool manifest deleted.";
}

# Delete the cached nuget package tool
if (Test-Path -Path $nugetCacheFilePath) {
    Remove-Item -Path $nugetCacheFilePath -Force -Recurse -Confirm:$false;
    Write-Host "✅Globally cached dotnet tool nuget package deleted.";
}

# Delete all nuget packages
foreach ($path in $packagePaths) {
    Remove-Item -Path $path;
    Write-Host "✅The NuGet package '$path' has been deleted.";
}

$projPath = "../../CICD/CICD.csproj";

# Create the nuget package
dotnet pack $projPath -o $basePath;

# Create the dotnet tool manifest
dotnet new tool-manifest

# Installed the dotnet tool locally
$packagePaths = Get-ChildItem -Path "$basePath/**/KinsonDigital.CICD*.nupkg" -Recurse | ForEach-Object{$_.Directory};

dotnet tool install "KinsonDigital.CICD" --add-source "$packagePaths" --version "1.0.0-preview.2"
