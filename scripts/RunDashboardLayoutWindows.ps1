param(
    [string]$Executable = "$PSScriptRoot/../MauiApp1/bin/UiLayoutWindows/MauiApp1.exe",
    [ValidatePattern('^com\.vocabmate\.uia[0-9a-z]+$')][string]$PackageId = 'com.vocabmate.uialayout20260930',
    [string]$Artifacts = "$PSScriptRoot/../MauiApp1/obj/UiAutomationEvidence/Layout"
)
. "$PSScriptRoot/WindowsUiAutomation.ps1" -Executable $Executable -PackageId $PackageId -Artifacts $Artifacts -FunctionsOnly
$productionData = Join-Path $env:LOCALAPPDATA 'User Name/com.vocabmate.app/Data/vocabmate.json'
$productionHash = if (Test-Path -LiteralPath $productionData) { (Get-FileHash -LiteralPath $productionData).Hash } else { $null }
$className = 'Layout smoke ' + (Get-Date -Format 'yyyyMMdd-HHmmss')
$deckName = 'Everyday expressions ' + (Get-Date -Format 'HHmmss')

function Save-Layout([string]$Name) {
    $elements = $script:Root.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
    $tree = foreach ($element in $elements) {
        [PSCustomObject]@{ Id = $element.Current.AutomationId; Name = $element.Current.Name; Type = $element.Current.ControlType.ProgrammaticName; Offscreen = $element.Current.IsOffscreen; Bounds = $element.Current.BoundingRectangle.ToString() }
    }
    $tree | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $script:Artifacts "$Name-tree.json") -Encoding UTF8
    $bounds = $script:Root.Current.BoundingRectangle
    $bitmap = [System.Drawing.Bitmap]::new([int]$bounds.Width, [int]$bounds.Height)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    try {
        $deviceContext = $graphics.GetHdc()
        try {
            Assert-Ui ([VocabMateUiWindow]::PrintWindow($script:Process.MainWindowHandle, $deviceContext, 2)) 'Capture the isolated window, not the desktop'
        } finally { $graphics.ReleaseHdc($deviceContext) }
        $bitmap.Save((Join-Path $script:Artifacts "$Name.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    } finally { $graphics.Dispose(); $bitmap.Dispose() }
}

function Resize-Layout([int]$Width) {
    [VocabMateUiWindow]::MoveWindow($script:Process.MainWindowHandle, 20, 20, $Width, 900, $true) | Out-Null
    Start-Sleep -Milliseconds 800
}

function Assert-Navigation([bool]$Sidebar) {
    $items = $script:Root.FindAll([System.Windows.Automation.TreeScope]::Descendants,
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, 'Home'))
    $visible = @($items | Where-Object { -not $_.Current.IsOffscreen })
    $sideItems = @($visible | Where-Object { $_.Current.ControlType -eq [System.Windows.Automation.ControlType]::ListItem })
    $tabs = @($visible | Where-Object { $_.Current.ControlType -eq [System.Windows.Automation.ControlType]::TabItem })
    Assert-Ui (($sideItems.Count -gt 0) -eq $Sidebar) 'Sidebar matches the window breakpoint'
    Assert-Ui (($tabs.Count -gt 0) -ne $Sidebar) 'Do not show duplicate navigation'
}

try {
    $script:Process = Start-Process -FilePath $script:Executable -WorkingDirectory (Split-Path $script:Executable) -WindowStyle Hidden -PassThru
    for ($attempt = 0; $attempt -lt 80; $attempt++) {
        Start-Sleep -Milliseconds 250
        $script:Process.Refresh()
        if ($script:Process.HasExited) { throw 'Isolated app exited at startup' }
        if ($script:Process.MainWindowHandle -ne 0) { break }
    }
    $script:Root = [System.Windows.Automation.AutomationElement]::FromHandle($script:Process.MainWindowHandle)
    Resize-Layout 1360
    $settingsName = if (Find-Ui 'Settings' $false) { 'Settings' } else { 'C' + [char]0xe0 + 'i ' + [char]0x111 + [char]0x1eb7 + 't' }
    Click-Name $settingsName
    Assert-Ui ($null -ne (Find-Ui $script:DataPath $false)) 'Verify the isolated data path before any writes'
    Choose-Ui 'LanguagePicker' 'English'
    Choose-Ui 'ThemePicker' 'Light'
    Click-Name 'Home'
    Assert-Navigation $true
    Click-Id 'HomeCreateClass'
    Set-UiText 'NewClassName' $className
    Click-Id 'AddClassButton'
    Click-Name $className
    Assert-Navigation $true
    Set-UiText 'NewDeckName' $deckName
    Click-Id 'AddDeckButton'
    Click-Name $deckName
    Click-Id 'AddCardButton'
    Set-UiText 'VietnameseEntry' 'hello'
    Set-UiText 'EnglishEntry' 'xin chao'
    Click-Id 'SaveCardButton'
    Click-Id 'StartLearningButton'
    if (Find-Ui 'DialogAcceptButton') { Click-Id 'DialogAcceptButton' }
    Wait-Ui 'StudyBackButton' | Out-Null
    Assert-Navigation $true
    Click-Id 'StudyBackButton'
    Click-Name 'Home'
    Wait-Ui 'ResumeLearningButton' | Out-Null
    Wait-Ui 'Recently studied' $false | Out-Null
    Save-Layout '01-home-wide-light'
    Click-Id 'HomeOpenLibrary'
    Assert-Ui ($null -eq (Find-Ui 'NewClassName')) 'Creation is a one-time navigation action'
    Set-UiText 'SearchClasses' 'no-match-layout-smoke'
    Wait-Ui 'No matching classes. Try a different name or clear your search.' $false | Out-Null
    Click-Id 'ClearClassSearch'
    Wait-Ui $className $false | Out-Null
    Save-Layout '02-library-wide-light'
    Click-Name 'Home'
    Resize-Layout 520
    Assert-Navigation $false
    Wait-Ui 'HomeSearch' | Out-Null
    Save-Layout '03-home-narrow-light'
    Resize-Layout 360
    Assert-Navigation $false
    Save-Layout '04-home-small-light'
    Resize-Layout 1360
    Assert-Navigation $true
    Click-Name 'Settings'
    Choose-Ui 'ThemePicker' 'Dark'
    Click-Name 'Home'
    Save-Layout '05-home-wide-dark'
    Click-Id 'ResumeLearningButton'
    Wait-Ui 'StudyBackButton' | Out-Null
    Click-Id 'StudyBackButton'
    $currentHash = if (Test-Path -LiteralPath $productionData) { (Get-FileHash -LiteralPath $productionData).Hash } else { $null }
    Assert-Ui ($currentHash -eq $productionHash) 'Production data remains unchanged'
    'PASS: home/create/class/deck/study/resume, library search, adaptive navigation at 360/520/1360, light/dark, production data unchanged.' |
        Tee-Object -FilePath (Join-Path $script:Artifacts 'summary.txt')
} catch {
    if ($null -ne $script:Root) { Save-Layout 'failure' }
    throw
} finally {
    Close-TestApp
}
