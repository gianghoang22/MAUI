# VocabMate

Ứng dụng .NET MAUI học từ vựng Anh/Việt, lấy cảm hứng từ flashcards: **lớp local → bộ từ → thẻ → học và ôn lại**. Không cần tài khoản hoặc backend.

## Chạy ngay

1. Mở `MauiApp1.slnx` bằng Visual Studio có workload .NET MAUI.
2. Chọn project `MauiApp1`, target **Windows Machine** hoặc Android emulator/device.
3. Chạy Debug. App hiển thị tên **VocabMate**; giữ namespace/project cũ để không đổi cấu trúc không cần thiết.
4. Tạo lớp, tạo bộ từ, chọn **Import Excel** và dùng `Samples/VocabMate-template.xlsx`.
5. Kiểm tra preview, xác nhận nhập rồi chọn chế độ và chiều học.

Project hiện dùng **.NET 10**, target **Android + Windows**; chưa cấu hình/kiểm thử iOS.

`MauiApp1/PlatformConfiguration/AndroidManifest.xml` là manifest overlay được khai báo trong `.csproj`, bổ sung cấu hình phát hiện dịch vụ TextToSpeech vào manifest Android của scaffold.

```powershell
dotnet restore MauiApp1/MauiApp1.csproj
dotnet build MauiApp1/MauiApp1.csproj -f net10.0-windows10.0.19041.0
dotnet build MauiApp1/MauiApp1.csproj -f net10.0-android
dotnet build MauiApp1/MauiApp1.csproj -f net10.0-windows10.0.19041.0 -c Release
```

## Tính năng

- Tạo/sửa/xóa lớp, bộ từ, thẻ; tìm kiếm hai mặt; xác nhận xóa liên đới.
- Import `.xlsx`, tự nhận thứ tự cột Việt/Anh, xem trước lỗi và bỏ cặp trùng.
- Flashcards, trắc nghiệm, viết đáp án; chọn Anh → Việt hoặc Việt → Anh.
- Game ghép cặp, tính số lần sai và thời gian, lưu tiến độ.
- Kết quả, ôn lại câu chưa nhớ, tiếp tục phiên đang dở.
- Autosave bản nháp thẻ/câu đang gõ, khôi phục sau khi mở lại app.
- UI Anh/Việt, theme sáng/tối/hệ thống, giữ lựa chọn sau khi khởi động lại.
- File Excel mẫu và phát âm bằng giọng tiếng Anh của OS; đã bỏ nút xuất/chia sẻ bộ từ.
- Sao đánh dấu đã thuộc, học riêng nhóm chưa thuộc/đã thuộc/tất cả; tiếp tục ngay trong bộ từ hoặc sau mỗi lượt.
- Flashcard lật hai mặt bằng nhấn thẻ/nút Lật; đánh giá một lần rồi tự sang thẻ tiếp theo.
- Đổi chế độ, chiều học và nhóm từ ngay trong màn học. Giao diện baby blue, bề mặt kính trong nhẹ và animation lật thẻ.

## Windows: bố cục và bàn phím

- Thư viện, lớp, bộ từ và import dùng form bên trái, danh sách bên phải khi vùng trang rộng từ 900 DIP. Form chiếm khoảng 30% (280–400 DIP), danh sách dùng phần còn lại. Cửa sổ hẹp chuyển thành hai vùng trên/dưới cuộn độc lập; Android vẫn cuộn form cùng danh sách.
- `Tab` / `Shift+Tab`: di chuyển giữa các điều khiển; `Enter` / `Space`: kích hoạt nút đang focus. Game ghép cặp cũng dùng Tab rồi Enter để chọn từng vế.
- Ô tạo lớp/bộ từ: `Enter` để tạo. Sửa thẻ: `Enter` ở ô tiếng Việt chuyển sang tiếng Anh; `Enter` ở ô tiếng Anh lưu thẻ.
- Trắc nghiệm: `1`–`4` chọn theo thứ tự trái sang phải, trên xuống dưới; hỗ trợ cả bàn phím số. Flashcard ở cả hai mặt: `1` đã nhớ, `2` chưa nhớ.
- Luyện viết tự focus ô đáp án; nhập rồi `Enter` để kiểm tra, `Enter` tiếp ở nút Tiếp theo chuyển câu. Không bắt phím tắt khi đang gõ hoặc chọn trong picker.
- Các ô nhập giữ trạng thái focus khi xử lý command; tạm chỉ đọc thay vì vô hiệu hóa cả cây giao diện. Autosave và dữ liệu cũ không thay đổi.

## File Excel mẫu

| Tiếng Việt | English |
| --- | --- |
| quả táo | apple |
| con mèo | cat |
| quyển sách | book |
| ngôi nhà | house |

File thực tế ở `Samples/VocabMate-template.xlsx` có 6 cặp từ. Có thể dùng nút chia sẻ file mẫu trong app để lấy một mẫu khác gồm 4 cặp.

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
- Thời gian session tính cả thời gian tạm nghỉ. Giao diện kính là phong cách gradient/translucency, không phải hiệu ứng khúc xạ Liquid Glass native của Apple.
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

## Lưu ý trước khi đưa repo lên Git

`.gitignore` hiện bỏ qua `Docs/`, `MauiApp1/Resources/`, `MauiApp1/Platforms/`, `MauiApp1/Properties/`. Quy tắc này đang do người dùng chỉnh nên không bị thay đổi. Một clone thiếu platform bootstrap/tài nguyên template sẽ không build; cần rà lại các file scaffold và tài liệu trong `MauiApp1/Docs/` muốn đưa vào Git trước khi chia sẻ repo.
