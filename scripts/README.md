# UI automation — VocabMate

## Cách hoạt động và phạm vi

- Windows dùng `UIAutomationClient`: tìm control thật bằng AutomationId, kích hoạt Invoke/SelectionItem, nhập bằng ValuePattern. Hộp chọn Excel thật được điền bằng Win32 message; không mock navigation/repository/file picker.
- Android dùng `adb shell input tap/swipe`, bàn phím Android và `uiautomator dump` trên emulator. File Excel được chọn trong DocumentsUI thật.
- Chỉ đọc JSON của **bản test** làm oracle để đối chiếu số lượng, ID, điểm và checkpoint. Mọi tạo/sửa/xóa dữ liệu trong các kịch bản đều đi qua UI.
- Câu hỏi được đối chiếu với bảng từ Nhật–Anh độc lập trong test, không lấy đáp án từ engine để mặc định coi kết quả engine là đúng.
- Mỗi lần chạy tạo lớp riêng theo thời gian. CRUD chỉ xóa lớp/deck do chính lần chạy đó tạo. Không clear data, gỡ app hay kill app thật.
- Mỗi suite dừng ở lỗi đầu tiên; đọc `results.json` và `summary.json` để biết case thực sự chạy. Không tính case chưa chạy là PASS.
- Các suite này không tương đương việc đã chạy toàn bộ checklist thủ công trong `TEST_CASES_DEMO.md`.

## Điều kiện

Windows desktop đang đăng nhập, .NET 10 + MAUI Windows/Android workload, Python 3, Android SDK và emulator `emulator-5554`. Android runner được thiết kế cho portrait **1080 × 2400**, giao diện hệ thống DocumentsUI tiếng Anh. Không đổi kích thước/xoay màn hình hoặc bấm đồng thời vào cửa sổ test khi suite đang chạy.

Không cần pip, Appium server hoặc WinAppDriver. Chạy các lệnh sau từ root repository trong PowerShell. Hai suite Windows **không chạy đồng thời**; Android có thể chạy cùng Windows.

## Build bản riêng, không chạm dữ liệu thật

ApplicationId cố định cho đợt test này: `com.vocabmate.uia20260928`, khác app thật `com.vocabmate.app`.

```powershell
dotnet build MauiApp1/MauiApp1.csproj -c Release -f net10.0-windows10.0.19041.0 `
  -p:ApplicationId=com.vocabmate.uia20260928 `
  -p:OutputPath=bin/UiAutomationWindows/ -p:IntermediateOutputPath=obj/UiAutomationWindows/ `
  -p:AppendTargetFrameworkToOutputPath=false -p:AppendRuntimeIdentifierToOutputPath=false -v:q

dotnet build MauiApp1/MauiApp1.csproj -c Debug -f net10.0-android `
  -p:ApplicationId=com.vocabmate.uia20260928 `
  -p:OutputPath=bin/UiAutomationAndroid/ -p:IntermediateOutputPath=obj/UiAutomationAndroid/ `
  -p:AppendTargetFrameworkToOutputPath=false -p:EmbedAssembliesIntoApk=true -v:q

$adb = "$env:LOCALAPPDATA/Android/Sdk/platform-tools/adb.exe"
& $adb -s emulator-5554 install -r MauiApp1/bin/UiAutomationAndroid/com.vocabmate.uia20260928-Signed.apk
```

`EmbedAssembliesIntoApk=true` cần thiết khi cài APK Debug trực tiếp bằng adb, thay vì fast deployment của IDE. Android Debug cho phép `run-as` đọc oracle riêng của package test. Windows test xác nhận đường dẫn ở trang Settings trước khi ghi và so sánh SHA-256 JSON app thật trước/sau.

## Chạy

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/RunWindowsUiRegression.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/RunWindowsCrudRegression.ps1
$env:PYTHONUTF8 = '1'
python scripts/RunAndroidUiRegression.py

dotnet test MauiApp1.Tests/MauiApp1.Tests.csproj -c Release `
  --logger "trx;LogFileName=full-regression.trx" `
  --results-directory MauiApp1/obj/UiAutomationEvidence/UnitTests
```

Các script PowerShell chứa Unicode có UTF-8 BOM để chạy đúng với Windows PowerShell 5.1. Mỗi runner nhận `-Artifacts` (PowerShell) hoặc `--artifacts` (Python) để giữ riêng evidence các lần chạy. Windows học có `-LeaveOpen` để giữ cửa sổ bản test phục vụ demo.

## Suites

| Suite | Nhóm kiểm tra |
| --- | --- |
| `RunWindowsUiRegression.ps1` | 13 kịch bản: ngôn ngữ/theme; tạo lớp/deck/thẻ Unicode; khôi phục draft; Excel preview/cancel/import; trùng/invalid/sai cặp ngôn ngữ; chọn ghép hai phía; vòng 6/6/1; reopen/resume/restart/Back; bộ lọc 3/10/13; viết 12/13 + retry; quiz 13/13; lịch sử và cửa sổ hẹp |
| `RunWindowsCrudRegression.ps1` | 7 kịch bản: tên trống/trùng; tìm kiếm/đổi tên lớp/deck; thẻ không hợp lệ/trùng; sửa giữ sao; bỏ draft; hai nút Use all cards và setup/cancel; hủy/xác nhận xóa thẻ/deck/lớp và cascade |
| `RunAndroidUiRegression.py` | 10 kịch bản trên emulator: tạo dữ liệu bằng tap; Excel thật; validation import; ghép cả hai phía 6/6/1; checkpoint qua đóng process; lọc 3/10/13; viết/retry; quiz; lịch sử/theme/locale |

Fixture `.xlsx` được tạo bằng ZIP/XML trong `create_ui_fixtures.py`, không dùng exporter của app. Có 13 cặp Nhật–Anh, cột đảo toàn bộ trùng, một hàng thiếu dữ liệu và một cặp ngôn ngữ không tương thích. Android dùng import cho Unicode; `adb input text` trong helper chỉ nhập ASCII.

## Evidence

Mặc định nằm trong `MauiApp1/obj/UiAutomationEvidence/` (được gitignore):

- `Windows/`, `WindowsCrud/`, `Android/`: `results.json`, `summary.json`, ảnh PNG và UI tree JSON/XML.
- `fixtures/`: các workbook dùng trong test.
- `UnitTests/`: kết quả TRX.
- Khi thất bại: `failure.png` + tree tương ứng. Android còn thu logcat của process test khi process vẫn chạy.

Suite học giữ dữ liệu demo trong package riêng. Các lần chạy lỗi có thể để lại lớp `UIA-*` trong bản test; có thể xóa qua UI của **bản test**, không xóa thư mục app thật.

## Chẩn đoán lỗi Windows (tùy chọn)

Để lấy stack trace của lỗi đã bị ViewModel chuyển thành thông báo thân thiện, thêm build property:

```powershell
$diagnostics = (Resolve-Path scripts/UiAutomationDiagnostics.targets).Path
# Thêm vào lệnh build Windows cô lập:
# "-p:CustomAfterMicrosoftCommonTargets=$diagnostics"
$env:VOCABMATE_UIA_DIAGNOSTICS = [IO.Path]::GetFullPath('MauiApp1/obj/UiAutomationEvidence/exceptions.log')
```

Target chỉ thêm `UiAutomationDiagnostics.cs` khi ApplicationId là package test. Build thường không chứa module này. Log first-chance bao gồm cả lỗi validation đã được xử lý; không coi mọi dòng exception là một test thất bại.

## Những gì vẫn cần kiểm tra riêng

Chất lượng/âm thanh TTS thực tế; native share/export end-to-end; thiết bị Android vật lý và phiên bản OS khác; TalkBack/Narrator; font hệ thống lớn; landscape; 2.000 thẻ/10 MB; độ bền nhiều giờ, cạn pin/disk và interruption đặc biệt. Ảnh theme không chứng minh việc dùng nhiều giờ sẽ không mỏi mắt.
