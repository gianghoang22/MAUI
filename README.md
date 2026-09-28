# VocabMate

Ứng dụng .NET MAUI học từ vựng Anh/Việt, lấy cảm hứng từ flashcards: **lớp local → bộ từ → thẻ → học và ôn lại**. Không cần tài khoản hoặc backend.

## Kiểm thử và demo

- `TEST_CASES_DEMO.md`: dữ liệu mẫu, test case thủ công có mức ưu tiên/kết quả mong đợi, kịch bản demo và đề xuất mở rộng chức năng.

## Chạy ngay

1. Mở `MauiApp1.slnx` bằng Visual Studio có workload .NET MAUI.
2. Chọn project `MauiApp1`, target **Windows Machine** hoặc Android emulator/device.
3. Chạy Debug. App hiển thị tên **VocabMate**; giữ namespace/project cũ để không đổi cấu trúc không cần thiết.
4. Tạo lớp, tạo bộ từ, bấm **Import Excel** và dùng `Samples/VocabMate-template.xlsx`.
5. Trong màn nhập, bấm **Chọn file**, kiểm tra preview rồi bấm **Nhập**; tải file mẫu bằng nút riêng. Chọn chế độ, chiều học và bấm **Bắt đầu học** ngay trong bộ từ.

Project hiện dùng **.NET 10**, target **Android + Windows**; chưa cấu hình/kiểm thử iOS.

SDK được ghim trong `global.json`. Test core chính thức nằm ở `MauiApp1.Tests` và chạy được độc lập trên Windows/CI.

`MauiApp1/PlatformConfiguration/AndroidManifest.xml` là manifest overlay được khai báo trong `.csproj`, bổ sung cấu hình phát hiện dịch vụ TextToSpeech vào manifest Android của scaffold.

```powershell
dotnet restore MauiApp1/MauiApp1.csproj
dotnet build MauiApp1/MauiApp1.csproj -f net10.0-windows10.0.19041.0
dotnet build MauiApp1/MauiApp1.csproj -f net10.0-android
dotnet build MauiApp1/MauiApp1.csproj -f net10.0-windows10.0.19041.0 -c Release
dotnet test MauiApp1.Tests/MauiApp1.Tests.csproj -c Release
```

```start
$env:ANDROID_HOME = "$env:LOCALAPPDATA\Android\Sdk"
& "$env:ANDROID_HOME\emulator\emulator.exe" -avd MAUI_Emulator_API_36
dotnet run
```

## Tính năng

- Android và Windows mở lớp/bộ từ bằng một lần bấm trên card; nút **...** riêng trên card chỉ chứa sửa/xóa, không kích hoạt mở card. Home có nút **Tiếp tục học** dưới card thống kê khi có phiên đang dở, không còn menu cạnh tiêu đề.
- Bộ từ hiển thị trực tiếp nút thêm từ, import Excel, lưu, xóa, bắt đầu/tiếp tục học; sao đánh dấu đã thuộc nằm ngoài menu trên từng từ. Màn học không dùng menu action: nghe phát âm, đánh dấu đã thuộc, lật, đã nhớ/chưa nhớ, kiểm tra/tiếp theo, tùy chỉnh và học tiếp đều có nút riêng theo trạng thái. Các nhóm nút tự xuống dòng khi màn hình hẹp; xóa và thay phiên học vẫn cần xác nhận.

- Tạo/sửa/xóa lớp, bộ từ, thẻ; tìm kiếm hai mặt; xác nhận xóa liên đới.
- Import `.xlsx`, tự nhận thứ tự cột Việt/Anh, xem trước lỗi và bỏ cặp trùng.
- Flashcards, trắc nghiệm, viết đáp án; chọn Anh → Việt hoặc Việt → Anh.
- Game ghép cặp, tính số lần sai và thời gian, lưu tiến độ.
- Kết quả, ôn lại câu chưa nhớ, tiếp tục phiên đang dở.
- Autosave bản nháp thẻ/câu đang gõ, khôi phục sau khi mở lại app.
- UI Anh/Việt, theme sáng/tối/hệ thống, giữ lựa chọn sau khi khởi động lại.
- File Excel mẫu và phát âm bằng giọng tiếng Anh của OS; đã bỏ nút xuất/chia sẻ bộ từ.
- Sao đánh dấu đã thuộc, học riêng nhóm chưa thuộc/đã thuộc/tất cả; tiếp tục ngay trong bộ từ hoặc sau mỗi lượt.
- Flashcard lật hai mặt bằng nhấn thẻ hoặc nút **Lật thẻ**; bấm **Đã nhớ/Chưa nhớ** một lần rồi tự sang thẻ tiếp theo. Windows dùng phép chiếu 3D, Android dùng animation xoay thẻ; cả hai tôn trọng cài đặt tắt chuyển động của hệ thống.
- Đổi chế độ, chiều học và nhóm từ ngay trong màn học. Giao diện ngà / than olive, điểm nhấn đồng trầm, bề mặt màu đặc và viền mảnh; giữ animation lật thẻ phục vụ thao tác học.

## Theme và spacing

- Phong cách quiet luxury: nền ngà, chữ than olive, điểm nhấn đồng trầm; không gradient, kính mờ hay hiệu ứng trang trí. Theme tối dùng bề mặt than ấm; giữ lựa chọn sáng/tối/hệ thống đã lưu.
- Typography Open Sans có sẵn, phân cấp tiêu đề / nội dung / nhãn phụ nhất quán, hỗ trợ tiếng Việt và font scaling. Nút tối thiểu 48 DIP, ô nhập và picker tối thiểu 52 DIP; focus, hover, pressed và disabled có phản hồi riêng.
- Lề ngoài 20 / 28 / 40 DIP theo chiều rộng, thẻ bo 14 DIP, nút bo 10 DIP. Nội dung dashboard/workspace giới hạn 1360 DIP. Trên màn rộng, banner học và thống kê nằm cạnh nhau để đưa danh sách lên cao hơn.
- Palette và style dùng chung: `MauiApp1/Themes/StudyTheme.xaml`. Màu Android native: `MauiApp1/PlatformConfiguration/AndroidColors.xml`, được khai báo trong project thay cho màu mặc định của scaffold.
- Popup rộng tối đa 400 DIP (menu) / 460 DIP (form, xác nhận), padding 20–24 DIP. Nút tối thiểu 48 DIP trên cả hai nền tảng; nội dung dài cuộn riêng, nút Hủy nằm ngoài vùng cuộn. Xóa có màu cảnh báo và xác nhận riêng; giữ Escape, Back, Tab và Enter.
- Màn sửa thẻ có nút **Lưu thẻ** trực tiếp. Màn nhập có nút chọn file, lấy mẫu, xác nhận nhập; không buộc người dùng tìm thao tác chính trong menu.
- Quy tắc thiết kế và checklist kiểm tra: `DESIGN.md`. Test `DesignSystemTests` kiểm tra breakpoint, touch target và độ tương phản của các cặp màu chữ/bề mặt trong cả hai theme.

## Windows: bố cục và bàn phím

- Lớp, bộ từ và import dùng form bên trái (36%), danh sách bên phải (64%) khi vùng trang đạt ít nhất 1000 × 560 DIP. Cửa sổ hẹp hoặc thấp dùng một luồng cuộn chung, tránh form bị nhốt trong vùng cuộn nhỏ. Quy tắc này áp dụng cả Windows và Android tablet; điện thoại dùng một cột.
- `Tab` / `Shift+Tab`: di chuyển giữa các điều khiển; `Enter` / `Space`: kích hoạt nút đang focus. Game ghép cặp cũng dùng Tab rồi Enter để chọn từng vế.
- Ô tạo lớp/bộ từ: `Enter` để tạo. Sửa thẻ: `Enter` ở ô tiếng Việt chuyển sang tiếng Anh; `Enter` ở ô tiếng Anh lưu thẻ.
- Trắc nghiệm: `1`–`4` chọn theo thứ tự trái sang phải, trên xuống dưới; hỗ trợ cả bàn phím số. Flashcard ở cả hai mặt: `1` đã nhớ, `2` chưa nhớ.
- Luyện viết tự focus ô đáp án; nhập rồi `Enter` để kiểm tra, sau đó focus về nút **Tiếp theo**. Flashcard focus vào nút **Lật thẻ**. Không bắt phím tắt khi đang gõ, chọn trong picker hoặc khi đang xử lý xác nhận.
- Các ô nhập giữ trạng thái focus khi xử lý command; tạm chỉ đọc thay vì vô hiệu hóa cả cây giao diện. Autosave và dữ liệu cũ không thay đổi.

## File Excel mẫu

| Tiếng Việt | English |
| --- | --- |
| quả táo | apple |
| con mèo | cat |
| quyển sách | book |
| ngôi nhà | house |

File thực tế ở `Samples/VocabMate-template.xlsx` có 6 cặp từ. Có thể dùng thao tác chia sẻ file mẫu trong menu **...** của màn nhập để lấy một mẫu khác gồm 4 cặp.

- Hai cột A/B, hàng tiêu đề `Tiếng Việt`/`Vietnamese` và `Tiếng Anh`/`English`; đảo thứ tự cột được.
- Chỉ đọc sheet hiển thị đầu tiên. Tối đa 2.000 dòng dữ liệu, file 10 MB và giới hạn giải nén an toàn.
- Không hỗ trợ `.xls`, CSV, file mã hóa hoặc công thức; nên lưu các ô dạng Text.
- Dòng thiếu mặt/quá dài/có công thức phải được sửa trước khi commit. Dòng trùng được bỏ qua, không ghi đè từ cũ.
- Import và lấy file mẫu vẫn hoạt động; không còn nút xuất bộ từ trong giao diện.

## Quy tắc học cần biết

- Tối đa 20 câu/lượt, ghép cặp tối đa 6 cặp. Nhóm **Chưa thuộc** bỏ qua các từ có sao; từ chưa nhớ vẫn có thể lặp để luyện tiếp.
- Cùng một prompt có nhiều nghĩa sẽ được gom thành một câu chấp nhận các nghĩa đã khai báo trong bộ từ.
- Trắc nghiệm cần 3 distractor phân biệt cho từng câu; bộ quá ít từ nên dùng Flashcards hoặc Viết đáp án.
- Chấm viết bỏ qua hoa/thường, khoảng trắng thừa và khác biệt Unicode NFC/NFD, **không bỏ dấu tiếng Việt hoặc tự đoán từ đồng nghĩa**.
- Flashcards là tự đánh giá: **Đã nhớ** đánh dấu sao, **Chưa nhớ** bỏ sao; cả hai lưu tiến độ và tự chuyển câu trong cùng lần ghi. Trắc nghiệm/viết không tự đánh dấu đã thuộc.
- **Tiếp tục phiên** giữ đúng câu/mặt/chế độ/chiều/input cũ. **Tiếp tục học** sau kết quả tạo lượt mới theo bộ lọc hiện hành, không lặp từ đã thuộc khi chọn Chưa thuộc.
- Sao có thể bật/tắt trong danh sách hoặc trên thẻ. Đổi chế độ trong Tùy chỉnh bắt đầu phiên mới có xác nhận nếu đang học dở; không xóa sao.
- Thời gian session tính cả thời gian tạm nghỉ. Đã bỏ toàn bộ gradient, translucency và shadow của theme kính cũ.
- Chỉ giữ một session đang dở. Bắt đầu session mới có xác nhận thay thế; giữ tối đa 100 kết quả đã hoàn thành.

## Dữ liệu và an toàn

Dữ liệu mới nằm trong `vocabmate.json` tại thư mục riêng của app; xem đường dẫn ở **Cài đặt**. `studymate.json` cũ được giữ nguyên, không migrate/xóa. Preference mới dùng prefix `vocabmate.*`, có đọc fallback ngôn ngữ/theme cũ ở lần đầu.

Nháp thẻ được lưu sau khoảng 400 ms ngừng gõ và flush khi rời trang/background. Nhấn **Lưu từ vào bộ** mới thay thẻ chính; Back giữ nháp, **Bỏ bản nháp** xóa phần chỉnh sửa. Lifecycle là best effort: force-kill quá sớm hoặc lỗi ghi vẫn có thể mất phần chưa persist.

JSON chưa mã hóa, không hỗ trợ nhiều tiến trình ghi chung file hoặc đồng bộ nhiều thiết bị. Không lưu bí mật. Gỡ app có thể làm mất dữ liệu. JSON hỏng không bị tự ghi đè bằng dữ liệu rỗng; sao lưu file trước khi khôi phục thủ công.

## Tài liệu học

- `MauiApp1/Docs/VocabMate-architecture.md`: kiến trúc, flow, validation, lưu nháp/lifecycle, handler và localization; đã cập nhật sao và responsive.
- `MauiApp1/Docs/Fix-implementation.md`: đối chiếu từng yêu cầu Fix.md, quy tắc mới và giới hạn giao diện/kiểm thử.
- `MauiApp1/Docs/VocabMate-roadmap.md`: roadmap và checklist checkpoint.

## Kiểm tra ngày 17/09/2026

- Bản sửa Fix.md build thành công Windows Release, Windows Debug (output riêng) và Android Debug, không warning/error. Output Debug mặc định bị khóa nếu bản app cũ đang mở: đóng app trước khi F5/build lại.
- 66 kiểm tra logic bằng harness tạm (bổ sung sao, bộ lọc, tiếp tục, tương thích JSON cũ): CRUD/cascade, chống duplicate, batch atomic, nháp, grading hai chiều, nghĩa thay thế, phiên/lịch sử, Excel inline/shared strings, đảo cột, lỗi/công thức/giới hạn ZIP/XML và dữ liệu hỏng đều pass.
- UI Automation Windows Release kiểm tra CRUD/nháp, bốn chế độ, lật hai lần, tự chuyển câu, tiếp tục bỏ qua từ có sao, đổi mode ngay trong trang, sao thủ công, resume mặt/chiều/input, history rỗng/có dữ liệu và theme/ngôn ngữ. Kiểm tra bounds ở 390/640/950/1440 pixel không thấy text giao nhau trong các màn đã chạy. Dữ liệu và preference kiểm thử được tách khỏi app người dùng; lớp test đã được dọn.
- Harness tạm nằm trong `MauiApp1/obj/`, không phải test suite được đưa vào Git. Chưa chạy thực tế trên Android hoặc kiểm thử UI picker/share/voice và screen reader.

## Kiểm tra duy trì

- `MauiApp1.Tests` là test project chính thức cho repository, search normalization, learning filter/session và XLSX round-trip.
- `.github/workflows/validate.yml` chạy test core và build Release cho Android/Windows trên mỗi push/pull request.
- Application ID nền hiện là `com.vocabmate.app`; trước khi phát hành cần đổi thành định danh duy nhất do sản phẩm sở hữu và cấu hình certificate/signing ngoài repo.
- Checklist đóng gói Android/Windows nằm ở `Docs/Release-Android-Windows.md`.

## Lưu ý trước khi đưa repo lên Git

`.gitignore` hiện bỏ qua `Docs/`, `MauiApp1/Resources/`, `MauiApp1/Platforms/`, `MauiApp1/Properties/`. Quy tắc này đang do người dùng chỉnh nên không bị thay đổi. Một clone thiếu platform bootstrap/tài nguyên template sẽ không build; cần rà lại các file scaffold và tài liệu trong `MauiApp1/Docs/` muốn đưa vào Git trước khi chia sẻ repo.
