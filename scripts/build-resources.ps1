Param(
    [Parameter(Mandatory)]
    [ValidateSet("Debug", "Release")]
    [string]$BuildConfiguration,
    [Parameter(Mandatory)]
    [bool]$DebugBuild
)

Set-Location $PSScriptRoot

Write-Output "[build.ps1]:"
Write-Output "BuildConfiguration=$BuildConfiguration"
Write-Output "DebugBuild=$DebugBuild"

if ($DebugBuild) {
    Set-Variable -Name BUILD_INFO_DIRECTORY -Value ".buildinfo" -Option ReadOnly
    $buildInfoDir = Join-Path "..\build\Debug\" $BUILD_INFO_DIRECTORY

    Write-Output "maybe creating folder at $buildInfoDir."
    if (-not (Test-Path $buildInfoDir)) {
        Write-Output "creating folder at $buildInfoDir."
        New-Item -ItemType Directory -Path $buildInfoDir | Out-Null
    }
}

Write-Output "running build command..."

$exePath = Resolve-Path ".\..\build\$BuildConfiguration\STOLON.CLI.exe" -ErrorAction Stop
$workingDir = Split-Path $exePath

Push-Location $workingDir
try {
    & $exePath build -v --bypass-devcheck
    # $LASTEXITCODE is automatically set here.
}
finally {
    Pop-Location
}

Write-Output "[build.ps1] exit code: $LASTEXITCODE"
exit $LASTEXITCODE