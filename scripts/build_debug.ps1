Param(
    [Parameter(Mandatory)]
    $BuildPath,
    [Parameter(Mandatory)]
    $ProjPath
);
Write-Output "[build_debug.ps1]:";

Set-Variable -Name BUILD_INFO_DIRECTORY -Value "_BUILDINFO" -Option ReadOnly;

$buildInfoDir = Join-Path $BuildPath $BUILD_INFO_DIRECTORY;
$csprojName = Split-Path $ProjPath -Leaf

Write-Output "running from $csprojName";

if (-not (Test-Path $buildInfoDir)) {
    Write-Output "creating folder at $buildInfoDir.";
    New-Item -ItemType Directory -Path $buildInfoDir | Out-Null;
}

Write-Output "[build_debug.ps1] succes.";