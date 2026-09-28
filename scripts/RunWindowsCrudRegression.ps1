param(
    [string]$Executable = "$PSScriptRoot/../MauiApp1/bin/UiAutomationWindows/MauiApp1.exe",
    [string]$PackageId = 'com.vocabmate.uia20260928',
    [string]$Artifacts = "$PSScriptRoot/../MauiApp1/obj/UiAutomationEvidence/WindowsCrud"
)
. "$PSScriptRoot/WindowsUiAutomation.ps1" -Executable $Executable -PackageId $PackageId -Artifacts $Artifacts -FunctionsOnly
$started = [DateTime]::UtcNow
$testClass = 'UIA-CRUD-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
$renamedClass = $testClass + '-renamed'
$productionData = Join-Path $env:LOCALAPPDATA 'User Name/com.vocabmate.app/Data/vocabmate.json'
$productionHash = if (Test-Path -LiteralPath $productionData) { (Get-FileHash -LiteralPath $productionData).Hash } else { $null }
function Assert-Dialog { Assert-Ui ($null -ne (Find-Ui 'DialogTitle')) 'Validation message appears'; Click-Id 'DialogCancelButton' }
function Discard-Draft {
    Click-Id 'CardEditorActionsButton'
    Click-Name 'Discard draft'
    Click-Id 'DialogAcceptButton'
}
try {
    Open-TestApp
    if (Find-Ui 'Settings' $false) { Click-Name 'Settings' } else { Click-Name 'Cài đặt' }
    Assert-Ui ($null -ne (Find-Ui $script:DataPath $false)) 'Isolated data path verified before writes'
    Choose-Ui 'LanguagePicker' 'English'
    Choose-Ui 'ThemePicker' 'Light'
    Click-Name 'Library'
    Run-UiCase 'WC01 - Empty class validation, duplicate protection, search and rename' {
        Click-Id 'ToggleCreateClassButton'
        Set-UiText 'NewClassName' '   '
        Assert-Ui (-not (Find-Ui 'AddClassButton').Current.IsEnabled) 'Whitespace class cannot be created'
        Set-UiText 'NewClassName' $testClass
        Click-Id 'AddClassButton'
        $script:ClassId = @((Read-TestData).Classes | Where-Object Name -eq $testClass)[0].Id
        if (-not (Find-Ui 'NewClassName')) { Click-Id 'ToggleCreateClassButton' }
        Set-UiText 'NewClassName' $testClass.ToUpperInvariant()
        Click-Id 'AddClassButton'
        Assert-Dialog
        Assert-Ui (@((Read-TestData).Classes | Where-Object Id -eq $script:ClassId).Count -eq 1) 'Duplicate class adds no record'
        Set-UiText 'SearchClasses' 'definitely-no-such-class-uia'
        Wait-Ui 'No matching classes. Try a different name or clear your search.' $false | Out-Null
        Set-UiText 'SearchClasses' $testClass
        Click-Id 'NamedRowActionsButton'
        Click-Id 'DialogAction0'
        Set-UiText 'DialogInput' $renamedClass
        Click-Id 'DialogAcceptButton'
        Assert-Ui (@((Read-TestData).Classes | Where-Object Id -eq $script:ClassId)[0].Name -eq $renamedClass) 'Rename persists on same class ID'
        Click-Name $renamedClass
        Save-UiEvidence '01-class-validation-rename'
    }
    Run-UiCase 'WC02 - Empty deck, duplicate deck, search and rename' {
        Set-UiText 'NewDeckName' ' '
        Assert-Ui (-not (Find-Ui 'AddDeckButton').Current.IsEnabled) 'Whitespace deck cannot be created'
        Set-UiText 'NewDeckName' 'CRUD deck'
        Click-Id 'AddDeckButton'
        $script:DeckId = @((Read-TestData).Decks | Where-Object ClassId -eq $script:ClassId)[0].Id
        Set-UiText 'NewDeckName' 'CRUD DECK'
        Click-Id 'AddDeckButton'
        Assert-Dialog
        Assert-Ui (@((Read-TestData).Decks | Where-Object ClassId -eq $script:ClassId).Count -eq 1) 'Duplicate deck blocked'
        Set-UiText 'SearchDecks' 'no-such-deck'
        Wait-Ui 'No matching decks. Try a different name or clear your search.' $false | Out-Null
        Click-Id 'ClearDeckSearch'
        Click-Id 'NamedRowActionsButton'
        Click-Id 'DialogAction0'
        Set-UiText 'DialogInput' 'Renamed deck'
        Click-Id 'DialogAcceptButton'
        Click-Name 'Renamed deck'
        Assert-Ui (-not (Find-Ui 'StartLearningButton').Current.IsEnabled) 'Empty deck cannot start study'
    }
    Run-UiCase 'WC03 - Card validation, add, duplicate protection and discard draft' {
        Click-Id 'AddCardButton'
        Set-UiText 'EnglishEntry' 'cat'
        Click-Id 'SaveCardButton'
        Assert-Dialog
        Assert-Ui (@((Read-TestData).Cards | Where-Object DeckId -eq $script:DeckId).Count -eq 0) 'Invalid card creates no record'
        Set-UiText 'VietnameseEntry' 'mèo'
        Click-Id 'SaveCardButton'
        $script:CardId = @((Read-TestData).Cards | Where-Object DeckId -eq $script:DeckId)[0].Id
        Click-Id 'AddCardButton'
        Set-UiText 'VietnameseEntry' '  MÈO  '
        Set-UiText 'EnglishEntry' 'CAT'
        Click-Id 'SaveCardButton'
        Assert-Dialog
        Assert-Ui (@((Read-TestData).Cards | Where-Object DeckId -eq $script:DeckId).Count -eq 1) 'Normalized duplicate card rejected'
        Discard-Draft
        Assert-Ui (@((Read-TestData).Drafts | Where-Object DeckId -eq $script:DeckId).Count -eq 0) 'Discard removes new-card draft'
        Click-Id 'AddCardButton'
        Assert-Ui ((Get-UiValue 'EnglishEntry') -eq '') 'Discarded new-card draft does not return'
        Set-UiText 'VietnameseEntry' 'chó'
        Set-UiText 'EnglishEntry' 'dog'
        Click-Id 'SaveCardButton'
        Save-UiEvidence '03-card-validation'
    }
    Run-UiCase 'WC04 - Search, list filter, edit preserves star and draft discard keeps saved card' {
        Set-UiText 'SearchCards' 'cAt'
        Click-Id 'CardStarButton'
        Choose-Ui 'CardListFilterPicker' 'Not mastered yet'
        Wait-Ui 'No matching cards. Try another word or reset your search and filters.' $false | Out-Null
        Click-Id 'ResetCardSearch'
        Set-UiText 'SearchCards' 'mèo'
        Click-Id 'CardActionsButton'
        Click-Id 'DialogAction0'
        Set-UiText 'EnglishEntry' 'kitten'
        Click-Id 'SaveCardButton'
        $card = @((Read-TestData).Cards | Where-Object Id -eq $script:CardId)[0]
        Assert-Ui ($card.English -eq 'kitten' -and $card.IsStarred) 'Editing keeps same card ID and mastery'
        Set-UiText 'SearchCards' 'kitten'
        Click-Id 'CardActionsButton'
        Click-Id 'DialogAction0'
        Set-UiText 'EnglishEntry' 'unsaved kitten'
        Discard-Draft
        Assert-Ui (@((Read-TestData).Cards | Where-Object Id -eq $script:CardId)[0].English -eq 'kitten') 'Discard does not overwrite saved card'
        Save-UiEvidence '04-edited-starred-card'
    }
    Run-UiCase 'WC04B - Use all cards buttons and study-setup cancellation' {
        Choose-Ui 'LearningFilterPicker' 'Not mastered yet'
        Click-Name 'Use all cards'
        Click-Id 'StartLearningButton'
        if (Find-Ui 'DialogAcceptButton') { Click-Id 'DialogAcceptButton' }
        Assert-Ui ((Read-TestData).Session.Questions.Count -eq 2) 'Use all cards includes starred card despite list search'
        $original = (Read-TestData).Session.Id
        Click-Id 'StudySetupButton'
        Choose-Ui 'StudyFilterPicker' 'Starred · mastered'
        Click-Id 'CancelStudySetupButton'
        Assert-Ui ((Read-TestData).Session.Id -eq $original) 'Cancelled setup leaves session unchanged'
        Click-Id 'StudySetupButton'
        Choose-Ui 'StudyFilterPicker' 'Not mastered yet'
        Click-Id 'ApplyStudySetupButton'
        Click-Id 'DialogAcceptButton'
        Assert-Ui ((Read-TestData).Session.Questions.Count -eq 1) 'Applying unstarred setup excludes mastered card'
        Click-Id 'StudySetupButton'
        Click-Name 'Use all cards'
        Click-Id 'ApplyStudySetupButton'
        Click-Id 'DialogAcceptButton'
        Assert-Ui ((Read-TestData).Session.Questions.Count -eq 2) 'In-session Use all cards restores both cards'
        Save-UiEvidence '04b-use-all-cards'
        Click-Id 'StudyBackButton'
    }
    Run-UiCase 'WC05 - Card delete cancellation and confirmation; deck delete cascades draft and session' {
        Click-Id 'CardActionsButton'
        Click-Id 'DialogDestructiveButton'
        Click-Id 'DialogCancelButton'
        Assert-Ui (@((Read-TestData).Cards | Where-Object Id -eq $script:CardId).Count -eq 1) 'Cancelled delete keeps card'
        Click-Id 'CardActionsButton'
        Click-Id 'DialogDestructiveButton'
        Click-Id 'DialogAcceptButton'
        Assert-Ui (@((Read-TestData).Cards | Where-Object Id -eq $script:CardId).Count -eq 0) 'Confirmed delete removes selected card'
        Click-Id 'ResetCardSearch'
        Choose-Ui 'LearningFilterPicker' 'All words'
        Click-Id 'StartLearningButton'
        if (Find-Ui 'DialogAcceptButton') { Click-Id 'DialogAcceptButton' }
        Click-Id 'StudyBackButton'
        Click-Id 'AddCardButton'
        Set-UiText 'EnglishEntry' 'orphan-draft'
        Click-Id 'PageBackButton'
        Assert-Ui (@((Read-TestData).Drafts | Where-Object DeckId -eq $script:DeckId).Count -eq 1) 'Draft exists before cascade test'
        Click-Id 'ToggleDeckDetailsButton'
        Click-Name 'Delete deck'
        Click-Id 'DialogCancelButton'
        Assert-Ui (@((Read-TestData).Decks | Where-Object Id -eq $script:DeckId).Count -eq 1) 'Cancelled deck deletion keeps deck'
        Click-Name 'Delete deck'
        Click-Id 'DialogAcceptButton'
        $snapshot = Read-TestData
        Assert-Ui (@($snapshot.Decks | Where-Object Id -eq $script:DeckId).Count -eq 0) 'Deck removed'
        Assert-Ui (@($snapshot.Cards | Where-Object DeckId -eq $script:DeckId).Count -eq 0) 'Deck cards removed'
        Assert-Ui (@($snapshot.Drafts | Where-Object DeckId -eq $script:DeckId).Count -eq 0) 'Deck drafts removed'
        Assert-Ui ($null -eq $snapshot.Session) 'Deleted deck session cleared'
    }
    Run-UiCase 'WC06 - Class delete cancellation and confirmed cascading removal' {
        Set-UiText 'NewDeckName' 'Cascade child'
        Click-Id 'AddDeckButton'
        Click-Name 'Cascade child'
        Click-Id 'AddCardButton'
        Set-UiText 'VietnameseEntry' 'nước'
        Set-UiText 'EnglishEntry' 'water'
        Click-Id 'SaveCardButton'
        $childId = @((Read-TestData).Decks | Where-Object ClassId -eq $script:ClassId)[0].Id
        Click-Id 'PageBackButton'
        Click-Id 'ClassActionsButton'
        Click-Id 'DialogDestructiveButton'
        Click-Id 'DialogCancelButton'
        Assert-Ui (@((Read-TestData).Classes | Where-Object Id -eq $script:ClassId).Count -eq 1) 'Cancelled class delete keeps children'
        Click-Id 'ClassActionsButton'
        Click-Id 'DialogDestructiveButton'
        Click-Id 'DialogAcceptButton'
        $snapshot = Read-TestData
        Assert-Ui (@($snapshot.Classes | Where-Object Id -eq $script:ClassId).Count -eq 0) 'Test class removed'
        Assert-Ui (@($snapshot.Decks | Where-Object ClassId -eq $script:ClassId).Count -eq 0) 'Child decks removed'
        Assert-Ui (@($snapshot.Cards | Where-Object DeckId -eq $childId).Count -eq 0) 'Child cards removed'
        Save-UiEvidence '06-class-cascade'
    }
} finally {
    $afterHash = if (Test-Path -LiteralPath $productionData) { (Get-FileHash -LiteralPath $productionData).Hash } else { $null }
    $summary = [PSCustomObject]@{
        StartedUtc=$started.ToString('o'); FinishedUtc=[DateTime]::UtcNow.ToString('o'); Platform='Windows'; PackageId=$PackageId;
        TestClass=$testClass; Passed=@($script:Results | Where-Object Status -eq 'PASS').Count;
        Failed=@($script:Results | Where-Object Status -eq 'FAIL').Count; ProductionDataUnchanged=($productionHash -eq $afterHash)
    }
    $summary | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $script:Artifacts 'summary.json') -Encoding UTF8
    $summary | Format-List
    Close-TestApp
}
