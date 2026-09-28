param(
    [string]$Executable = "$PSScriptRoot/../MauiApp1/bin/UiAutomationWindows/MauiApp1.exe",
    [ValidatePattern('^com\.vocabmate\.uia[0-9a-z]+$')][string]$PackageId = 'com.vocabmate.uia20260928',
    [string]$Artifacts = "$PSScriptRoot/../MauiApp1/obj/UiAutomationEvidence/Windows",
    [switch]$FunctionsOnly,
    [switch]$ProbeOnly,
    [int]$AttachProcessId = 0
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
if (-not ('VocabMateUiWindow' -as [type])) {
    Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class VocabMateUiWindow {
    [DllImport("user32.dll", CharSet=CharSet.Unicode)] public static extern IntPtr SendMessage(IntPtr handle, uint message, IntPtr word, string text);
    [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr handle, uint message, IntPtr word, IntPtr value);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr handle);
    [DllImport("user32.dll")] public static extern bool MoveWindow(IntPtr handle, int left, int top, int width, int height, bool repaint);
}
"@
}
$script:Executable = [IO.Path]::GetFullPath($Executable)
$script:Artifacts = [IO.Path]::GetFullPath($Artifacts)
$script:DataPath = Join-Path $env:LOCALAPPDATA "User Name/$PackageId/Data/vocabmate.json"
$script:Process = $null
$script:Root = $null
$script:Results = [System.Collections.Generic.List[object]]::new()
New-Item -ItemType Directory -Path $script:Artifacts -Force | Out-Null

function Find-Ui([string]$Value, [bool]$ById = $true, [bool]$Button = $false, $Scope = $script:Root) {
    if ($null -eq $Scope) { return $null }
    $property = if ($ById) { [System.Windows.Automation.AutomationElement]::AutomationIdProperty } else { [System.Windows.Automation.AutomationElement]::NameProperty }
    $condition = [System.Windows.Automation.PropertyCondition]::new($property, $Value)
    if ($Button) {
        $condition = [System.Windows.Automation.AndCondition]::new($condition,
            [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Button))
    }
    return $Scope.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
}
function Wait-Ui([string]$Value, [bool]$ById = $true, [int]$TimeoutSeconds = 15) {
    $deadline = [DateTime]::UtcNow.AddSeconds($TimeoutSeconds)
    do {
        $element = Find-Ui $Value $ById
        if ($null -ne $element) { return $element }
        Start-Sleep -Milliseconds 200
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "UI element not found: $Value"
}
function Open-TestApp {
    if (-not (Test-Path -LiteralPath $script:Executable)) { throw "Build the isolated executable first: $script:Executable" }
    $script:Process = Start-Process -FilePath $script:Executable -WorkingDirectory (Split-Path $script:Executable) -WindowStyle Normal -PassThru
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        Start-Sleep -Milliseconds 250
        $script:Process.Refresh()
        if ($script:Process.HasExited) { throw "App exited: $($script:Process.ExitCode)" }
        if ($script:Process.MainWindowHandle -ne 0) { break }
    }
    $script:Root = [System.Windows.Automation.AutomationElement]::FromHandle($script:Process.MainWindowHandle)
    [VocabMateUiWindow]::MoveWindow($script:Process.MainWindowHandle, 40, 40, 1250, 920, $true) | Out-Null
    [VocabMateUiWindow]::SetForegroundWindow($script:Process.MainWindowHandle) | Out-Null
    Set-Content -LiteralPath (Join-Path $script:Artifacts 'process-id.txt') -Value $script:Process.Id
    Start-Sleep -Seconds 1
}
function Close-TestApp {
    if ($null -ne $script:Process -and -not $script:Process.HasExited) {
        $null = $script:Process.CloseMainWindow()
        if (-not $script:Process.WaitForExit(8000)) { throw 'Test app did not close normally; left running for inspection.' }
    }
}
function Click-Ui($Element) {
    if ($null -eq $Element) { throw 'Cannot click a missing UI element' }
    $scroll = $null
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.ScrollItemPattern]::Pattern, [ref]$scroll)) {
        $scroll.ScrollIntoView()
        Start-Sleep -Milliseconds 100
    }
    if (-not $Element.Current.IsEnabled) { throw "UI element disabled: $($Element.Current.Name)" }
    $invoke = $null
    $selection = $null
    if ($Element.TryGetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern, [ref]$invoke)) { $invoke.Invoke() }
    elseif ($Element.TryGetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern, [ref]$selection)) { $selection.Select() }
    else { throw "No UI activation pattern: $($Element.Current.Name)" }
    Start-Sleep -Milliseconds 450
}
function Click-Id([string]$Id) { Click-Ui (Wait-Ui $Id) }
function Click-Name([string]$Name) { Click-Ui (Wait-Ui $Name $false) }
function Set-UiText([string]$Id, [string]$Text) {
    $element = Wait-Ui $Id
    $pattern = $null
    if (-not $element.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$pattern)) {
        $editCondition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Edit)
        $element = $element.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $editCondition)
        if ($null -eq $element -or -not $element.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$pattern)) { throw "No ValuePattern: $Id" }
    }
    $pattern.SetValue($Text)
    Start-Sleep -Milliseconds 150
}
function Get-UiValue([string]$Id) {
    $element = Wait-Ui $Id
    $pattern = $null
    if ($element.TryGetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern, [ref]$pattern)) { return $pattern.Current.Value }
    return $element.Current.Name
}
function Choose-Ui([string]$Id, [string]$Text) {
    $element = Wait-Ui $Id
    $pattern = [System.Windows.Automation.ExpandCollapsePattern]$element.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern)
    $pattern.Expand()
    Start-Sleep -Milliseconds 250
    $option = Find-Ui $Text $false $false $element
    if ($null -eq $option) { $option = Wait-Ui $Text $false }
    Click-Ui $option
}
function Pick-UiWorkbook([string]$Path) {
    Click-Id 'ChooseWorkbookButton'
    $dialog = Wait-Ui 'Open' $false
    $fileName = Find-Ui '1148' $true $false $dialog
    $open = Find-Ui '1' $true $false $dialog
    Assert-Ui ($null -ne $fileName -and $null -ne $open) 'Native Windows file picker controls are present'
    $resolved = (Resolve-Path -LiteralPath $Path).Path
    [VocabMateUiWindow]::SendMessage([IntPtr]$fileName.Current.NativeWindowHandle, 12, [IntPtr]::Zero, $resolved) | Out-Null
    [VocabMateUiWindow]::PostMessage([IntPtr]$open.Current.NativeWindowHandle, 245, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
    Start-Sleep -Milliseconds 900
    Wait-Ui 'ImportFirstLanguage' | Out-Null
}
function Read-TestData {
    if (-not (Test-Path -LiteralPath $script:DataPath)) { throw "Isolated test data not found: $script:DataPath" }
    return Get-Content -LiteralPath $script:DataPath -Raw -Encoding UTF8 | ConvertFrom-Json
}
function Assert-Ui([bool]$Condition, [string]$Message) {
    if (-not $Condition) { throw "Assertion failed: $Message" }
}
function Save-UiEvidence([string]$Name) {
    $elements = $script:Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    $tree = foreach ($element in $elements) {
        try {
            [PSCustomObject]@{ Id = $element.Current.AutomationId; Name = $element.Current.Name; Type = $element.Current.ControlType.ProgrammaticName; Enabled = $element.Current.IsEnabled; Offscreen = $element.Current.IsOffscreen; Bounds = $element.Current.BoundingRectangle.ToString() }
        } catch { }
    }
    $tree | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $script:Artifacts "$Name-tree.json") -Encoding UTF8
    [VocabMateUiWindow]::SetForegroundWindow($script:Process.MainWindowHandle) | Out-Null
    Start-Sleep -Milliseconds 250
    $bounds = $script:Root.Current.BoundingRectangle
    $bitmap = [System.Drawing.Bitmap]::new([int]$bounds.Width, [int]$bounds.Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.CopyFromScreen([int]$bounds.X, [int]$bounds.Y, 0, 0, $bitmap.Size)
        $bitmap.Save((Join-Path $script:Artifacts "$Name.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    } finally { $graphics.Dispose(); $bitmap.Dispose() }
}
function Run-UiCase([string]$Name, [scriptblock]$Action) {
    $started = [DateTime]::UtcNow
    try {
        & $Action
        $script:Results.Add([PSCustomObject]@{ Name = $Name; Status = 'PASS'; Seconds = ([DateTime]::UtcNow - $started).TotalSeconds })
        Write-Output "PASS: $Name"
    } catch {
        $script:Results.Add([PSCustomObject]@{ Name = $Name; Status = 'FAIL'; Error = $_.ToString(); Seconds = ([DateTime]::UtcNow - $started).TotalSeconds })
        Save-UiEvidence 'failure'
        throw
    } finally {
        $script:Results | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $script:Artifacts 'results.json') -Encoding UTF8
    }
}
if ($AttachProcessId -gt 0) {
    $script:Process = Get-Process -Id $AttachProcessId
    Assert-Ui ($script:Process.Path -eq $script:Executable) 'Only attach to the isolated executable'
    $script:Root = [System.Windows.Automation.AutomationElement]::FromHandle($script:Process.MainWindowHandle)
}
if ($FunctionsOnly) { return }
Open-TestApp
if ($ProbeOnly) {
    Save-UiEvidence 'initial'
    Write-Output "Test app PID: $($script:Process.Id)"
    Write-Output "Isolated data: $script:DataPath"
    return
}
