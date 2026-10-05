param(
    [ValidateSet('Debug', 'Release')][string]$Configuration = 'Debug',
    [string]$AndroidSdkPath = $env:ANDROID_HOME,
    [string]$JavaSdkPath = $env:JAVA_HOME
)

$ErrorActionPreference = 'Stop'
if (-not $AndroidSdkPath) {
    $AndroidSdkPath = Join-Path $env:LOCALAPPDATA 'Android\Sdk'
}
if (-not $JavaSdkPath) {
    $JavaSdkPath = Join-Path $env:LOCALAPPDATA 'Android\jdk\jdk-21'
}
if (-not (Test-Path -LiteralPath $AndroidSdkPath)) {
    throw 'Android SDK not found. Pass -AndroidSdkPath or use Android SDK Manager.'
}
if ((Test-Path -LiteralPath $JavaSdkPath) -and
    -not (Test-Path -LiteralPath (Join-Path $JavaSdkPath 'bin\java.exe'))) {
    $nestedJdk = Get-ChildItem -LiteralPath $JavaSdkPath -Directory |
        Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'bin\java.exe') } |
        Sort-Object Name -Descending | Select-Object -First 1
    if ($nestedJdk) { $JavaSdkPath = $nestedJdk.FullName }
}
if (-not (Test-Path -LiteralPath (Join-Path $JavaSdkPath 'bin\java.exe'))) {
    throw 'JDK not found. Pass -JavaSdkPath pointing to JDK 21.'
}

& dotnet build (Join-Path $PSScriptRoot 'MauiBasics.csproj') -c $Configuration `
    "-p:AndroidSdkDirectory=$AndroidSdkPath" "-p:JavaSdkDirectory=$JavaSdkPath" -v:minimal
if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE)." }

$apkPath = Join-Path $PSScriptRoot "bin\$Configuration\net10.0-android\com.learning.viecnho-Signed.apk"
if (-not (Test-Path -LiteralPath $apkPath)) { throw "Signed APK missing: $apkPath" }
$outputDirectory = Join-Path $PSScriptRoot 'artifacts'
New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
$outputPath = Join-Path $outputDirectory "ViecNho-$($Configuration.ToLowerInvariant()).apk"
Copy-Item -LiteralPath $apkPath -Destination $outputPath -Force
Write-Output "APK: $outputPath"
