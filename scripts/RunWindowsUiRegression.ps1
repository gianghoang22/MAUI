param(
    [string]$Executable = "$PSScriptRoot/../MauiApp1/bin/UiAutomationWindows/MauiApp1.exe",
    [string]$PackageId = 'com.vocabmate.uia20260928',
    [string]$Artifacts = "$PSScriptRoot/../MauiApp1/obj/UiAutomationEvidence/Windows",
    [switch]$LeaveOpen
)
. "$PSScriptRoot/WindowsUiAutomation.ps1" -Executable $Executable -PackageId $PackageId -Artifacts $Artifacts -FunctionsOnly
$started = [DateTime]::UtcNow
$testClass = 'UIA-' + (Get-Date -Format 'yyyyMMdd-HHmmss')
$manualDeck = 'Manual Japanese'
$studyDeck = 'Japanese 13'
$fixtureDirectory = [IO.Path]::GetFullPath("$PSScriptRoot/../MauiApp1/obj/UiAutomationEvidence/fixtures")
& python "$PSScriptRoot/create_ui_fixtures.py" $fixtureDirectory
if ($LASTEXITCODE -ne 0) { throw 'Could not create Excel fixtures' }
$productionData = Join-Path $env:LOCALAPPDATA 'User Name/com.vocabmate.app/Data/vocabmate.json'
$productionHash = if (Test-Path -LiteralPath $productionData) { (Get-FileHash -LiteralPath $productionData).Hash } else { $null }
$englishToJapanese = [ordered]@{ cat='猫'; dog='犬'; water='水'; book='本'; school='学校'; sun='太陽'; moon='月'; flower='花'; mountain='山'; river='川'; friend='友達'; car='車'; train='電車' }
$script:StudyDeckId = $null

function Start-Study([string]$Mode, [string]$Direction = 'English → Japanese', [string]$Filter = 'All words') {
    Choose-Ui 'LearningModePicker' $Mode
    Choose-Ui 'DirectionPicker' $Direction
    Choose-Ui 'LearningFilterPicker' $Filter
    Click-Id 'StartLearningButton'
    if (Find-Ui 'DialogAcceptButton') { Click-Id 'DialogAcceptButton' }
    Wait-Ui 'StudySetupButton' | Out-Null
}
function Complete-MatchBoard {
    $current = (Read-TestData).Session
    foreach ($question in $current.Questions) {
        if ($current.MatchedIds -contains $question.CardId) { continue }
        $expected = $englishToJapanese[$question.Prompt]
        Assert-Ui ($expected -eq $question.Answer) 'Match question agrees with independent Excel fixture'
        Click-Name $expected
        Click-Name $question.Prompt
    }
    Assert-Ui ($null -ne (Read-TestData).Session.FinishedAt) 'Match board completes through UI'
}
function Resume-Home {
    Close-TestApp
    Open-TestApp
    Click-Id 'ResumeLearningButton'
    Wait-Ui 'StudySetupButton' | Out-Null
}
function Get-ExpectedAnswer([string]$Prompt, [bool]$Reverse) {
    if (-not $Reverse) { return $englishToJapanese[$Prompt] }
    return @($englishToJapanese.Keys | Where-Object { $englishToJapanese[$_] -eq $Prompt })[0]
}

try {
    Open-TestApp
    Run-UiCase 'WU01 - Isolated app, English UI and light theme' {
        if (Find-Ui 'Settings' $false) { Click-Name 'Settings' } else { Click-Name 'Cài đặt' }
        Assert-Ui ($null -ne (Find-Ui $script:DataPath $false)) 'UI reports the isolated data path before any test writes'
        Choose-Ui 'LanguagePicker' 'English'
        Choose-Ui 'ThemePicker' 'Light'
        Save-UiEvidence '01-settings-isolation'
        Click-Name 'Library'
    }
    Run-UiCase 'WU02 - Create class, set Japanese-English metadata and add card through UI' {
        Click-Id 'ToggleCreateClassButton'
        Set-UiText 'NewClassName' $testClass
        Click-Id 'AddClassButton'
        Click-Name $testClass
        Set-UiText 'NewDeckName' $manualDeck
        Click-Id 'AddDeckButton'
        Click-Name $manualDeck
        Click-Id 'ToggleDeckDetailsButton'
        Set-UiText 'FirstLanguageEntry' 'Japanese'
        Set-UiText 'SecondLanguageEntry' 'English'
        Click-Id 'SaveDeckButton'
        Click-Id 'AddCardButton'
        Assert-Ui ($null -ne (Find-Ui 'Japanese' $false)) 'Editor labels the first side Japanese'
        Assert-Ui ($null -ne (Find-Ui 'English' $false)) 'Editor labels the second side English'
        Set-UiText 'VietnameseEntry' '猫'
        Set-UiText 'EnglishEntry' 'cat'
        Click-Id 'SaveCardButton'
        $deck = @((Read-TestData).Decks | Where-Object Name -eq $manualDeck)[-1]
        Assert-Ui ($deck.FirstLanguage -eq 'ja' -and $deck.SecondLanguage -eq 'en') 'Manual language metadata persists'
        Assert-Ui (@((Read-TestData).Cards | Where-Object DeckId -eq $deck.Id).Count -eq 1) 'One card saved through editor'
        Save-UiEvidence '02-manual-deck'
    }
    Run-UiCase 'WU03 - Unsaved card draft survives normal close and reopening' {
        Click-Id 'AddCardButton'
        Set-UiText 'VietnameseEntry' '犬'
        Set-UiText 'EnglishEntry' 'dog'
        Start-Sleep -Seconds 1
        Click-Id 'PageBackButton'
        Close-TestApp
        Open-TestApp
        Click-Name $testClass
        Click-Name $manualDeck
        Click-Id 'AddCardButton'
        Assert-Ui ((Get-UiValue 'VietnameseEntry') -eq '犬') 'Japanese draft recovered'
        Assert-Ui ((Get-UiValue 'EnglishEntry') -eq 'dog') 'English draft recovered'
        Save-UiEvidence '03-draft-restored'
        Click-Id 'SaveCardButton'
        Click-Id 'PageBackButton'
    }
    Run-UiCase 'WU04 - Native Excel picker, preview, cancel and confirmed 13-card import' {
        Set-UiText 'NewDeckName' $studyDeck
        Click-Id 'AddDeckButton'
        Click-Name $studyDeck
        $script:StudyDeckId = @((Read-TestData).Decks | Where-Object { $_.Name -eq $studyDeck -and $_.ClassId -eq @((Read-TestData).Classes | Where-Object Name -eq $testClass)[0].Id })[0].Id
        Click-Id 'ImportExcelButton'
        Pick-UiWorkbook (Join-Path $fixtureDirectory 'japanese-english-13.xlsx')
        Assert-Ui ((Get-UiValue 'ImportFirstLanguage') -eq 'Japanese') 'Detected Japanese header'
        Assert-Ui ((Get-UiValue 'ImportSecondLanguage') -eq 'English') 'Detected English header'
        Assert-Ui ($null -ne (Find-Ui '13 new · 0 duplicates · 0 invalid' $false)) 'Preview counts match fixture'
        Save-UiEvidence '04-import-preview'
        Click-Id 'ConfirmImportButton'
        Click-Id 'DialogCancelButton'
        Assert-Ui (@((Read-TestData).Cards | Where-Object DeckId -eq $script:StudyDeckId).Count -eq 0) 'Cancel writes no cards'
        Click-Id 'ConfirmImportButton'
        Click-Id 'DialogAcceptButton'
        Wait-Ui 'DialogCancelButton' | Out-Null
        Click-Id 'DialogCancelButton'
        Assert-Ui (@((Read-TestData).Cards | Where-Object DeckId -eq $script:StudyDeckId).Count -eq 13) 'Confirmed UI import adds exactly 13 cards'
        $deck = @((Read-TestData).Decks | Where-Object Id -eq $script:StudyDeckId)[0]
        Assert-Ui ($deck.FirstLanguage -eq 'ja' -and $deck.SecondLanguage -eq 'en') 'Empty deck adopts ja/en after confirmation'
    }
    Run-UiCase 'WU05 - Reversed duplicates, invalid rows and language mismatch are blocked in UI' {
        Click-Id 'ImportExcelButton'
        Pick-UiWorkbook (Join-Path $fixtureDirectory 'reversed-duplicates.xlsx')
        Assert-Ui ($null -ne (Find-Ui '0 new · 13 duplicates · 0 invalid' $false)) 'Reversed columns deduplicate correctly'
        Assert-Ui (-not (Find-Ui 'ConfirmImportButton').Current.IsEnabled) 'All-duplicate import is disabled'
        Pick-UiWorkbook (Join-Path $fixtureDirectory 'invalid-row.xlsx')
        Assert-Ui (-not (Find-Ui 'ConfirmImportButton').Current.IsEnabled) 'One invalid row blocks the whole import'
        Pick-UiWorkbook (Join-Path $fixtureDirectory 'language-mismatch.xlsx')
        Assert-Ui (-not (Find-Ui 'ConfirmImportButton').Current.IsEnabled) 'Different language pair cannot overwrite a populated deck'
        Save-UiEvidence '05-language-mismatch'
        Assert-Ui (@((Read-TestData).Cards | Where-Object DeckId -eq $script:StudyDeckId).Count -eq 13) 'Rejected imports leave original 13 cards untouched'
        Click-Id 'PageBackButton'
    }
    Run-UiCase 'WU06 - Match either side first, deselect, change selection and recover from wrong match' {
        Start-Study 'Match pairs'
        $questions = (Read-TestData).Session.Questions
        Assert-Ui ($questions.Count -eq 6) 'First batch has six pairs'
        $script:FirstBatch = @($questions.CardId)
        Click-Name $questions[0].Answer
        Assert-Ui ($null -eq (Find-Ui 'DialogTitle')) 'Right-first does not open an error'
        Save-UiEvidence '06-right-first-selected'
        Click-Name $questions[0].Answer
        Click-Name $questions[1].Prompt
        Assert-Ui ((Read-TestData).Session.MatchMistakes -eq 0) 'Deselecting a right tile prevents an unwanted match'
        Click-Name $questions[1].Answer
        Assert-Ui ((Read-TestData).Session.MatchedIds.Count -eq 1) 'Left-first pairing works'
        Click-Name $questions[0].Prompt
        Click-Name $questions[2].Prompt
        Click-Name $questions[2].Answer
        Assert-Ui ((Read-TestData).Session.MatchedIds.Count -eq 2) 'Selection can move to another tile on the same side'
        Click-Name $questions[0].Answer
        Click-Name $questions[3].Prompt
        Assert-Ui ((Read-TestData).Session.MatchMistakes -eq 1) 'Incorrect pair increments the counter once'
        Click-Name $questions[0].Answer
        Click-Name $questions[0].Prompt
        Assert-Ui ((Read-TestData).Session.MatchedIds.Count -eq 3) 'Right-first correct pair works after a mismatch'
        Complete-MatchBoard
        Save-UiEvidence '06-first-batch-result'
    }
    Run-UiCase 'WU07 - Second six-pair batch contains no card from first batch' {
        Click-Id 'ContinueLearningButton'
        $second = (Read-TestData).Session
        Assert-Ui ($second.Questions.Count -eq 6) 'Second batch has six pairs'
        Assert-Ui ($second.CompletedPrompts.Count -eq 6) 'Round remembers first six prompts'
        Assert-Ui (@($second.Questions | Where-Object { $script:FirstBatch -contains $_.CardId }).Count -eq 0) 'Second batch has zero repeats'
        $script:SecondBatch = @($second.Questions.CardId)
        Click-Name $second.Questions[0].Answer
        Click-Name $second.Questions[0].Prompt
    }
    Run-UiCase 'WU08 - Resume preserves batch history and reaches final one-pair batch' {
        $oldSession = (Read-TestData).Session
        Resume-Home
        $resumed = (Read-TestData).Session
        Assert-Ui ($resumed.Id -eq $oldSession.Id -and $resumed.MatchedIds.Count -eq 1) 'Current board and matched pair survive restart'
        Assert-Ui ($resumed.CompletedPrompts.Count -eq 6) 'Previous batch survives restart'
        Complete-MatchBoard
        Click-Id 'ContinueLearningButton'
        $third = (Read-TestData).Session
        Assert-Ui ($third.Questions.Count -eq 1 -and $third.CompletedPrompts.Count -eq 12) 'Last batch contains the thirteenth word only'
        Assert-Ui (@($script:FirstBatch + $script:SecondBatch) -notcontains $third.Questions[0].CardId) 'Last word was not shown before'
        Save-UiEvidence '08-last-one-pair'
        Complete-MatchBoard
        Assert-Ui ($null -eq (Find-Ui 'ContinueLearningButton')) 'No automatic loop at the end of a round'
        Assert-Ui ($null -ne (Find-Ui 'RestartRoundButton')) 'Explicit restart is available'
        Save-UiEvidence '08-round-complete'
    }
    Run-UiCase 'WU09 - Only explicit restart starts a new round' {
        Click-Id 'RestartRoundButton'
        $restarted = (Read-TestData).Session
        Assert-Ui ($restarted.Questions.Count -eq 6 -and $restarted.CompletedPrompts.Count -eq 0) 'Explicit restart resets round tracking'
        Click-Id 'StudyBackButton'
        Assert-Ui ($null -eq (Find-Ui 'DialogTitle')) 'Leaving the resumed session has no error dialog'
        if ($null -eq (Find-Ui 'LearningModePicker')) {
            Click-Name $testClass
            Click-Name $studyDeck
        }
    }
    Run-UiCase 'WU10 - Three mastered words, starred/unstarred/all filters and replacement cancellation' {
        Start-Study 'Flashcards'
        Click-Id 'FlipCardButton'
        Click-Id 'RememberButton'
        Click-Id 'RememberButton'
        Click-Id 'RememberButton'
        Assert-Ui (@((Read-TestData).Cards | Where-Object { $_.DeckId -eq $script:StudyDeckId -and $_.IsStarred }).Count -eq 3) 'Three UI ratings mark exactly three words mastered'
        Click-Id 'StudyBackButton'
        Start-Study 'Flashcards' 'English → Japanese' 'Starred · mastered'
        Assert-Ui ((Read-TestData).Session.Questions.Count -eq 3) 'Starred filter includes three words'
        Click-Id 'StudyBackButton'
        Start-Study 'Flashcards' 'English → Japanese' 'Not mastered yet'
        Assert-Ui ((Read-TestData).Session.Questions.Count -eq 10) 'Unstarred filter includes ten words'
        $previous = (Read-TestData).Session.Id
        Click-Id 'StudyBackButton'
        Choose-Ui 'LearningFilterPicker' 'All words'
        Click-Id 'StartLearningButton'
        Click-Id 'DialogCancelButton'
        Assert-Ui ((Read-TestData).Session.Id -eq $previous) 'Cancel replacement preserves previous session'
        Click-Id 'StartLearningButton'
        Click-Id 'DialogAcceptButton'
        Assert-Ui ((Read-TestData).Session.Questions.Count -eq 13) 'All words restores all 13 including three mastered words'
        Save-UiEvidence '10-all-thirteen'
        Click-Id 'StudyBackButton'
    }
    Run-UiCase 'WU11 - Written Japanese-English answers, one wrong answer, result and retry' {
        Start-Study 'Written answers' 'Japanese → English'
        for ($index = 0; $index -lt 13; $index++) {
            $prompt = (Wait-Ui 'QuestionText').Current.Name
            $answer = Get-ExpectedAnswer $prompt $true
            Assert-Ui (-not [string]::IsNullOrEmpty($answer)) 'Visible Japanese question belongs to fixture'
            $input = if ($index -eq 0) { 'not-an-answer' } else { '  ' + $answer.ToUpperInvariant() + '  ' }
            Set-UiText 'WrittenAnswerEntry' $input
            Click-Id 'CheckAnswerButton'
            Click-Id 'NextQuestionButton'
        }
        $result = (Read-TestData).Session
        Assert-Ui (@($result.Attempts | Where-Object Correct).Count -eq 12) 'Written result is 12/13'
        Save-UiEvidence '11-written-result'
        Click-Id 'RetryWrongButton'
        Assert-Ui ((Read-TestData).Session.Questions.Count -eq 1) 'Retry contains only the incorrect question'
        $prompt = (Wait-Ui 'QuestionText').Current.Name
        Set-UiText 'WrittenAnswerEntry' (Get-ExpectedAnswer $prompt $true)
        Click-Id 'CheckAnswerButton'
        Click-Id 'NextQuestionButton'
        Assert-Ui ((Read-TestData).Session.Attempts[0].Correct) 'Retry answer is correct'
        Click-Id 'StudyBackButton'
    }
    Run-UiCase 'WU12 - Multiple choice grades Japanese answers without changing mastery' {
        Start-Study 'Multiple choice'
        for ($index = 0; $index -lt 13; $index++) {
            $prompt = (Wait-Ui 'QuestionText').Current.Name
            $answer = Get-ExpectedAnswer $prompt $false
            Assert-Ui (-not [string]::IsNullOrEmpty($answer)) 'Visible English question belongs to fixture'
            Click-Name $answer
            Click-Id 'NextQuestionButton'
        }
        Assert-Ui (@((Read-TestData).Session.Attempts | Where-Object Correct).Count -eq 13) 'Multiple-choice score is 13/13'
        Assert-Ui (@((Read-TestData).Cards | Where-Object { $_.DeckId -eq $script:StudyDeckId -and $_.IsStarred }).Count -eq 3) 'Quiz does not silently change stars'
        Save-UiEvidence '12-quiz-result'
        Click-Id 'StudyBackButton'
        Click-Id 'PageBackButton'
        Click-Id 'PageBackButton'
    }
    Run-UiCase 'WU13 - History, narrow layout, theme and language persistence' {
        Click-Name 'Results'
        Assert-Ui ($null -ne (Find-Ui $studyDeck $false)) 'Completed sessions appear in history'
        Save-UiEvidence '13-history'
        Click-Name 'Settings'
        Choose-Ui 'ThemePicker' 'Dark'
        Save-UiEvidence '13-dark-theme'
        Choose-Ui 'ThemePicker' 'Light'
        Choose-Ui 'LanguagePicker' 'Tiếng Việt'
        [VocabMateUiWindow]::MoveWindow($script:Process.MainWindowHandle, 40, 40, 460, 880, $true) | Out-Null
        Start-Sleep -Milliseconds 500
        Save-UiEvidence '13-narrow-vietnamese'
        Close-TestApp
        Open-TestApp
        Assert-Ui ($null -ne (Find-Ui 'Thư viện' $false)) 'Vietnamese UI preference survives reopening'
        Click-Name 'Cài đặt'
        Assert-Ui ((Get-UiValue 'ThemePicker') -match 'Sáng') 'Light theme preference survives restart'
        Choose-Ui 'LanguagePicker' 'English'
        Click-Name 'Library'
        Save-UiEvidence '13-final-library'
    }
} catch {
    Write-Error $_ -ErrorAction Continue
    throw
} finally {
    $afterHash = if (Test-Path -LiteralPath $productionData) { (Get-FileHash -LiteralPath $productionData).Hash } else { $null }
    $summary = [PSCustomObject]@{
        StartedUtc = $started.ToString('o'); FinishedUtc = [DateTime]::UtcNow.ToString('o'); Platform = 'Windows';
        Executable = $script:Executable; PackageId = $PackageId; DataPath = $script:DataPath; TestClass = $testClass;
        Passed = @($script:Results | Where-Object Status -eq 'PASS').Count;
        Failed = @($script:Results | Where-Object Status -eq 'FAIL').Count;
        ProductionDataUnchanged = ($productionHash -eq $afterHash)
    }
    $summary | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $script:Artifacts 'summary.json') -Encoding UTF8
    $summary | Format-List
    if (-not $LeaveOpen) { Close-TestApp }
}
