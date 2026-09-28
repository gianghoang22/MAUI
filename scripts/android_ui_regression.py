import argparse
import json
import re
import subprocess
import time
from datetime import datetime, timezone
from pathlib import Path
from xml.etree import ElementTree

PACKAGE = "com.vocabmate.uia20260928"
ROOT = Path(__file__).resolve().parent.parent
ADB = Path.home() / "AppData/Local/Android/Sdk/platform-tools/adb.exe"
ARTIFACTS = ROOT / "MauiApp1/obj/UiAutomationEvidence/Android"
SERIAL = "emulator-5554"
RESULTS = []


def adb(*arguments, check=True):
    result = subprocess.run([str(ADB), "-s", SERIAL, *arguments], capture_output=True, timeout=40)
    if check and result.returncode:
        details = result.stderr.decode("utf-8", errors="replace") + result.stdout.decode("utf-8", errors="replace")
        raise RuntimeError(f"adb {arguments!r} exited {result.returncode}: {details}")
    return result.stdout.decode("utf-8", errors="replace").strip()


def tree():
    for attempt in range(10):
        adb("shell", "rm", "-f", "/sdcard/vocabmate-uia-tree.xml")
        adb("shell", "uiautomator", "dump", "/sdcard/vocabmate-uia-tree.xml", check=False)
        text = adb("shell", "cat", "/sdcard/vocabmate-uia-tree.xml", check=False)
        try:
            return ElementTree.fromstring(text[text.index("<?xml"):])
        except (ValueError, ElementTree.ParseError):
            time.sleep(1)
    raise RuntimeError("Could not read Android UI tree")


def find(value, root=None):
    root = tree() if root is None else root
    for node in root.iter("node"):
        if node.get("resource-id") in (value, PACKAGE + ":id/" + value) or node.get("text") == value or node.get("content-desc") == value:
            return node
    return None


def tap(value, scroll=False):
    for attempt in range(7 if scroll else 2):
        node = find(value)
        if node is not None:
            bounds = list(map(int, re.findall(r"\d+", node.get("bounds", ""))))
            if len(bounds) == 4 and bounds[2] > bounds[0] and bounds[3] > bounds[1] and bounds[1] >= 70 and bounds[3] <= 2340:
                if node.get("enabled") == "false":
                    raise AssertionError("Disabled control: " + value)
                ARTIFACTS.mkdir(parents=True, exist_ok=True)
                with (ARTIFACTS / "actions.jsonl").open("a", encoding="utf-8") as log:
                    log.write(json.dumps({"utc": datetime.now(timezone.utc).isoformat(), "control": value, "bounds": bounds, "package": node.get("package")}, ensure_ascii=False) + "\n")
                adb("shell", "input", "tap", str((bounds[0] + bounds[2]) // 2), str((bounds[1] + bounds[3]) // 2))
                time.sleep(0.7)
                return
        if scroll:
            adb("shell", "input", "swipe", "1000", "1900", "1000", "1100", "550")
        time.sleep(0.25)
    raise AssertionError("Android control not found: " + value)


def set_text(control, value):
    if not value.isascii():
        raise ValueError("adb keyboard input is ASCII only; Unicode coverage uses real Excel import")
    tap(control, scroll=True)
    adb("shell", "input", "keyevent", "KEYCODE_MOVE_END")
    adb("shell", "input", "keycombination", "113", "29")
    adb("shell", "input", "keyevent", "KEYCODE_DEL")
    if value:
        adb("shell", "input", "text", value.replace(" ", "%s"))
    adb("shell", "input", "keyevent", "KEYCODE_BACK")


def choose(control, option):
    tap(control, scroll=True)
    tap(option)


def data():
    return json.loads(adb("shell", "run-as", PACKAGE, "cat", "files/vocabmate.json"))


def capture(name):
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    root = tree()
    ElementTree.ElementTree(root).write(ARTIFACTS / (name + ".xml"), encoding="utf-8", xml_declaration=True)
    image = subprocess.run([str(ADB), "-s", SERIAL, "exec-out", "screencap", "-p"], capture_output=True, check=True, timeout=30).stdout
    (ARTIFACTS / (name + ".png")).write_bytes(image)


def launch():
    component = adb("shell", "cmd", "package", "resolve-activity", "--brief", PACKAGE).splitlines()[-1]
    assert component.startswith(PACKAGE + "/"), "Unexpected launch target: " + component
    adb("shell", "am", "start", "-W", "-n", component, "-f", "0x10008000")
    time.sleep(3)
    assert adb("shell", "pidof", PACKAGE, check=False), "App is not running"
    for attempt in range(10):
        root = tree()
        if any(node.get("resource-id") == PACKAGE + ":id/ToggleCreateClassButton" for node in root.iter("node")):
            return
        time.sleep(1)
    raise AssertionError("Library did not become ready after launch")


def run_case(name, action):
    started = time.monotonic()
    try:
        action()
        RESULTS.append({"name": name, "status": "PASS", "seconds": round(time.monotonic() - started, 2)})
        print("PASS: " + name, flush=True)
    except Exception as error:
        RESULTS.append({"name": name, "status": "FAIL", "error": str(error)})
        capture("failure")
        raise
    finally:
        ARTIFACTS.mkdir(parents=True, exist_ok=True)
        (ARTIFACTS / "results.json").write_text(json.dumps(RESULTS, ensure_ascii=False, indent=2), encoding="utf-8")


def pick_workbook(name):
    tap("ChooseWorkbookButton", scroll=True)
    for attempt in range(10):
        root = tree()
        if any(node.get("package") == "com.google.android.documentsui" for node in root.iter("node")):
            break
        time.sleep(0.5)
    else:
        raise AssertionError("Native DocumentsUI did not open")
    if find(name, root) is None:
        node = find("Show roots", root)
        if node is None:
            node = find("Show navigation drawer", root)
        if node is not None:
            tap(node.get("content-desc") or node.get("text"))
            tap("Downloads")
    tap(name, scroll=True)
    for attempt in range(10):
        if find("ImportFirstLanguage") is not None:
            return
        time.sleep(0.5)
    raise AssertionError("Import language preview did not appear")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--probe", action="store_true")
    arguments = parser.parse_args()
    ARTIFACTS.mkdir(parents=True, exist_ok=True)
    launch()
    if arguments.probe:
        capture("initial")
        for node in tree().iter("node"):
            if node.get("text") or node.get("content-desc"):
                print(node.get("resource-id"), node.get("text"), node.get("content-desc"), node.get("bounds"))
        return


if __name__ == "__main__":
    main()
