# runs when building in Debug Mode. 
# sometimes runs multiple times for the different projects.

Param(
    [Parameter(Mandatory)]
    $BuildConfiguration,
    [Parameter(Mandatory)]
    $ProjPath,
    [Parameter(Mandatory)]
    $DebugBuild
);

Write-Output "[build.ps1]:";

Write-Output "running from $(Split-Path $ProjPath -Leaf).";

Write-Output "BuildConfiguration=$BuildConfiguration"
Write-Output "ProjPath=$ProjPath"
Write-Output "DebugBuild=$DebugBuild"

if ($DebugBuild) {
    Set-Variable -Name BUILD_INFO_DIRECTORY -Value ".buildinfo" -Option ReadOnly;

    $buildInfoDir = Join-Path "..\build\Debug\" $BUILD_INFO_DIRECTORY;

    # create the .buildinfo directory if it doesn't exist already.
    Write-Output "maybe creating folder at $buildInfoDir.";
    if (-not (Test-Path $buildInfoDir)) {
        Write-Output "creating folder at $buildInfoDir.";
        New-Item -ItemType Directory -Path $buildInfoDir | Out-Null;
    }
}

# Copy-Item -Path ($ProjPath + "..\STOLON\" + "resources\*") -Destination "..\build\Debug\" -Recurse -Force

Write-Output "running build command.";

# # list and print all files in the Debug/ directory
# $debugFolderPath = "..\build\Debug\"
# Write-Output "Listing all files in Debug folder: $debugFolderPath"

# $files = Get-ChildItem -Path $debugFolderPath -Recurse -File

# if ($files.Count -eq 0) {
#     Write-Output "No files found in the Debug folder."
# }
# else {
#     $files | ForEach-Object { Write-Output $_.FullName }
# }

Start-Process -FilePath "C:\Users\Gebruiker\source\repos\STOLON\build\Debug\STOLON.CLI.exe" -ArgumentList "build -v" -WorkingDirectory (Split-Path "C:\Users\Gebruiker\source\repos\STOLON\build\Debug\STOLON.CLI.exe") -NoNewWindow -Wait

Write-Output "[build.ps1] succes.";