param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$Project = "src/Inventory.Api/Inventory.Api.csproj",
    [switch]$SelfContained = $true,
    [switch]$SingleFile = $true
)

$ErrorActionPreference = "Stop"

Write-Host "==> Restoring packages" -ForegroundColor Cyan
dotnet restore $Project

$publishArgs = @(
    "publish",
    $Project,
    "-c", $Configuration,
    "-r", $Runtime
)

if ($SelfContained) {
    $publishArgs += "--self-contained"
    $publishArgs += "true"
} else {
    $publishArgs += "--self-contained"
    $publishArgs += "false"
}

if ($SingleFile) {
    $publishArgs += "/p:PublishSingleFile=true"
}

$publishArgs += "/p:PublishTrimmed=false"

Write-Host "==> Publishing with args: $($publishArgs -join ' ')" -ForegroundColor Cyan
dotnet @publishArgs

$outputPath = "src/Inventory.Api/bin/$Configuration/net8.0/$Runtime/publish"
Write-Host "✅ Publish completed: $outputPath" -ForegroundColor Green
