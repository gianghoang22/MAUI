param(
    [string]$Executable = "$PSScriptRoot/../MauiApp1/bin/UiAutomationWindows/MauiApp1.exe",
    [string]$PackageId = 'com.vocabmate.uia20260928',
    [string]$Artifacts = "$PSScriptRoot/../MauiApp1/obj/UiAutomationEvidence/HomeSearch"
)
. "$PSScriptRoot/WindowsUiAutomation.ps1" -Executable $Executable -PackageId $PackageId -Artifacts $Artifacts -FunctionsOnly
$prefix = 'UIA-Search-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
$firstClass = "$prefix English"
$secondClass = "$prefix Japanese"
$productionData = Join-Path $env:LOCALAPPDATA 'User Name/com.vocabmate.app/Data/vocabmate.json'
$beforeHash = if (Test-Path -LiteralPath $productionData) { (Get-FileHash -LiteralPath $productionData).Hash } else { $null }

function Press-SearchEnter([string]$Id) {
    $field = Wait-Ui $Id
    $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Edit)
    $edit = $field.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    Assert-Ui ($null -ne $edit) 'Search text editor exists'
    [VocabMateUiWindow]::SetForegroundWindow($script:Process.MainWindowHandle) | Out-Null
    $edit.SetFocus()
    [System.Windows.Forms.SendKeys]::SendWait('{ENTER}')
    Start-Sleep -Milliseconds 350
}

function Get-SearchText([string]$Id) {
    $field = Wait-Ui $Id
    $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Edit)
    $edit = $field.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    Assert-Ui ($null -ne $edit) 'Search text editor exists'
    $value = [System.Windows.Automation.ValuePattern]$edit.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern)
    return $value.Current.Value
}

function Type-Search([string]$Id, [string]$Text) {
    $field = Wait-Ui $Id
    $condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::Edit)
    $edit = $field.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    Assert-Ui ($null -ne $edit) 'Search text editor exists'
    [VocabMateUiWindow]::SetForegroundWindow($script:Process.MainWindowHandle) | Out-Null
    $edit.SetFocus()
    [System.Windows.Forms.SendKeys]::SendWait('^a{BACKSPACE}')
    if ($Text.Length -gt 0) { [System.Windows.Forms.SendKeys]::SendWait($Text) }
    Assert-Ui ((Get-SearchText $Id) -eq $Text) 'Keyboard input reaches the search field'
}

function Wait-SearchGone([string]$Name) {
    $deadline = [DateTime]::UtcNow.AddSeconds(5)
    while ($null -ne (Find-Ui $Name $false) -and [DateTime]::UtcNow -lt $deadline) { Start-Sleep -Milliseconds 50 }
    Assert-Ui ($null -eq (Find-Ui $Name $false)) "Search removes nonmatching result: $Name"
}

try {
    Open-TestApp
    $settingsName = if (Find-Ui 'Settings' $false) { 'Settings' } else { 'C' + [char]0x00e0 + 'i ' + [char]0x0111 + [char]0x1eb7 + 't' }
    Click-Name $settingsName
    Assert-Ui ($null -ne (Find-Ui $script:DataPath $false)) 'Isolated data path verified'
    Choose-Ui 'LanguagePicker' 'English'
    Click-Name 'Library'
    foreach ($name in @($firstClass, $secondClass)) {
        Click-Id 'ToggleCreateClassButton'
        Set-UiText 'NewClassName' $name
        Click-Id 'AddClassButton'
    }
    Click-Name 'Home'

    Run-UiCase 'HS01 - Home filters while typing without navigation' {
        Type-Search 'HomeSearch' $firstClass
        Wait-Ui $firstClass $false | Out-Null
        Wait-SearchGone $secondClass
        Assert-Ui ($null -ne (Find-Ui 'HomeCreateClass')) 'Typing stays on Home'
    }
    Run-UiCase 'HS02 - Repeated Enter stays on Home and preserves results' {
        for ($attempt = 0; $attempt -lt 4; $attempt++) {
            Press-SearchEnter 'HomeSearch'
            Assert-Ui ($null -ne (Find-Ui 'HomeCreateClass')) 'Enter stays on Home'
            Assert-Ui ($null -ne (Find-Ui $firstClass $false)) 'Search result remains'
            Assert-Ui ((Get-SearchText 'HomeSearch') -eq $firstClass) 'Query remains'
        }
        Save-UiEvidence '02-home-enter'
    }
    Run-UiCase 'HS03 - Query replacement and no-match state update immediately' {
        Type-Search 'HomeSearch' $secondClass
        Wait-Ui $secondClass $false | Out-Null
        Wait-SearchGone $firstClass
        Type-Search 'HomeSearch' "$prefix-missing"
        Wait-SearchGone $secondClass
        Assert-Ui ($null -ne (Find-Ui 'HomeCreateClass')) 'No-match state stays on Home'
        Save-UiEvidence '03-no-match'
        Type-Search 'HomeSearch' ''
        Wait-Ui 'HomeViewAll' | Out-Null
    }
    Run-UiCase 'HS04 - Library realtime search and Enter do not return to Home' {
        Click-Id 'HomeOpenLibrary'
        Type-Search 'SearchClasses' $secondClass
        Wait-Ui $secondClass $false | Out-Null
        Wait-SearchGone $firstClass
        for ($attempt = 0; $attempt -lt 3; $attempt++) {
            Press-SearchEnter 'SearchClasses'
            Assert-Ui ($null -ne (Find-Ui 'ToggleCreateClassButton')) 'Enter stays in Library'
            Assert-Ui ($null -eq (Find-Ui 'HomeCreateClass')) 'Enter does not go Home'
        }
        Save-UiEvidence '04-library-enter'
    }
} finally {
    Close-TestApp
    $afterHash = if (Test-Path -LiteralPath $productionData) { (Get-FileHash -LiteralPath $productionData).Hash } else { $null }
    $summary = [pscustomobject]@{
        Passed = @($script:Results | Where-Object Status -eq 'PASS').Count
        Failed = @($script:Results | Where-Object Status -eq 'FAIL').Count
        ProductionDataUnchanged = ($beforeHash -eq $afterHash)
        TestClasses = @($firstClass, $secondClass)
    }
    $summary | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $script:Artifacts 'summary.json') -Encoding UTF8
    $summary | Format-List
}
