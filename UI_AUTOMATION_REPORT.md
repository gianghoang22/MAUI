# Báo cáo UI automation — VocabMate

Ngày chạy: **28/09/2026**. Đây là kết quả chạy thực tế, khác checklist thủ công trong `TEST_CASES_DEMO.md`.

## 1. Kết quả

| Bộ kiểm tra | Kết quả | Evidence |
| --- | --- | --- |
| Windows UI — học, import, khôi phục, cài đặt | **13/13 PASS** | `MauiApp1/obj/UiAutomationEvidence/WindowsPassed/summary.json` |
| Windows UI — CRUD, validation, Use all cards, cascade | **7/7 PASS** | `MauiApp1/obj/UiAutomationEvidence/WindowsCrudWait/summary.json` |
| Android UI — emulator | **10/10 PASS** | `MauiApp1/obj/UiAutomationEvidence/AndroidExplicit/summary.json` |
| Unit test .NET 10, Release | **68/68 PASS**, 0 skipped | `MauiApp1/obj/UiAutomationEvidence/UnitTests/full-regression.trx` |
| Build Windows Release thường | **PASS**, 0 warning / 0 error | `net10.0-windows10.0.19041.0` |
| Build Android Release thường | **PASS**, 0 warning / 0 error | `net10.0-android` |

Tổng cộng **30 kịch bản UI đạt**, cộng riêng **68 unit test đạt**. Android chạy trọn 10 kịch bản liên tục từ **11:17:56 đến 11:39:38, UTC+7**. Không cộng case chưa chạy hoặc lần chạy dừng giữa chừng thành PASS. Các thư mục khác như `WindowsDiagnostic`, `WindowsFixed`, `WindowsGuarded`, `AndroidFull`, `AndroidRetest` giữ evidence điều tra/lỗi script ban đầu, không phải kết quả cuối.

## 2. Đã bấm trực tiếp những gì trên Windows?

| ID | Kịch bản đã đạt |
| --- | --- |
| WU01 | Xác nhận đường dẫn bản test, đổi English và theme Light |
| WU02 | Tạo lớp/bộ; tự nhập nhãn Japanese/English; lưu thẻ chữ Nhật |
| WU03 | Gõ nháp, Back, đóng/mở app; nháp Unicode được khôi phục đúng |
| WU04 | Chọn Excel bằng dialog Windows thật; preview 13 từ; hủy không ghi; xác nhận nhập đúng 13 từ, nhận ja/en |
| WU05 | File đảo cột được nhận là 13 dòng trùng; một dòng invalid hoặc sai cặp ngôn ngữ chặn import; dữ liệu cũ không đổi |
| WU06 | Ghép từ phía phải hoặc trái; bấm lại để bỏ chọn; đổi lựa chọn cùng phía; ghép sai rồi sửa |
| WU07 | Nhóm sáu cặp thứ hai không chứa ID nào của nhóm đầu |
| WU08 | Đóng/mở giữa nhóm, khôi phục cặp đã ghép; đến nhóm cuối một cặp; hết 13 từ không tự lặp |
| WU09 | Chỉ nút bắt đầu vòng mới reset lịch sử vòng; Back sau resume không hiện lỗi |
| WU10 | Đánh dấu ba từ đã thuộc; bộ lọc đã thuộc/chưa thuộc/tất cả cho 3/10/13 câu; hủy thay phiên giữ phiên cũ |
| WU11 | Viết Nhật → Anh, cố ý sai một câu: 12/13; chữ hoa/khoảng trắng được chuẩn hóa; retry đúng một câu sai |
| WU12 | Trắc nghiệm Anh → Nhật đạt 13/13, không thay ba dấu sao đã có |
| WU13 | Kết quả hiện trong lịch sử; theme Dark/Light; tiếng Việt; cửa sổ 460 × 880; mở lại giữ lựa chọn |
| WC01 | Tên lớp rỗng bị chặn, tên trùng khác hoa/thường bị chặn; tìm không thấy; đổi tên giữ ID |
| WC02 | Bộ từ rỗng/trùng, tìm kiếm/đổi tên; bộ rỗng không bắt đầu học |
| WC03 | Thẻ thiếu một mặt không lưu; thêm hợp lệ; trùng chuẩn hóa bị chặn; bỏ nháp không khôi phục lại |
| WC04 | Tìm hai phía; kết hợp sao với tìm kiếm; sửa từ giữ ID/sao; bỏ chỉnh sửa không thay thẻ đã lưu |
| WC04B | Bấm **Use all cards** ngay ở deck và trong **Study setup**: đều bao gồm cả thẻ có sao; hủy setup giữ phiên cũ |
| WC05 | Hủy/xác nhận xóa thẻ; hủy/xác nhận xóa bộ; xóa bộ dọn thẻ, nháp và session liên quan |
| WC06 | Hủy/xác nhận xóa lớp; xác nhận dọn cả bộ và thẻ con |

Đây là native UI Automation gọi control của cửa sổ app đang chạy, không phải gọi trực tiếp ViewModel/service. Input Excel qua hộp chọn file hệ thống. JSON chỉ được **đọc** làm oracle; không seed hoặc sửa JSON để vượt qua flow.

### Android — 10/10 PASS

| ID | Kịch bản đã đạt trên emulator |
| --- | --- |
| AU01 | Package test riêng, giao diện English và Light |
| AU02 | Tạo lớp/bộ bằng tap và bàn phím Android |
| AU03 | DocumentsUI chọn Excel thật; nhận tiêu đề Nhật–Anh; hủy không ghi; xác nhận nhập đủ 13 cặp Unicode |
| AU04 | Chặn file toàn trùng/đảo cột, file có hàng thiếu dữ liệu và file sai cặp ngôn ngữ |
| AU05 | Chọn ghép từ cả hai phía, bỏ chọn, thay lựa chọn cùng phía, ghép sai rồi sửa |
| AU06 | Nhóm 6/6/1 không trùng; đưa app nền, dừng process, mở lại và resume; hết từ chỉ lặp khi chủ động bắt đầu vòng mới |
| AU07 | Đánh dấu ba từ; bộ lọc trả đúng 3/10/13 câu |
| AU08 | Viết Nhật → Anh đạt 12/13 theo chủ đích; retry chỉ một câu sai và trả lời đúng |
| AU09 | Trắc nghiệm Anh → Nhật đạt 13/13; số thẻ có sao vẫn bằng ba |
| AU10 | Lịch sử hiện kết quả; đổi Dark/Light/tiếng Việt; mở lại giữ cài đặt; cuối test trả về English/Light |

Android dùng thao tác tọa độ `adb input tap/swipe` trên UI thật. `AndroidExplicit/actions.jsonl` ghi thời gian, control, bounds và package mỗi lần tap; XML/PNG và `logcat.txt` nằm cùng thư mục. Đã xem ảnh thực tế có chữ Nhật hiển thị đúng; không chỉ kiểm tra chuỗi trong JSON.

## 3. Lỗi thực tế tìm được và đã sửa

**Back từ phiên ghép cặp được resume có thể hiện “Unable to continue”.**

- Reproduce: hoàn thành nhóm đầu → đóng/mở app → resume → hoàn thành các nhóm còn lại → bắt đầu vòng mới → Back.
- Stack trace ghi nhận `InvalidOperationException: PlatformView cannot be null here`, lúc Windows ngắt handler của button đang focus và áp lại visual state/viền.
- Sửa ở `MauiApp1/Controls/StudyButtonHandler.cs`: không map thay đổi thuộc tính sang native view khi handler đã mất PlatformView; đăng ký riêng Windows trong `MauiApp1/MauiProgram.cs`.
- Giữ nguyên focus visual cho bàn phím, không nuốt exception navigation hay bỏ toàn bộ style focus.
- Chạy lại WU09 và toàn bộ các lần Back trong hai suite Windows: PASS. Unit test và hai build Release cũng PASS.
- Evidence lỗi trước sửa: `MauiApp1/obj/UiAutomationEvidence/WindowsDiagnostic/failure.png` và `windows-full-exceptions.log` ở thư mục cha.

Một số lần chạy đầu còn lỗi **test driver**: selector của nút submit, SelectionPattern của picker, SearchBar bọc TextBox, chờ collection cập nhật và chờ cửa sổ file picker. Những lỗi này được sửa trong script, không tính là bug chức năng của app.

## 4. Môi trường và an toàn dữ liệu

- Working tree trên commit gốc `93baf61`; có thay đổi chưa commit của các yêu cầu trước và bản sửa handler trong đợt test này.
- Windows build OS `10.0.26200.0`, PowerShell `5.1.26100.9444`, .NET SDK `10.0.401`.
- Android emulator `emulator-5554`, Android 16 / API 36, 1080 × 2400 portrait.
- ApplicationId test: `com.vocabmate.uia20260928`; app thật: `com.vocabmate.app`.
- Windows UI dùng Release ở `MauiApp1/bin/UiAutomationWindows/`; có opt-in diagnostic module chỉ trong package test để lấy stack trace.
- Android UI dùng APK Debug tự chứa assemblies ở `MauiApp1/bin/UiAutomationAndroid/`, cho phép `run-as` đọc oracle. Build Android Release thường được xác nhận riêng, không đồng nghĩa đã UI-test APK Release trên thiết bị thật.
- Không xóa/ghi dữ liệu app thật. Hai suite Windows đều ghi `ProductionDataUnchanged: true`. SHA-256 JSON app thật trước/sau: `2A7023D8071CE8B56458A1575991CC0EA96CE337321FECDF819FE44650AECE3B`.
- Không sửa workbook người dùng đang mở trong `Samples/`. Các workbook test nằm trong `obj/UiAutomationEvidence/fixtures/`.

## 5. Chạy lại và xem demo

Xem hướng dẫn build/cài bản cô lập và câu lệnh đầy đủ trong `scripts/README.md`.

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/RunWindowsUiRegression.ps1
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/RunWindowsCrudRegression.ps1
$env:PYTHONUTF8 = '1'
python scripts/RunAndroidUiRegression.py
```

Chạy hai suite Windows lần lượt, không song song. Không thao tác tay lên app/emulator khi runner đang bấm. Windows suite học giữ lại lớp `UIA-*`, bộ `Japanese 13` để demo; CRUD xóa đúng lớp do chính suite tạo. App thật không dùng chung dữ liệu với bản này. Có thể thêm `-LeaveOpen` khi chỉ chạy suite học để giữ cửa sổ phục vụ demo; đóng cửa sổ đó trước khi chạy suite khác.

Ảnh minh họa đã chụp:

- `WindowsPassed/04-import-preview.png`: preview Excel Nhật–Anh.
- `WindowsPassed/08-round-complete.png`: hết vòng 13 từ.
- `WindowsPassed/10-all-thirteen.png`: tất cả 13 từ sau khi đã đánh dấu ba từ.
- `WindowsPassed/11-written-result.png`, `WindowsPassed/12-quiz-result.png`: kết quả viết/trắc nghiệm.
- `WindowsPassed/13-narrow-vietnamese.png`: giao diện sáng tiếng Việt, cửa sổ hẹp.
- `WindowsCrudWait/04b-use-all-cards.png`: nút Use all cards trong phiên học.
- `AndroidExplicit/06-final-one-no-repeat.png`: hoàn thành nhóm cuối trên Android.
- `AndroidExplicit/08-written-twelve-of-thirteen.png`, `AndroidExplicit/09-quiz-thirteen-of-thirteen.png`: kết quả chấm điểm trên Android.
- `AndroidExplicit/quiz-visual-check.png`: hiển thị cặp `train` / `電車` trong app thật.
- `AndroidExplicit/10-vietnamese-light.png`: giao diện Android sáng, tiếng Việt.

Các đường dẫn ảnh trên đều tính từ `MauiApp1/obj/UiAutomationEvidence/`.

## 6. Giới hạn — không tuyên bố đã test hết mọi tình huống

Chưa xác minh chất lượng âm thanh/phát âm thực tế; share file mẫu end-to-end với ứng dụng ngoài; thiết bị Android vật lý và các OS khác; TalkBack/Narrator; font hệ thống lớn và landscape; giới hạn 2.000 thẻ/10 MB qua UI; stress nhiều giờ, hết dung lượng và interruption bất thường.

Hiện giao diện không có nút xuất toàn bộ deck, nên không ghi nhận export deck UI là PASS. Service XLSX có unit test riêng. Ảnh sáng/tối chỉ xác nhận trạng thái giao diện, **không chứng minh ngồi học nhiều giờ sẽ không mỏi mắt**.

Không đổi toàn bộ trạng thái trong `TEST_CASES_DEMO.md` sang PASS: các kịch bản automation bao phủ nhiều chức năng nhưng không ánh xạ một-một với mọi test case thủ công.
