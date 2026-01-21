Set-Location $PSScriptRoot
$buildScript = ".\..\..\..\scripts\build-resources.ps1"

if (-not (Test-Path $buildScript)) {
    Write-Host "build script not found." -ForegroundColor Red
    exit 1
}

try {
    Write-Host "`nbuilding (RELEASE)..."
    & $buildScript -BuildConfiguration "Release" -DebugBuild $False -ErrorAction Stop
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nbuild successful." -ForegroundColor Green
        exit 0
    }
    else {
        throw "exit code: $LASTEXITCODE"
    }
}
catch {
    Write-Host "`nbuild failed: $_" -ForegroundColor Red
    exit 1
}