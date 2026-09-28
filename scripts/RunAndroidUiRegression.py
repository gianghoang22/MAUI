import argparse
import json
import sys
import time
from datetime import datetime, timezone
from pathlib import Path
import android_ui_regression as ui

WORDS = {"cat": "猫", "dog": "犬", "water": "水", "book": "本", "school": "学校", "sun": "太陽", "moon": "月", "flower": "花", "mountain": "山", "river": "川", "friend": "友達", "car": "車", "train": "電車"}
STATE = {}

def top():
    for attempt in range(15):
        root = ui.tree()
        if ui.find("PageBackButton", root) is not None or ui.find("StudySetupButton", root) is not None:
            return
        ui.adb("shell", "input", "swipe", "1000", "650", "1000", "1900", "350")
    time.sleep(0.2)

def visible(control):
    for attempt in range(8):
        node = ui.find(control)
        if node is not None:
            return node
        ui.adb("shell", "input", "swipe", "1000", "1900", "1000", "1100", "550")
    raise AssertionError("Control not reachable: " + control)

def back():
    top()
    ui.tap("StudyBackButton" if ui.find("StudyBackButton") is not None else "PageBackButton")
    assert ui.find("DialogTitle") is None, "Navigation showed an error dialog"

def start_study(mode, direction="English → Japanese", selected_filter="All words"):
    top()
    ui.choose("LearningModePicker", mode)
    ui.choose("DirectionPicker", direction)
    ui.choose("LearningFilterPicker", selected_filter)
    ui.tap("StartLearningButton", scroll=True)
    if ui.find("DialogAcceptButton") is not None:
        ui.tap("DialogAcceptButton")
    assert ui.find("StudySetupButton") is not None

def complete_matches():
    current = ui.data()["Session"]
    for question in current["Questions"]:
        if question["CardId"] in current["MatchedIds"]:
            continue
        assert WORDS[question["Prompt"]] == question["Answer"]
        top()
        ui.tap(question["Answer"], scroll=True)
        top()
        ui.tap(question["Prompt"], scroll=True)
    assert ui.data()["Session"]["FinishedAt"] is not None

def reopen():
    ui.adb("shell", "input", "keyevent", "KEYCODE_HOME")
    time.sleep(1)
    ui.adb("shell", "am", "force-stop", ui.PACKAGE)
    ui.launch()

def cards():
    return [card for card in ui.data()["Cards"] if card["DeckId"] == STATE["deck_id"]]

def isolation():
    assert ui.PACKAGE.startswith("com.vocabmate.uia")
    ui.tap("Settings" if ui.find("Settings") is not None else "Cài đặt")
    ui.choose("LanguagePicker", "English")
    ui.choose("ThemePicker", "Light")
    ui.capture("01-isolated-settings")
    ui.tap("Library")

def create_deck():
    ui.tap("ToggleCreateClassButton", scroll=True)
    ui.set_text("NewClassName", STATE["class"])
    ui.tap("AddClassButton", scroll=True)
    ui.tap(STATE["class"], scroll=True)
    ui.set_text("NewDeckName", STATE["deck"])
    ui.tap("AddDeckButton", scroll=True)
    ui.tap(STATE["deck"], scroll=True)
    snapshot = ui.data()
    class_id = next(item["Id"] for item in snapshot["Classes"] if item["Name"] == STATE["class"])
    STATE["deck_id"] = next(item["Id"] for item in snapshot["Decks"] if item["ClassId"] == class_id)
    assert not cards()

def import_workbook():
    ui.tap("ImportExcelButton", scroll=True)
    ui.pick_workbook("japanese-english-13.xlsx")
    assert "Japanese" in visible("ImportFirstLanguage").get("text")
    assert "English" in visible("ImportSecondLanguage").get("text")
    ui.capture("03-excel-preview")
    ui.tap("ConfirmImportButton", scroll=True)
    ui.tap("DialogCancelButton")
    assert not cards()
    ui.tap("ConfirmImportButton", scroll=True)
    ui.tap("DialogAcceptButton")
    ui.tap("DialogCancelButton")
    assert len(cards()) == 13
    assert {(card["English"], card["Vietnamese"]) for card in cards()} == set(WORDS.items())
    deck = next(item for item in ui.data()["Decks"] if item["Id"] == STATE["deck_id"])
    assert (deck["FirstLanguage"], deck["SecondLanguage"]) == ("ja", "en")

def rejected_imports():
    top()
    ui.tap("ImportExcelButton")
    for name in ("reversed-duplicates.xlsx", "invalid-row.xlsx", "language-mismatch.xlsx"):
        top()
        ui.pick_workbook(name)
        assert visible("ConfirmImportButton").get("enabled") == "false", name
        assert len(cards()) == 13
    ui.capture("04-language-mismatch")
    back()

def first_match():
    start_study("Match pairs")
    questions = ui.data()["Session"]["Questions"]
    assert len(questions) == 6
    STATE["first"] = {item["CardId"] for item in questions}
    top()
    ui.tap(questions[0]["Answer"], scroll=True)
    ui.tap(questions[0]["Answer"], scroll=True)
    assert not ui.data()["Session"]["MatchedIds"]
    top()
    ui.tap(questions[0]["Answer"], scroll=True)
    top()
    ui.tap(questions[1]["Answer"], scroll=True)
    top()
    ui.tap(questions[1]["Prompt"], scroll=True)
    assert len(ui.data()["Session"]["MatchedIds"]) == 1
    top()
    ui.tap(questions[0]["Prompt"], scroll=True)
    top()
    ui.tap(questions[2]["Answer"], scroll=True)
    assert len(ui.data()["Session"]["MatchedIds"]) == 1
    complete_matches()
    ui.capture("05-first-six-complete")

def remaining_matches():
    ui.tap("ContinueLearningButton", scroll=True)
    second = {item["CardId"] for item in ui.data()["Session"]["Questions"]}
    assert len(second) == 6 and not STATE["first"] & second
    question = ui.data()["Session"]["Questions"][0]
    top()
    ui.tap(question["Prompt"], scroll=True)
    top()
    ui.tap(question["Answer"], scroll=True)
    reopen()
    ui.tap("ResumeLearningButton", scroll=True)
    assert ui.data()["Session"]["MatchedIds"] == [question["CardId"]]
    complete_matches()
    ui.tap("ContinueLearningButton", scroll=True)
    last = {item["CardId"] for item in ui.data()["Session"]["Questions"]}
    assert len(last) == 1 and not last & (STATE["first"] | second)
    complete_matches()
    assert len(STATE["first"] | second | last) == 13
    assert ui.find("ContinueLearningButton") is None
    ui.capture("06-final-one-no-repeat")
    ui.tap("RestartRoundButton", scroll=True)
    assert len(ui.data()["Session"]["Questions"]) == 6
    back()
    if ui.find("LearningModePicker") is None:
        ui.tap(STATE["class"], scroll=True)
        ui.tap(STATE["deck"], scroll=True)

def mastery_filters():
    start_study("Flashcards")
    for index in range(3):
        ui.tap("FlipCardButton", scroll=True)
        ui.tap("RememberButton", scroll=True)
    assert sum(card["IsStarred"] for card in cards()) == 3
    ui.capture("07-three-mastered")
    back()
    for selected_filter, count in (("Starred · mastered", 3), ("Not mastered yet", 10), ("All words", 13)):
        start_study("Flashcards", selected_filter=selected_filter)
        assert len(ui.data()["Session"]["Questions"]) == count
        back()

def written():
    start_study("Written answers", "Japanese → English")
    reverse = {value: key for key, value in WORDS.items()}
    for index in range(13):
        top()
        answer = reverse[visible("QuestionText").get("text")]
        ui.set_text("WrittenAnswerEntry", "incorrect" if index == 0 else answer.upper())
        ui.tap("CheckAnswerButton", scroll=True)
        ui.tap("NextQuestionButton", scroll=True)
    assert sum(attempt["Correct"] for attempt in ui.data()["Session"]["Attempts"]) == 12
    ui.capture("08-written-twelve-of-thirteen")
    ui.tap("RetryWrongButton", scroll=True)
    assert len(ui.data()["Session"]["Questions"]) == 1
    top()
    ui.set_text("WrittenAnswerEntry", reverse[visible("QuestionText").get("text")])
    ui.tap("CheckAnswerButton", scroll=True)
    ui.tap("NextQuestionButton", scroll=True)
    assert ui.data()["Session"]["Attempts"][0]["Correct"]
    back()

def quiz():
    start_study("Multiple choice")
    for index in range(13):
        top()
        prompt = visible("QuestionText").get("text")
        ui.tap(WORDS[prompt], scroll=True)
        ui.tap("NextQuestionButton", scroll=True)
    assert sum(attempt["Correct"] for attempt in ui.data()["Session"]["Attempts"]) == 13
    assert sum(card["IsStarred"] for card in cards()) == 3
    ui.capture("09-quiz-thirteen-of-thirteen")
    back()
    back()
    back()

def preferences():
    ui.tap("Results")
    assert ui.find(STATE["deck"]) is not None
    ui.capture("10-history")
    ui.tap("Settings")
    ui.choose("ThemePicker", "Dark")
    ui.capture("10-dark")
    ui.choose("ThemePicker", "Light")
    ui.choose("LanguagePicker", "Tiếng Việt")
    ui.capture("10-vietnamese-light")
    reopen()
    assert ui.find("Thư viện") is not None
    ui.tap("Cài đặt")
    assert "Sáng" in visible("ThemePicker").get("text")
    ui.choose("LanguagePicker", "English")
    ui.tap("Library")

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--artifacts", type=Path, default=ui.ARTIFACTS)
    arguments = parser.parse_args()
    ui.ARTIFACTS = arguments.artifacts.resolve()
    ui.ARTIFACTS.mkdir(parents=True, exist_ok=True)
    started = datetime.now(timezone.utc)
    STATE["class"] = "UIA Android " + started.strftime("%H%M%S")
    STATE["deck"] = "Japanese 13 " + started.strftime("%H%M%S")
    fixtures = ui.ROOT / "MauiApp1/obj/UiAutomationEvidence/fixtures"
    ui.subprocess.run([sys.executable, str(ui.ROOT / "scripts/create_ui_fixtures.py"), str(fixtures)], check=True)
    for workbook in fixtures.glob("*.xlsx"):
        ui.adb("push", str(workbook), "/sdcard/Download/" + workbook.name)
    ui.adb("shell", "am", "force-stop", ui.PACKAGE)
    ui.launch()
    try:
        for name, action in [
            ("AU01 - Isolated package, English and light theme", isolation),
            ("AU02 - Create class and deck using touchscreen", create_deck),
            ("AU03 - Real Excel picker, Japanese headers, cancel and import", import_workbook),
            ("AU04 - Block duplicate, invalid and mismatched imports", rejected_imports),
            ("AU05 - Either-side matching, deselect, selection change and wrong pair", first_match),
            ("AU06 - Disjoint 6/6/1 batches, process restart and explicit new round", remaining_matches),
            ("AU07 - Three mastered cards and 3/10/13 study filters", mastery_filters),
            ("AU08 - Written answers 12/13 and retry wrong answer", written),
            ("AU09 - Multiple choice 13/13 without changing mastery", quiz),
            ("AU10 - History, themes and locale survive restart", preferences),
        ]:
            ui.run_case(name, action)
    finally:
        summary = {"startedUtc": started.isoformat(), "finishedUtc": datetime.now(timezone.utc).isoformat(), "platform": "Android", "serial": ui.SERIAL, "packageId": ui.PACKAGE, "testClass": STATE["class"], "passed": sum(item["status"] == "PASS" for item in ui.RESULTS), "failed": sum(item["status"] == "FAIL" for item in ui.RESULTS)}
        (ui.ARTIFACTS / "summary.json").write_text(json.dumps(summary, indent=2), encoding="utf-8")
        print(json.dumps(summary, indent=2), flush=True)
        process_id = ui.adb("shell", "pidof", ui.PACKAGE, check=False)
        if process_id:
            (ui.ARTIFACTS / "logcat.txt").write_text(ui.adb("logcat", "-d", "--pid=" + process_id), encoding="utf-8")

if __name__ == "__main__":
    main()
