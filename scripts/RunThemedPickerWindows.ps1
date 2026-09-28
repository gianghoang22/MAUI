param([string]$Artifacts = "$PSScriptRoot/../MauiApp1/obj/UiAutomationEvidence/ThemedPickerWindowsSmoke")
. "$PSScriptRoot/WindowsUiAutomation.ps1" -Artifacts $Artifacts -FunctionsOnly
$production = Join-Path $env:LOCALAPPDATA 'User Name/com.vocabmate.app/Data/vocabmate.json'
$before = if (Test-Path -LiteralPath $production) { (Get-FileHash -LiteralPath $production).Hash } else { $null }
try {
    Open-TestApp
    if (Find-Ui 'Settings' $false) { Click-Name 'Settings' } else { Click-Name 'Cài đặt' }
    Assert-Ui ($null -ne (Find-Ui $script:DataPath $false)) 'Only isolated test app is modified'
    Choose-Ui 'LanguagePicker' 'English'
    Choose-Ui 'ThemePicker' 'Light'
    Run-UiCase 'PW01 - Themed selection indicator, cancel and same-value selection' {
        Click-Id 'ThemePicker'
        Assert-Ui ((Wait-Ui 'PickerOption1').Current.HelpText -eq 'Currently selected') 'Light is identified as selected for accessibility'
        Assert-Ui ((Wait-Ui 'PickerOption2').Current.HelpText -ne 'Currently selected') 'Dark is not marked selected'
        Save-UiEvidence '01-light-options'
        Click-Id 'DialogCancelButton'
        Assert-Ui ((Get-UiValue 'ThemePicker') -match 'Light') 'Cancel preserves Light'
        Choose-Ui 'ThemePicker' 'Light'
        Assert-Ui ($null -eq (Find-Ui 'PickerOption0')) 'Selecting current value closes the popup'
    }
    Run-UiCase 'PW02 - Dark popup and current selection use app theme' {
        Choose-Ui 'ThemePicker' 'Dark'
        Assert-Ui ((Get-UiValue 'ThemePicker') -match 'Dark') 'Dark binding updates immediately'
        Click-Id 'ThemePicker'
        Assert-Ui ((Wait-Ui 'PickerOption2').Current.HelpText -eq 'Currently selected') 'Dark is marked selected'
        Save-UiEvidence '02-dark-options'
        Click-Id 'DialogCancelButton'
    }
    Run-UiCase 'PW03 - Vietnamese options, compact popup and reopen persistence' {
        Choose-Ui 'LanguagePicker' 'Tiếng Việt'
        Choose-Ui 'ThemePicker' 'Sáng'
        [VocabMateUiWindow]::MoveWindow($script:Process.MainWindowHandle, 40, 40, 460, 880, $true) | Out-Null
        Click-Id 'LanguagePicker'
        Assert-Ui ((Wait-Ui 'PickerOption0').Current.HelpText -eq 'Đang được chọn') 'Selected status is localized'
        Save-UiEvidence '03-compact-vietnamese'
        Click-Id 'DialogCancelButton'
        Close-TestApp
        Open-TestApp
        Click-Name 'Cài đặt'
        Assert-Ui ((Get-UiValue 'ThemePicker') -match 'Sáng') 'Light persists after restart'
        Assert-Ui ((Get-UiValue 'LanguagePicker') -match 'Tiếng Việt') 'Language persists after restart'
        Choose-Ui 'LanguagePicker' 'English'
    }
} finally {
    Close-TestApp
    $after = if (Test-Path -LiteralPath $production) { (Get-FileHash -LiteralPath $production).Hash } else { $null }
    Assert-Ui ($before -eq $after) 'Production data unchanged'
}
