"""Smoke test APK đã cài. Chỉ tạo/xóa việc thử của script; không clear app data.
Usage: python tests/android_smoke.py --serial emulator-5554 [--probe]
"""
import argparse
import json
import re
import subprocess
import time
from pathlib import Path
from xml.etree import ElementTree as ET

PACKAGE = "com.learning.viecnho"
OUTPUT = Path(__file__).resolve().parents[1] / "artifacts" / "android"
parser = argparse.ArgumentParser()
parser.add_argument("--serial", default="emulator-5554")
parser.add_argument("--probe", action="store_true")
args = parser.parse_args()
OUTPUT.mkdir(parents=True, exist_ok=True)


def adb(*parts, binary=False):
    result = subprocess.run(["adb", "-s", args.serial, *parts],
                            capture_output=True, check=True, timeout=45)
    return result.stdout if binary else result.stdout.decode("utf-8", errors="replace").strip()


def tree():
    for _ in range(3):
        adb("shell", "uiautomator", "dump", "/sdcard/viecnho-smoke.xml")
        xml = adb("shell", "cat", "/sdcard/viecnho-smoke.xml")
        try:
            return ET.fromstring(xml[xml.index("<?xml"):])
        except (ValueError, ET.ParseError):
            time.sleep(1)
    raise AssertionError("Cannot read Android UI tree")


def find(value, root=None):
    root = tree() if root is None else root
    for node in root.iter("node"):
        if value in (node.get("text"), node.get("content-desc"), node.get("resource-id"),
                     node.get("resource-id", "").removeprefix(PACKAGE + ":id/")):
            return node
    return None


def tap(value, scroll=False):
    for _ in range(6 if scroll else 1):
        node = find(value)
        if node is not None:
            bounds = list(map(int, re.findall(r"\d+", node.get("bounds", ""))))
            if len(bounds) == 4 and bounds[2] > bounds[0] and bounds[3] > bounds[1]:
                assert node.get("enabled") != "false", f"Disabled: {value}"
                adb("shell", "input", "tap", str((bounds[0] + bounds[2]) // 2),
                    str((bounds[1] + bounds[3]) // 2))
                time.sleep(0.5)
                return
        if scroll:
            swipe(up=True)
    raise AssertionError(f"Missing control: {value}")


def swipe(up=True):
    size = adb("shell", "wm", "size").splitlines()[-1]
    width, height = map(int, re.search(r"(\d+)x(\d+)", size).groups())
    start, end = (0.75, 0.3) if up else (0.3, 0.8)
    adb("shell", "input", "swipe", str(width // 2), str(int(height * start)),
        str(width // 2), str(int(height * end)), "400")
    time.sleep(0.4)


def capture(name):
    ET.ElementTree(tree()).write(OUTPUT / f"{name}.xml", encoding="utf-8", xml_declaration=True)
    (OUTPUT / f"{name}.png").write_bytes(adb("exec-out", "screencap", "-p", binary=True))


def launch():
    component = adb("shell", "cmd", "package", "resolve-activity", "--brief", PACKAGE).splitlines()[-1]
    assert component.startswith(PACKAGE + "/")
    adb("shell", "am", "start", "-W", "-n", component)
    time.sleep(2)
    assert adb("shell", "pidof", PACKAGE), "App exited during startup; check logcat"


def data():
    return json.loads(adb("shell", "run-as", PACKAGE, "cat", "files/todos.json"))


launch()
if args.probe:
    capture("probe")
    for node in tree().iter("node"):
        if node.get("text") or node.get("content-desc"):
            print(node.get("resource-id"), node.get("text"), node.get("content-desc"), node.get("bounds"))
    raise SystemExit(0)

checks = []


def passed(name):
    checks.append(name)
    print("PASS:", name, flush=True)
    (OUTPUT / "results.json").write_text(json.dumps(checks, ensure_ascii=False, indent=2), encoding="utf-8")


try:
    capture("01-initial")
    tap("AddTaskButton")
    assert find("Bạn hãy nhập tên việc cần làm.") is not None
    passed("Blank input shows inline validation")

    title = "MAUI smoke " + str(int(time.time()))
    tap("NewTaskEntry")
    adb("shell", "input", "text", title.replace(" ", "%s"))
    adb("shell", "input", "keyevent", "KEYCODE_BACK")
    tap("AddTaskButton")
    item = next(item for item in data() if item["Title"] == title)
    passed("Add writes the actual JSON file")

    tap("Hoàn thành: " + title, scroll=True)
    assert next(row for row in data() if row["Id"] == item["Id"])["IsCompleted"]
    capture("02-completed")
    passed("Toggle persists completion")

    adb("shell", "am", "force-stop", PACKAGE)
    launch()
    tap("Làm lại: " + title, scroll=True)
    assert not next(row for row in data() if row["Id"] == item["Id"])["IsCompleted"]
    passed("App restart restores saved task and allows reopening")

    tap("Xóa việc: " + title, scroll=True)
    tap("Giữ lại")
    assert any(row["Id"] == item["Id"] for row in data())
    passed("Cancel deletion preserves item")
    tap("Xóa việc: " + title)
    tap("Xóa")
    assert all(row["Id"] != item["Id"] for row in data())
    passed("Confirm deletion persists removal")

    tap("Khám phá")
    tap("IncrementButton")
    tap("IncrementButton")
    assert find("CounterValue").get("text") == "2"
    tap("Việc nhỏ")
    tap("Khám phá")
    assert find("CounterValue").get("text") == "2"
    tap("ResetCounterButton")
    assert find("CounterValue").get("text") == "0"
    capture("03-learn")
    passed("Counter binding, tab state and reset work")
    print(f"All {len(checks)} Android smoke checks passed.", flush=True)
except Exception:
    capture("failure")
    raise
