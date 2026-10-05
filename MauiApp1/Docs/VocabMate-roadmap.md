# VocabMate — checklist checkpoint

## Checkpoint 1: sản phẩm học được

- [x] Lớp local → bộ từ → thẻ Anh/Việt; tạo/sửa/xóa và xác nhận xóa liên đới.
- [x] Import `.xlsx` hai cột, nhận header đảo thứ tự, preview, báo dòng lỗi và bỏ duplicate.
- [x] Flashcards: lật đáp án, tự đánh giá nhớ/chưa nhớ.
- [x] Trắc nghiệm: bốn lựa chọn phân biệt, không dùng nghĩa đúng khác làm distractor.
- [x] Viết đáp án: chuẩn hóa Unicode/khoảng trắng/hoa thường, giữ dấu tiếng Việt.
- [x] Học hai chiều độc lập với ngôn ngữ giao diện.
- [x] Lưu bộ từ, lịch sử, câu cần ôn và phiên học offline.
- [x] Ôn lại câu sai/chưa nhớ từ kết quả.
- [x] Giao diện Anh/Việt, sáng/tối, giữ preference sau khởi động lại.
- [x] MVVM, Shell, DI, compiled bindings, data templates, resource dictionaries.
- [x] Custom flashcard có bindable properties và control template.
- [x] Behavior validation cho hai mặt từ và validation bảo vệ ở repository.
- [x] Debounce lưu nháp, khôi phục editor, session/input; lifecycle flush best effort.
- [x] Handler riêng `VocabularyEntry` trên Android, không ảnh hưởng `Entry` thường.

## Phần mở rộng đã đưa vào bản này

- [x] Game ghép cặp tối đa 6 cặp, lưu thứ tự/tiến độ và số lần ghép sai.
- [x] Xuất/chia sẻ bộ từ thành Excel và chia sẻ file mẫu.
- [x] Nút nghe tiếng Anh qua TextToSpeech của thiết bị.

## Kiểm thử/nghiệm thu cần phân biệt

- [x] Kiểm tra logic repository, engine, Excel bằng harness tạm, không phụ thuộc MAUI UI.
- [x] Test core chính thức bằng `MauiApp1.Tests`, chạy độc lập trên .NET 10.
- [x] Build Windows Debug, Windows Release SourceGen và Android Debug.
- [x] CI kiểm tra test core và build Release cho Android/Windows.
- [ ] Chạy đầy đủ trên Android emulator/device: picker, share, giọng đọc, handler, background/kill/resume.
- [ ] Kiểm tra TalkBack/Narrator, cỡ chữ lớn, tương phản và focus order.
- [ ] Kiểm tra workbook thực tế xuất từ nhiều phiên bản Excel/Google Sheets/LibreOffice.
- [ ] Kiểm tra giọng đọc khi thiết bị không có voice tiếng Anh, không có mạng hoặc không có ứng dụng nhận share.
- [ ] Application ID nền đã bỏ scaffold (`com.vocabmate.app`); trước phát hành vẫn cần thay bằng ID duy nhất và cấu hình signing/package theo tài khoản phát hành.
- [ ] Nếu cần iOS: cấu hình target/môi trường Mac/signing và chạy test; chưa tuyên bố hỗ trợ đã kiểm chứng.

## Kịch bản demo đề xuất

1. Tạo lớp `English every day`, bộ `Everyday vocabulary`.
2. Import `Samples/VocabMate-template.xlsx`; xem preview rồi xác nhận.
3. Import lại: thấy duplicate, không sinh thẻ trùng.
4. Thử file thiếu nghĩa/có công thức: preview báo lỗi, không cho commit một phần.
5. Tạo thẻ thủ công, gõ một mặt rồi thoát; mở lại để demo khôi phục nháp.
6. Demo cả ba chế độ học và đổi chiều Anh/Việt; cố ý trả lời sai một câu.
7. Xem kết quả, ôn lại chỉ từ sai.
8. Chơi ghép cặp, cố ý ghép sai rồi hoàn thành.
9. Gõ dở câu trả lời, thoát app và bấm Resume sau khi mở lại.
10. Đổi UI Anh/Việt và theme; nội dung bộ từ không thay đổi.
11. Xuất bộ từ ra Excel, mở lại qua preview để kiểm tra roundtrip.

## Ngoài scope hiện tại

Backend, tài khoản/lớp online, đồng bộ nhiều thiết bị, thi đấu nhiều người, AI chấm nghĩa gần đúng, spaced repetition, push notification và gamification dài hạn. Chỉ mở rộng khi sản phẩm local và bài kiểm thử nền tảng đã ổn định.
