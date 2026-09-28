from pathlib import Path
import android_ui_regression as ui
import RunAndroidUiRegression as suite


def settings():
    ui.tap("Settings" if ui.find("Settings") is not None else "Cài đặt")
    ui.choose("LanguagePicker", "English")
    ui.choose("ThemePicker", "Light")
    ui.tap("ThemePicker")
    assert ui.find("PickerOption1") is not None
    assert ui.find("android:id/alertTitle") is None
    ui.capture("01-light-options")
    ui.tap("DialogCancelButton")
    assert "Light" in ui.find("ThemePicker").get("text")
    ui.choose("ThemePicker", "Dark")
    ui.tap("ThemePicker")
    ui.capture("02-dark-options")
    ui.tap("DialogCancelButton")
    ui.choose("LanguagePicker", "Tiếng Việt")
    ui.choose("ThemePicker", "Sáng")
    ui.tap("LanguagePicker")
    ui.capture("03-vietnamese-options")
    ui.adb("shell", "input", "keyevent", "KEYCODE_BACK")
    assert ui.find("PickerOption0") is None
    assert "Tiếng Việt" in ui.find("LanguagePicker").get("text")
    suite.reopen()
    ui.tap("Cài đặt")
    assert "Sáng" in ui.find("ThemePicker").get("text")
    ui.choose("LanguagePicker", "English")
    ui.tap("Library")


def study_selectors():
    snapshot = ui.data()
    deck = next((item for item in reversed(snapshot["Decks"]) if item["FirstLanguage"] == "ja" and item["SecondLanguage"] == "en" and sum(card["DeckId"] == item["Id"] for card in snapshot["Cards"]) == 13), None)
    assert deck is not None, "Run the Android regression first to create the isolated 13-card Japanese deck"
    classroom = next(item for item in snapshot["Classes"] if item["Id"] == deck["ClassId"])
    ui.tap(classroom["Name"], scroll=True)
    ui.tap(deck["Name"], scroll=True)
    suite.start_study("Flashcards")
    session_id = ui.data()["Session"]["Id"]
    ui.tap("StudySetupButton")
    ui.choose("StudyModePicker", "Written answers")
    ui.choose("StudyDirectionPicker", "Japanese → English")
    ui.choose("StudyFilterPicker", "Starred · mastered")
    ui.tap("CancelStudySetupButton", scroll=True)
    assert ui.data()["Session"]["Id"] == session_id
    suite.top()
    ui.tap("StudySetupButton")
    ui.choose("StudyModePicker", "Written answers")
    ui.choose("StudyDirectionPicker", "Japanese → English")
    ui.choose("StudyFilterPicker", "All words")
    ui.tap("ApplyStudySetupButton", scroll=True)
    ui.tap("DialogAcceptButton")
    current = ui.data()["Session"]
    assert current["Mode"] == 2 and current["Direction"] == 1 and current["Filter"] == 2
    assert len(current["Questions"]) == 13
    ui.capture("04-written-setup-applied")
    suite.back()
    ui.choose("CardListFilterPicker", "Starred · mastered")
    ui.capture("05-list-filter")
    ui.choose("CardListFilterPicker", "All words")


def main():
    ui.ARTIFACTS = ui.ROOT / "MauiApp1/obj/UiAutomationEvidence/ThemedPickerAndroid"
    ui.adb("shell", "am", "force-stop", ui.PACKAGE)
    ui.launch()
    ui.run_case("PA01 - Themed options, cancel, Android Back, locale and persistence", settings)
    ui.run_case("PA02 - All seven learning/list pickers bind and cancel/apply correctly", study_selectors)


if __name__ == "__main__":
    main()
