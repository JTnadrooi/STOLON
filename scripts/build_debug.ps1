# runs when building in Debug Mode. 
# sometimes runs multiple times for the different projects.

Param(
    [Parameter(Mandatory)]
    $BuildPath,
    [Parameter(Mandatory)]
    $ProjPath
);
Write-Output "[build_debug.ps1]:";

Set-Variable -Name BUILD_INFO_DIRECTORY -Value ".buildinfo" -Option ReadOnly;

$buildInfoDir = Join-Path $BuildPath $BUILD_INFO_DIRECTORY;
$csprojName = Split-Path $ProjPath -Leaf

Write-Output "running from $csprojName";

# create the .buildinfo directory if it doesn't exist already.
if (-not (Test-Path $buildInfoDir)) {
    Write-Output "creating folder at $buildInfoDir.";
    New-Item -ItemType Directory -Path $buildInfoDir | Out-Null;
}

Write-Output "[build_debug.ps1] succes.";