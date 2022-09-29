$packageName = "KinsonDigital.CICD";
$packageSourcePath = "./packages";

# Execute the other script to create the package
& "$PSScriptRoot/CreateToolPackage.ps1";

# Delete the cached nuget package tool
$nugetCacheFilePath = "C:/Users/$env:UserName/.nuget/packages/$packageName";
if (Test-Path -Path $nugetCacheFilePath) {
    Remove-Item -Path $nugetCacheFilePath -Force -Recurse -Confirm:$false;
    Write-Host "✅Globally cached dotnet tool nuget package deleted.";
}

# Create the dotnet tool manifest
cd $PSScriptRoot;
dotnet new tool-manifest --force;

# Extract the vesion from the nuget package name
[string[]]$packagePath = Get-ChildItem -Path "$packageSourcePath/*.nupkg" -Recurse | ForEach-Object { $_.Name };

if ($packagePath.Length -gt 1) {
    Write-Host "❌Too many nuget packages returned.  Do not know which one to use.";
    exit 1;
}

$packageVersion = $packagePath[0].Replace(".nupkg", "").Split("$packageName.")[1];

# Install the tool
dotnet tool install $packageName --add-source $packageSourcePath --version $packageVersion;
