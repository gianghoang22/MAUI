# VocabMate — Test case chức năng, kịch bản demo và hướng mở rộng

> Đối chiếu mã nguồn ngày **28/09/2026**. Đây là kế hoạch kiểm thử thủ công, **không phải báo cáo đã chạy test**. Mọi case bắt đầu ở trạng thái `NT`.

## 1. Cách sử dụng

- Phạm vi: Windows và Android; ứng dụng offline, lớp → bộ từ → thẻ → phiên học → lịch sử.
- Dùng mục 4 để test, mục 5 để demo 12–15 phút, mục 7 để thảo luận mở rộng.
- `P0`: cốt lõi/an toàn dữ liệu/chặn demo. `P1`: chức năng quan trọng. `P2`: biên hoặc chuyên sâu.
- `NT`: chưa chạy; `PASS`: đạt; `FAIL`: không đạt; `BLOCKED`: thiếu điều kiện. Ghi riêng nền tảng, ví dụ `W: PASS / A: NT`.
- Tên nút có thể thay đổi theo ngôn ngữ. Kiểm tra hành vi, không yêu cầu thông báo trùng từng chữ.
- Câu hỏi được xáo trộn: đối chiếu nội dung, không mặc định câu đầu là `apple`.

### Phiếu một lần chạy

| Trường | Giá trị cần điền |
| --- | --- |
| Người test / ngày chạy | ... |
| Commit hoặc bản build / Debug hay Release | ... |
| Thiết bị / OS / Windows hay Android | ... |
| Ngôn ngữ / theme / cỡ chữ hệ thống | ... |
| Bộ dữ liệu / trạng thái sao ban đầu | ... |
| Tổng đã chạy / PASS / FAIL / BLOCKED / NT | ... |
| Lỗi chặn demo | ... |

## 2. Chuẩn bị và an toàn

1. Dùng emulator, tài khoản OS thử nghiệm hoặc dữ liệu có tiền tố `DEMO-`; không thử xóa trên dữ liệu thật.
2. Nếu có dữ liệu cần giữ: đóng app, sao lưu thủ công `vocabmate.json`; đường dẫn nằm trong Cài đặt. Không xóa toàn bộ dữ liệu để làm trống màn demo.
3. Chạy cùng bản build trên Windows và Android, ghi kết quả riêng. Môi trường mới dùng để kiểm tra trạng thái rỗng.
4. Chuẩn bị bàn phím/chuột trên Windows; bàn phím mềm, Back hệ thống và xoay màn hình trên Android.
5. Kiểm tra trước giọng tiếng Anh và âm lượng OS. Phát âm phụ thuộc thiết bị, không phải dịch vụ riêng của app.
6. Demo dùng theme Sáng; kiểm tra thêm Tối/Hệ thống ở nhóm cài đặt.
7. Autosave: chờ ít nhất **1 giây** sau khi ngừng gõ, rồi Back hoặc đưa app về nền trước khi đóng/mở. Force-kill tức thì không phải trường hợp app đảm bảo lưu.
8. Mỗi case có tiền điều kiện độc lập. Khôi phục bộ mẫu hoặc tạo lại thủ công nếu case trước đã sửa/xóa/đánh sao. App chưa có nút nhân bản bộ từ.
9. Các workbook trong mục 3 là **fixture người test cần chuẩn bị**, chưa được tạo bởi tài liệu này. File mẫu có sẵn: `Samples/VocabMate-template.xlsx`.

## 3. Dữ liệu kiểm thử

### D01 — Bộ cơ bản, 8 thẻ

Lớp `DEMO-English A1`, bộ `Everyday Basics`, mô tả `Từ vựng dùng cho kiểm thử và demo`. Ban đầu tất cả **chưa có sao**.

| Tiếng Việt | Tiếng Anh |
| --- | --- |
| quả táo | apple |
| con mèo | cat |
| quyển sách | book |
| ngôi nhà | house |
| nước | water |
| mặt trời | sun |
| người bạn | friend |
| đi học | go to school |

- Trạng thái lọc chuẩn: chỉ đánh sao `apple`, `cat` → đã thuộc 2, chưa thuộc 6, tất cả 8.
- Đáp án sai có chủ đích: `zzz-demo-wrong`.
- Không mặc định `táo` được chấm đúng thay cho `quả táo`; phải khai báo đáp án được chấp nhận.

### Các bộ bổ sung

| Mã | Chuẩn bị | Mục đích |
| --- | --- | --- |
| D02 | `DEMO-Small`: 3 cặp apple/cat/book từ D01; `DEMO-One`: chỉ apple | Thiếu đáp án nhiễu hoặc thiếu cặp ghép |
| D03 | `DEMO-Multiple meanings`: `ngân hàng/bank`, `bờ sông/bank`, `quả táo/apple`, `con mèo/cat`, `quyển sách/book` | Cùng câu hỏi có nhiều đáp án hợp lệ |
| D04 | 25 thẻ duy nhất: `từ 01/word01` đến `từ 25/word25`, chưa sao | Giới hạn số câu/cặp mỗi lượt |

D03 chiều Anh → Việt: `bank` là một câu hỏi, chấp nhận hai nghĩa đã khai báo ở **hai thẻ riêng**. Không nhập dấu `;` trong một ô rồi coi là cú pháp tách nghĩa.

### Workbook

Chỉ hai cột A/B; dòng đầu là tiêu đề; lưu `.xlsx` thật, không đổi đuôi CSV.

| Fixture | Nội dung | Kỳ vọng |
| --- | --- | --- |
| X01-valid.xlsx | `Tiếng Việt`, `Tiếng Anh`; 8 dòng D01 | Bộ trống: 8 hợp lệ/0 trùng/0 lỗi |
| X02-reversed.xlsx | `English`, `Vietnamese`; đảo cả hai cột dữ liệu D01 | Không đảo nhầm mặt sau import |
| X03-duplicates.xlsx | `quả táo/apple`, `quyển sách/book`, `trường học/school`, `cái cây/tree`, `cái cây/tree` | Bộ có D01: 2 mới/3 trùng/0 lỗi |
| X04-invalid.xlsx | `quả táo/apple`, `con mèo/[ô trống]`, `quyển sách/book` | 1 lỗi; chặn cả lô |
| X05-formula.xlsx | Một ô dữ liệu dùng công thức, ví dụ `="apple"` | Không hỗ trợ công thức |
| X06-extra-column.xlsx | Hai cột hợp lệ, thêm `ghi chú` ở C2 | Lỗi cấu trúc hai cột |
| X07-empty.xlsx | Chỉ tiêu đề hợp lệ | Không có dữ liệu |
| X08-bad-header.xlsx | Tiêu đề `Từ`, `Nghĩa`; nội dung D01 | Tiêu đề không hợp lệ |
| X09-blank-rows.xlsx | D01 xen hai dòng hoàn toàn trống | Bỏ qua dòng trống, vẫn có 8 thẻ |
| X10-multi-sheet.xlsx | Sheet hiển thị đầu có 2 thẻ; sheet hiển thị sau có 8 thẻ khác | Chỉ đọc sheet hiển thị đầu tiên |

Tiêu đề hỗ trợ: `Tiếng Việt`/`Vietnamese`/`VI` và `Tiếng Anh`/`English`/`EN`, không phân biệt hoa thường, có thể đảo thứ tự cột.

## 4. Test case chức năng

Trong cột thao tác, thực hiện theo thứ tự đánh số. `KQ` để người test cập nhật.

### 4.1. Khởi động và điều hướng

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| APP-01 | P0 | Môi trường chưa có dữ liệu | 1. Mở app. 2. Xem Thư viện/Lịch sử/Cài đặt. | Không crash; trạng thái rỗng rõ; tổng lớp/bộ/thẻ bằng 0; không có phiên để tiếp tục. | NT |
| APP-02 | P0 | D01 | 1. Bấm một lần vào lớp, rồi bộ. 2. Back hai lần. | Đúng trang/nội dung; không cần double-click; trở về đúng cấp cha. | NT |
| APP-03 | P1 | Có lớp/bộ | 1. Bấm `...` trên card. 2. Hủy menu. | Chỉ mở menu, không đồng thời vào card; hủy không đổi dữ liệu. | NT |
| APP-04 | P1 | Chỉ một lớp chứa D01 | 1. Xem thống kê. 2. Thêm 1 thẻ, quay về Thư viện. | Ban đầu 1 lớp/1 bộ/8 thẻ; sau thêm 9 thẻ; tìm kiếm không làm sai tổng dữ liệu. | NT |

### 4.2. Lớp và bộ từ

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| CRUD-01 | P0 | Chưa có lớp mẫu | 1. Tạo `DEMO-English A1`. 2. Mở lớp. 3. Đóng/mở app. | Lớp tạo một lần, tồn tại sau mở lại. | NT |
| CRUD-02 | P1 | Form tên lớp/bộ | 1. Để trống hoặc toàn dấu cách. 2. Thêm/lưu nếu nút khả dụng. | Không lưu tên rỗng; nút không khả dụng hoặc phản hồi validation. | NT |
| CRUD-03 | P1 | Có `DEMO-English A1` | 1. Tạo ` demo-english   a1 `. 2. Đổi lớp khác thành tên này. | Chặn trùng sau chuẩn hóa hoa thường/khoảng trắng; dữ liệu cũ còn nguyên. | NT |
| CRUD-04 | P1 | Form tên lớp/bộ | 1. Thử 80 ký tự ASCII. 2. Dán 81 ký tự. | 80 hợp lệ; UI hoặc validation ngăn lưu quá 80; không crash. | NT |
| CRUD-05 | P0 | Lớp chứa D01 | 1. Đổi tên lớp. 2. Lưu. 3. Xem lại Thư viện. | Tên mới hiển thị, bộ/thẻ không mất hoặc đổi lớp. | NT |
| CRUD-06 | P0 | Có lớp | 1. Tạo bộ `Everyday Basics`. 2. Mở thông tin bộ, thêm mô tả. 3. Lưu/mở lại. | Tên/mô tả lưu đúng; bộ mới chưa có thẻ. | NT |
| CRUD-07 | P1 | Có hai lớp | 1. Tạo hai bộ cùng tên trong một lớp. 2. Tạo tên đó ở lớp khác. | Chặn trùng cùng lớp; cho phép cùng tên khác lớp. | NT |
| CRUD-08 | P1 | Có bộ | 1. Đổi tên/mô tả. 2. Thử mô tả 500/501 ký tự ASCII. 3. Lưu. | Lưu giá trị hợp lệ, ngăn quá 500; không mất thẻ. | NT |
| CRUD-09 | P0 | Bộ thử có thẻ, nháp, kết quả, phiên dở; có bộ đối chứng | 1. Xóa bộ rồi Hủy. 2. Xóa lại, xác nhận. | Hủy giữ dữ liệu; xác nhận xóa bộ và thẻ/nháp/kết quả/phiên liên quan, không ảnh hưởng bộ đối chứng. | NT |
| CRUD-10 | P0 | Lớp thử có 2 bộ; lớp đối chứng | 1. Xóa lớp rồi Hủy. 2. Xóa lại, xác nhận. | Hủy không đổi; xác nhận xóa lớp cùng dữ liệu các bộ con, không ảnh hưởng lớp đối chứng. | NT |

### 4.3. Thẻ, tìm kiếm, sao và nháp

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| CARD-01 | P0 | Bộ trống | 1. Thêm `quả táo/apple`. 2. Lưu từ vào bộ. 3. Mở lại. | Hai mặt đúng, tăng 1 thẻ; nháp của lần thêm đã được xử lý. | NT |
| CARD-02 | P1 | Form thẻ | 1. Bỏ trống từng mặt/toàn dấu cách. 2. Thử 200/201 ký tự ASCII mỗi mặt. | Mỗi mặt phải có nội dung, tối đa 200; dữ liệu sai không cập nhật thẻ chính. | NT |
| CARD-03 | P1 | D01 | 1. Thêm `  QUẢ  TÁO / APPLE  `. 2. Thêm `trái táo/apple`. | Chặn cặp trùng chuẩn hóa; cho phép mặt Anh giống nhưng nghĩa Việt khác. | NT |
| CARD-04 | P0 | D01, apple có sao | 1. Sửa nghĩa Việt của apple. 2. Lưu, mở lại. | Nội dung cập nhật; số thẻ không tăng; giữ sao. | NT |
| CARD-05 | P0 | Thẻ thử có nháp | 1. Xóa rồi Hủy. 2. Xóa lại, xác nhận. | Hủy giữ thẻ; xác nhận xóa thẻ/nháp, không xóa thẻ khác. | NT |
| CARD-06 | P0 | D01 | 1. Tìm `APPLE`, rồi `qua tao`, rồi `zzzz`. 2. Xóa tìm kiếm. | Tìm theo cả hai mặt, không phân biệt hoa thường/dấu; không kết quả có thông báo; xóa lọc lại đủ 8. | NT |
| CARD-07 | P1 | Lớp `DEMO-Tiếng Anh`, bộ `Đồ vật` | 1. Tìm lớp `tieng anh`. 2. Tìm bộ `do vat`. 3. Xóa tìm kiếm. | Tìm không dấu đúng; xóa phục hồi danh sách. | NT |
| CARD-08 | P0 | D01 chưa sao | 1. Sao apple/cat. 2. Lọc Đã thuộc/Chưa thuộc/Tất cả. 3. Tìm apple trong Chưa thuộc. | Lần lượt 2/6/8; tìm kết hợp bộ lọc cho 0; sao còn khi mở lại app. | NT |
| CARD-09 | P0 | D01 | 1. Thêm thẻ, gõ hai mặt chưa Lưu. 2. Chờ 1 giây, Back. 3. Thêm thẻ lại. | Khôi phục nháp mới; danh sách chính chưa tăng; Lưu mới tạo thẻ. | NT |
| CARD-10 | P0 | D01 | 1. Sửa thẻ chưa lưu. 2. Chờ 1 giây, đưa app nền rồi đóng/mở lại. 3. Mở thẻ. | Phục hồi nháp; thẻ chính giữ bản cũ tới lúc Lưu. | NT |
| CARD-11 | P0 | Thẻ có nháp sửa | 1. Bỏ bản nháp rồi Hủy. 2. Bỏ lại, xác nhận. 3. Mở thẻ. | Hủy giữ nháp; xác nhận trở về nội dung đã lưu, không xóa thẻ. | NT |

### 4.4. Import Excel

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| IMP-01 | P0 | Bộ trống; X01 | 1. Chọn X01. 2. Xem preview. 3. Nhập và xác nhận. | 8 hợp lệ/0 trùng/0 lỗi; sau xác nhận đúng 8 thẻ. | NT |
| IMP-02 | P0 | Bộ trống; X01 | 1. Chọn file. 2. Nhập nhưng hủy xác nhận. 3. Back. | Chưa thêm thẻ; xem preview không tự ghi dữ liệu. | NT |
| IMP-03 | P1 | Màn import | 1. Mở chọn file. 2. Hủy picker. | Không crash/nhập dữ liệu; preview cũ được xóa khi bắt đầu lần chọn mới. | NT |
| IMP-04 | P0 | Bộ trống; X02 | 1. Nhập file đảo cột. 2. Mở thẻ, học Anh → Việt. | Đúng mặt: apple là Anh, quả táo là Việt. | NT |
| IMP-05 | P0 | D01; X03 | 1. Chọn file, xem thống kê. 2. Xác nhận nhập. | 2 mới/3 trùng/0 lỗi; thêm school/tree một lần; tổng 10 thẻ. | NT |
| IMP-06 | P1 | D01; X01 | 1. Chọn lại X01. 2. Thử Nhập. | 0 mới/8 trùng/0 lỗi; không cho nhập lô không có thẻ mới; tổng vẫn 8. | NT |
| IMP-07 | P0 | Bộ trống; X04 | 1. Chọn file, xem dòng lỗi. 2. Thử Nhập. 3. Sửa file ngoài app và chọn lại. | Chỉ rõ dòng lỗi; **chặn cả lô** khi còn lỗi; sửa xong/chọn lại mới nhập được. | NT |
| IMP-08 | P1 | X05/X06/X08 | 1. Chọn lần lượt công thức/cột thừa/sai tiêu đề. 2. Kiểm tra bộ. | Lỗi phù hợp; không ghi thẻ từ file không hợp lệ. | NT |
| IMP-09 | P1 | Bộ trống; X07/X09 | 1. Chọn file chỉ tiêu đề. 2. Chọn file xen hàng trống. | X07 bị từ chối; X09 bỏ hàng trống và còn 8 thẻ hợp lệ. | NT |
| IMP-10 | P1 | Bộ trống; X10 | 1. Chọn file, xem tên sheet/preview. 2. Nhập. | Chỉ sheet hiển thị đầu được đọc; không gộp mọi sheet. | NT |
| IMP-11 | P1 | OS có ứng dụng nhận file | 1. Bấm File Excel mẫu. 2. Lưu/chia sẻ qua OS. 3. Mở file nhận. | File xlsx hai cột, 4 cặp mẫu apple/cat/book/house; hủy chia sẻ không đổi bộ. | NT |
| IMP-12 | P2 | Fixture riêng: >10 MiB, >2001 hàng XML kể cả tiêu đề, file hỏng/CSV đổi đuôi xlsx | 1. Chọn từng file. 2. Đóng lỗi. 3. Kiểm tra bộ. | Từ chối vượt giới hạn/không hợp lệ; không crash/ghi một phần. Nếu picker ẩn file, ghi chưa kiểm thử parser, không coi là PASS parser. | NT |

### 4.5. Thiết lập và phiên học

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| SES-01 | P0 | D01, sao apple/cat | 1. Chọn Flashcards. 2. Tạo lượt riêng với Đã thuộc/Chưa thuộc/Tất cả. | Lần lượt 2/6/8 câu, chỉ đúng nhóm từ. | NT |
| SES-02 | P0 | Bộ trống; hoặc D01 chưa sao, lọc Đã thuộc | 1. Thử bắt đầu. 2. Đổi Tất cả khi có thẻ. | Không tạo phiên rỗng; phản hồi hoặc nút không khả dụng; nhóm hợp lệ học được. | NT |
| SES-03 | P0 | D01 | 1. Học Anh → Việt. 2. Tạo lượt Việt → Anh. | Đổi đúng mặt hỏi/đáp; không tự dịch hoặc sửa nội dung thẻ. | NT |
| SES-04 | P0 | Có phiên dở | 1. Từ bộ khác hoặc Tùy chỉnh, bắt đầu phiên mới. 2. Hủy. 3. Thử lại, đồng ý. | Hủy giữ phiên cũ; đồng ý thay phiên; chỉ một phiên dở, không xóa sao. | NT |
| SES-05 | P0 | Flashcards D01 đang dở | 1. Lật thẻ, ghi câu/mặt/chỉ số. 2. Thoát. 3. Tiếp tục từ Thư viện. 4. Thử lại từ bộ đó. | Giữ câu, mặt, mode, chiều và tiến độ; resume không xáo lại phiên. | NT |
| SES-06 | P0 | Viết đáp án đang dở | 1. Gõ một phần, chưa kiểm tra. 2. Chờ 1 giây, nền rồi đóng/mở app. 3. Tiếp tục. | Phục hồi câu/input, không tự chấm hoặc chuyển câu. | NT |
| SES-07 | P1 | Phiên đang dở | 1. Mở Tùy chỉnh. 2. Đổi lựa chọn rồi Hủy. 3. Học tiếp. | Phiên/cấu hình thực tế không đổi, không mất input/tiến độ. | NT |
| SES-08 | P1 | D04, Tất cả | 1. Tạo lượt Flashcards/Trắc nghiệm/Viết riêng. 2. Tạo lượt Ghép cặp. | Lượt thường tối đa 20 câu, ghép tối đa 6 cặp; không lặp prompt chuẩn hóa trong lượt. | NT |

### 4.6. Flashcards và phát âm

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| FLA-01 | P0 | Flashcards D01 | 1. Chạm thẻ để lật. 2. Bấm Lật thẻ để lật lại. | Đúng hai mặt; lật không tăng chỉ số/tự chấm. | NT |
| FLA-02 | P0 | Flashcards D01 | 1. Ghi từ hiện tại, bấm Đã nhớ. 2. Từ tiếp theo bấm Chưa nhớ. 3. Xem sao trong bộ. | Mỗi lần tự chuyển đúng một câu; Đã nhớ bật sao, Chưa nhớ bỏ sao; kết quả theo tự đánh giá. | NT |
| FLA-03 | P0 | Flashcards, Chưa thuộc | 1. Đánh Đã nhớ mọi câu. 2. Xem kết quả. 3. Tiếp tục nếu còn từ đủ điều kiện. | Lượt mới không lấy thẻ đã sao; hết nhóm chưa thuộc thì báo hết/không cho tiếp tục nhóm rỗng. | NT |
| FLA-04 | P1 | Giọng Anh OS sẵn sàng, có âm lượng | 1. Nghe ở từng chiều học. 2. Lật thẻ và nghe lại. | Luôn đọc phần tiếng Anh của câu tương ứng. | NT |
| FLA-05 | P2 | Thiết bị thử riêng không có giọng Anh | 1. Bấm nghe. 2. Đóng phản hồi, học tiếp. | Thông báo không có giọng; vẫn học được, không mất tiến độ. | NT |

### 4.7. Trắc nghiệm

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| MC-01 | P0 | D01, Trắc nghiệm, Tất cả | 1. Chọn đúng. 2. Tiếp theo. 3. Chọn sai. | Mỗi câu 4 lựa chọn phân biệt; phản hồi đúng/sai chính xác; phải Tiếp theo sau chọn, không chấm lại cùng câu. | NT |
| MC-02 | P1 | D02 | 1. Bắt đầu Trắc nghiệm. 2. Đổi Flashcards hoặc Viết. | Báo thiếu 3 đáp án nhiễu phân biệt; mode không cần nhiễu vẫn chạy. | NT |
| MC-03 | P1 | D03, Anh → Việt, Tất cả | 1. Tới bank, xem lựa chọn. 2. Chọn nghĩa đúng xuất hiện. | Nghĩa đúng khác không được làm đáp án nhiễu; chấm đúng; không tự đổi sao. | NT |

### 4.8. Viết đáp án

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| WRI-01 | P0 | D01, Viết, Việt → Anh | 1. Tới đi học. 2. Nhập `  GO   TO SCHOOL  `. 3. Kiểm tra. | Đúng dù khác hoa/thường/khoảng trắng; không tự đánh sao. | NT |
| WRI-02 | P0 | D01, Viết | 1. Kiểm tra input trống/toàn cách. 2. Nhập zzz-demo-wrong, kiểm tra. | Rỗng không chấm; sai có phản hồi; chỉ sang câu khi Tiếp theo. | NT |
| WRI-03 | P1 | D01, Viết, Anh → Việt | 1. Với apple nhập qua tao. 2. Lượt riêng nhập quả táo. | Không dấu sai, có dấu đúng; quy tắc chấm khác tìm kiếm không dấu. | NT |
| WRI-04 | P1 | D03, Viết, Anh → Việt | 1. Với bank nhập ngân hàng. 2. Lượt riêng nhập bờ sông. 3. Thử nghĩa chưa khai báo. | Chấp nhận cả hai nghĩa đã khai báo, không tự chấp nhận đồng nghĩa mới. | NT |
| WRI-05 | P2 | Người test chuẩn bị quả táo ở Unicode NFC/NFD | 1. Lưu thẻ NFC. 2. Dán đáp án NFD khi hỏi apple, kiểm tra. | Chấm đúng sau chuẩn hóa Unicode, không mất dấu. | NT |

### 4.9. Ghép cặp

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| MAT-01 | P0 | D01, Ghép cặp, Tất cả | 1. Chọn trái rồi phải đúng. 2. Hoàn tất các cặp. | Tăng tiến độ; cặp đã ghép không ghép lại; xong có kết quả/thời gian/số lần sai. | NT |
| MAT-02 | P0 | Ghép cặp đang dở | 1. Ghép sai một lần. 2. Xem đếm sai. 3. Ghép lại đúng. | Sai tăng đúng 1, chưa hoàn tất cặp; vẫn sửa được ở lần ghép sau. | NT |
| MAT-03 | P0 | Đã ghép đúng 1 cặp và sai 1 lần | 1. Thoát/nền sau thao tác hoàn tất. 2. Mở lại, tiếp tục. | Giữ cặp đã ghép, thứ tự cột phải, đếm sai; không yêu cầu giữ ô trái chọn dở chưa ghép. | NT |
| MAT-04 | P1 | DEMO-One hoặc không đủ 2 cặp phân biệt | 1. Bắt đầu Ghép cặp. | Báo không đủ cặp, không tạo game rỗng; đổi mode khác được. | NT |

### 4.10. Kết quả, ôn sai và lịch sử

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| RES-01 | P0 | D01, Viết, Tất cả | 1. Đúng 6 câu, sai 2 câu đã ghi lại. 2. Hoàn tất. 3. Mở Lịch sử. | 6/8, đúng 2 câu sai, đúng bộ/mode/chiều; một kết quả cho phiên. | NT |
| RES-02 | P0 | Kết quả RES-01 | 1. Ôn lại câu sai tại kết quả. 2. Hoàn tất. | Chỉ 2 câu sai, không 6 câu đúng; giữ mode/chiều gốc; kết quả lượt ôn riêng. | NT |
| RES-03 | P1 | Lịch sử có lượt sai, đang có phiên khác dở | 1. Mở `...` trên kết quả, ôn sai. 2. Hủy thay phiên. 3. Thử lại và đồng ý. | Hủy giữ phiên hiện tại; đồng ý bắt đầu lượt ôn từ kết quả đã chọn. | NT |
| RES-04 | P1 | Lượt thường đúng hết; lượt Ghép cặp có lần ghép sai | 1. Xem kết quả/menu lịch sử. | Lượt thường đúng hết không có ôn sai. Ghép cặp hiện không tạo WrongQuestions dù có đếm sai; thiếu nút ôn sai ở đây không phải lỗi. | NT |
| RES-05 | P1 | Có ít nhất 2 lượt hoàn tất | 1. Xem Lịch sử. 2. Rời/quay lại kết quả, đóng/mở app. | Kết quả mới trước cũ; không nhân đôi kết quả khi mở lại. | NT |
| RES-06 | P2 | Phiên dở | 1. Học một câu. 2. Nghỉ 30 giây. 3. Hoàn tất. | Thời gian kết quả bao gồm lúc nghỉ, không chỉ thời gian thao tác. | NT |
| RES-07 | P2 | Môi trường QA riêng | 1. Hoàn tất 101 lượt ngắn, ghi thứ tự. 2. Kiểm tra bản sao chỉ đọc nếu UI khó đếm. | Tối đa 100 kết quả mới nhất. Không cần thực hiện trong demo. | NT |

### 4.11. Ngôn ngữ, theme và offline

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| SET-01 | P0 | Dữ liệu/phiên dở | 1. Đổi Việt → English. 2. Xem trang/menu/dialog. 3. Đổi lại Việt. | Nhãn đổi đúng; từ người dùng nhập không bị dịch; không mất phiên/dữ liệu. | NT |
| SET-02 | P0 | Cài đặt | 1. Chọn Sáng, xem Thư viện/thẻ/form/dialog. 2. Chọn Tối, kiểm tra lại. | Sage Paper sáng, banner sáng; Tối có cặp màu riêng; chữ/nút/lỗi đọc được, không trắng trên nền sáng. | NT |
| SET-03 | P1 | Có thể đổi theme OS | 1. Chọn Hệ thống. 2. Đổi OS sáng/tối. 3. Xem app/system bar Android. | Theo theme OS, biểu tượng system bar vẫn rõ. | NT |
| SET-04 | P0 | Ngôn ngữ/theme khác mặc định | 1. Đóng app bình thường. 2. Mở lại. | Giữ lựa chọn, dữ liệu và sao. | NT |
| SET-05 | P0 | D01, tắt mạng | 1. Tạo/sửa thẻ. 2. Học/xem lịch sử. 3. Mở lại vẫn offline. | Chức năng local không cần tài khoản/backend. Nếu thử nghe phải chuẩn bị giọng offline; tách lỗi TTS khỏi CRUD/học. | NT |
| SET-06 | P1 | Cài đặt | 1. Xem dữ liệu thiết bị. | Có đường dẫn vocabmate.json; không mô tả là cloud backup hoặc file mã hóa. | NT |

### 4.12. Giao diện và tình huống bất thường

| ID | Ưu tiên | Tiền điều kiện | Thao tác | Kết quả mong đợi | KQ |
| --- | --- | --- | --- | --- | --- |
| UX-01 | P0 | Windows khoảng 390/1280 DIP; Android dọc/ngang | 1. Xem danh sách/form/học/dialog. 2. Resize/xoay lúc nhập. | Không che nút chính, mất input, chồng chữ; đổi một/hai cột khi đủ chỗ; không kéo ngang để dùng chức năng chính. | NT |
| UX-02 | P1 | Danh sách dài/editor nhiều dòng | 1. Cuộn chuột/touchpad/phím Windows. 2. Vuốt Android. | Thanh cuộn ẩn theo thiết kế nhưng cuộn được tới cuối. | NT |
| UX-03 | P1 | Windows, màn học | 1. Tab/Shift+Tab. 2. 1–4 chọn quiz, 1/2 đánh giá flashcard. 3. Enter/Space khi phù hợp. 4. Gõ số trong input. | Focus rõ; shortcut ngoài input hoạt động; gõ số trong ô không bị chiếm; không chấm hai lần. | NT |
| UX-04 | P1 | Android, bàn phím mềm/dialog | 1. Nhập cuối form. 2. Cuộn tới Lưu. 3. Back/Hủy/chạm ngoài dialog. | Nút cần thiết tiếp cận được; hủy không xóa/nhập/thay phiên; Back giữ nháp đã persist. | NT |
| UX-05 | P2 | Cỡ chữ lớn; Narrator/TalkBack | 1. Mở trang chính. 2. Duyệt control/đọc lỗi. | Không cắt nội dung quan trọng; control có tên, focus hợp lý; hình trang trí không đọc thừa. Phải kiểm tra thực tế, không mặc định PASS. | NT |
| UX-06 | P1 | Tắt animation OS | 1. Lật thẻ hai lần. 2. Đánh giá/sang câu. | Mặt cập nhật đúng không cần animation; không kẹt nội dung/nút. | NT |
| SAFE-01 | P0 | Form thêm/lưu/import và màn trả lời | 1. Bấm nhanh hai lần lúc đang xử lý. 2. Xem dữ liệu/tiến độ. | Không ghi trùng, nhân đôi kết quả, chấm hai lần cùng câu; ghi màn/nhịp bấm nếu có lỗi. | NT |
| SAFE-02 | P2 | QA biệt lập, có bản sao sạch, app đóng | 1. Làm hỏng JSON thử. 2. Mở app. 3. Đóng, kiểm tra file. 4. Khôi phục bản sao khi app đóng. | Báo lỗi đọc, không tự ghi đè bằng dữ liệu rỗng. Tuyệt đối không thử trên dữ liệu thật/máy demo. | NT |

## 5. Kịch bản demo 12–15 phút

### Trước buổi demo

- Chuẩn bị X01/X03/X04, mở sẵn thư mục; theme Sáng, tiếng Việt, âm lượng vừa đủ, giọng Anh đã kiểm tra.
- Dùng lớp demo sạch; có bảng đáp án D01; dùng `zzz-demo-wrong` để tạo câu sai.
- Không thay phiên dở của người dùng thật. Dùng dữ liệu riêng và tập lại kịch bản một lần.
- Không demo file hỏng/lớn, 101 lượt hay xóa liên đới trong luồng chính. Muốn trình diễn xóa, dùng riêng lớp `DEMO-Delete me` ở phần hỏi đáp.

| Mốc | Thao tác | Điểm cần nói / thấy được |
| --- | --- | --- |
| 0:00–0:45 | Giới thiệu Thư viện/theme. | App offline Windows/Android, tổ chức lớp/bộ/thẻ. “Lớp” là phân nhóm local, không phải lớp học cộng tác. |
| 0:45–2:00 | Tạo DEMO-English A1, Everyday Basics, thêm mô tả. | Không cần tài khoản. |
| 2:00–3:00 | Chọn X01, xem preview rồi xác nhận. | Đúng 8 thẻ; xem trước không tự ghi. |
| 3:00–3:45 | Tìm qua tao, xóa tìm kiếm; sao apple/cat, lọc. | Tìm không dấu; Đã thuộc 2, Chưa thuộc 6. Sao chưa phải lịch ôn thông minh. |
| 3:45–5:00 | Flashcards, Anh → Việt, Chưa thuộc. Lật/nghe/Đã nhớ một câu. Lật câu sau, thoát và Tiếp tục. | Tự chuyển sau đánh giá; resume đúng mặt/câu. Chưa cần hoàn tất lượt. |
| 5:00–6:00 | Tùy chỉnh → Trắc nghiệm, Tất cả; xác nhận thay phiên. Trả lời 1–2 câu. | Bốn lựa chọn, phản hồi ngay; chỉ một phiên dở. |
| 6:00–8:30 | Đổi Viết, Việt → Anh, Tất cả; hoàn tất 8 câu, sai đúng 1 câu. | Kết quả 7/8; đọc đáp án theo câu đang hiện, không theo thứ tự D01. |
| 8:30–9:15 | Ôn lại câu sai, trả lời đúng, hoàn tất. | Đúng 1 câu trong lượt ôn, không học lại cả bộ. |
| 9:15–10:30 | Về bộ, Ghép cặp, Tất cả; sai một lần rồi hoàn tất. | Tối đa 6 cặp, đếm sai/thời gian. |
| 10:30–11:15 | Mở Lịch sử, chỉ lượt 7/8 và lượt ôn. | Lượt ôn có kết quả riêng; phiên bỏ dở không là lượt hoàn tất. |
| 11:15–12:00 | Đổi Tối/Sáng và English/Việt. | UI đổi, nội dung từ không đổi. |
| 12:00–13:00 | Thêm thẻ chưa Lưu; chờ, Back, thêm lại. Sau demo nháp thì Bỏ nháp. | Khôi phục nháp, Lưu mới vào bộ; bỏ nháp để dữ liệu D01 vẫn sạch. |
| 13:00–14:00 | Chọn X04, xem lỗi rồi hủy; nếu còn giờ chọn X03 chỉ preview. | Lỗi chặn cả lô; X03 là 2 mới/3 trùng nếu bộ vẫn D01. |
| 14:00–15:00 | Tổng kết và đề xuất mở rộng. | Tách rõ tính năng có sẵn với ý tưởng chưa triển khai. |

### Bản rút gọn 5 phút

1. Chuẩn bị D01 sẵn, không nhập tay.
2. Tìm không dấu/sao → Flashcards/lật/nghe → thoát/tiếp tục.
3. Đổi Viết, hoàn tất 8 câu với 1 câu sai → kết quả → ôn sai.
4. Lịch sử, đổi theme, kết bằng đề xuất lịch ôn và sao lưu.
5. Bỏ ghép cặp, import lỗi, nháp và test biên khỏi bản ngắn.

### Xử lý sự cố

- Không có tiếng: nói rõ phụ thuộc giọng OS, bỏ qua phát âm, không cài dịch vụ giữa buổi.
- Picker không thấy file: kiểm tra thư mục/định dạng; dùng D01 chuẩn bị sẵn làm dự phòng.
- Hết từ theo bộ lọc: đổi Tất cả; không xóa sao hàng loạt.
- Thiếu đáp án quiz: dùng D01 đầy đủ, không dùng bộ 1–3 thẻ.
- Không hứa “theme đảm bảo không mỏi mắt”, “dữ liệu đã lên cloud” hay “hiểu mọi từ đồng nghĩa”.

## 6. Báo lỗi và gate trước demo

```text
Bug ID: BUG-001
Case / build / OS / thiết bị: ...
Tiền điều kiện và dữ liệu: ...
Các bước tái hiện:
1. ...
2. ...
Kỳ vọng: ...
Thực tế: ...
Mức độ: Blocker / Major / Minor / Cosmetic
Tần suất: ... lần / ... lần thử
Ảnh/video: ...
Ảnh hưởng dữ liệu: có / không / chưa rõ
Workaround / khôi phục: ...
Retest / build đã sửa: ...
```

- [ ] Chạy toàn bộ kịch bản trên **chính thiết bị sẽ trình diễn** ít nhất một lần.
- [ ] P0 trong luồng demo PASS; không còn crash, sai điểm hoặc mất dữ liệu.
- [ ] Khôi phục fixture/bộ mẫu sau chạy thử.
- [ ] Kiểm tra dấu tiếng Việt, theme, kích thước cửa sổ, giọng đọc.
- [ ] Có bộ dự phòng để không phụ thuộc picker.
- [ ] Ghi giới hạn đã biết; không ghi đã test Android nếu chỉ chạy Windows.

### Kiểm thử tự động hỗ trợ

```powershell
dotnet test MauiApp1.Tests/MauiApp1.Tests.csproj -c Release
dotnet build MauiApp1/MauiApp1.csproj -c Release -f net10.0-windows10.0.19041.0
dotnet build MauiApp1/MauiApp1.csproj -c Release -f net10.0-android
```

Build/test core không thay thế kiểm tra picker, giọng đọc, bàn phím, responsive và thao tác trên thiết bị. Không chép trạng thái PASS từ tài liệu khác vào phiếu chạy.


## 7. Đề xuất mở rộng chức năng

Đây là **đề xuất dựa trên app hiện tại**, chưa triển khai. Công sức là ước lượng tương đối, không phải cam kết thời gian giao hàng.

### 7.1. Roadmap ưu tiên

| Thứ tự | Chức năng | Giá trị | MVP nên làm | Công sức / lưu ý |
| --- | --- | --- | --- | --- |
| 1 | Sao lưu và khôi phục | Bảo vệ dữ liệu local khi đổi máy/gỡ app; dễ demo giá trị thực tế | Xuất lớp/bộ/thẻ/sao; preview số lượng/schema trước khôi phục; tạo bản sao dữ liệu cũ trước thay thế | Vừa. Xác định rõ thay thế hay hợp nhất; không tự ghi đè. Phiên dở/nháp nên nằm ngoài MVP hoặc có quy tắc riêng. |
| 2 | Lịch ôn cách quãng | Trả lời câu hỏi “hôm nay cần ôn gì”, không chỉ học lại một bộ | Thời điểm ôn tiếp theo/mức nhớ; hàng đợi Đến hạn; đánh giá Quên/Khó/Nhớ/Dễ | Vừa–lớn. Tách lịch ôn khỏi sao thủ công; bắt đầu bằng quy tắc minh bạch, có kiểm thử đồng hồ/dữ liệu cũ. |
| 3 | Thống kê và mục tiêu ngày | Hiển thị tiến bộ dài hạn | Số từ học/ôn, độ chính xác theo mode, mục tiêu từ/ngày, biểu đồ 7 ngày | Vừa. Định nghĩa “từ” và “lượt”; không đếm resume là lượt mới; không trộn ghép sai với tỷ lệ đúng quiz. |
| 4 | Chế độ tập trung, nhắc nghỉ | Phù hợp hướng giao diện dịu và học trong thời gian dài | Hẹn giờ tùy chỉnh, tạm dừng, nhắc nghỉ nhẹ, ẩn thông tin phụ; không khóa việc học khi hết giờ | Nhỏ–vừa. Lưu thời gian học chủ động riêng vì thời gian phiên hiện tính cả lúc nghỉ; không cam kết lợi ích sức khỏe. |
| 5 | Thẻ có IPA, ví dụ, ghi chú | Học cách dùng từ thay vì chỉ dịch | Trường tùy chọn do người dùng nhập; hiển thị khi lật thẻ | Vừa. Migration schema, giữ tương thích Excel hai cột; ảnh/audio tùy chỉnh làm sau do dung lượng và backup. |
| 6 | Quản lý hàng loạt | Giảm thao tác khi có nhiều thẻ | Chọn nhiều, chuyển bộ, nhân bản bộ; xác nhận trước thao tác phá hủy | Vừa. Duplicate ở đích, ảnh hưởng phiên dở; cần xác nhận hoặc Undo. |
| 7 | Nhóm từ hay sai | Ôn đúng điểm yếu dựa trên lịch sử | Danh sách sai nhiều theo khoảng ngày, tạo phiên học từ nhóm đó | Vừa. Theo ID thẻ, không chỉ dựa vào 100 kết quả gần nhất; không đồng nhất “hay sai” với “chưa có sao”. |
| 8 | Đồng bộ nhiều thiết bị | Chuyển giữa Windows và Android | Sau backup/migration: tài khoản tùy chọn, hàng đợi đồng bộ, trạng thái offline | Lớn. Backend, xung đột sửa/xóa, quyền riêng tư và vận hành; chưa nên là bước đầu. |
| 9 | Gợi ý nghĩa/ví dụ bằng AI | Giảm công nhập liệu | Gợi ý ở màn soạn thẻ; người dùng duyệt và xác nhận trước lưu | Lớn/phụ thuộc dịch vụ. Kiểm soát chi phí và dữ liệu gửi đi; không lấy nội dung gợi ý làm tiêu chuẩn chấm tuyệt đối. |

### 7.2. Nên chọn hướng nào?

**Bản nâng cấp thực dụng:** sao lưu/khôi phục → lịch ôn → thống kê. Ba phần bổ sung trực tiếp cho nền offline, từ bảo vệ dữ liệu đến cá nhân hóa việc học và theo dõi tiến bộ.

**Muốn demo sớm với phạm vi nhỏ:** mục tiêu ngày + chế độ tập trung. Cần định nghĩa cách đo trước, không chỉ thêm đồng hồ trang trí.

**Chọn một tính năng làm điểm nhấn đồ án:** lịch ôn cách quãng. Demo: đánh giá thẻ → xem lần ôn tiếp → mở hàng đợi đến hạn. Dùng đồng hồ giả trong môi trường test, không đổi ngày OS máy cá nhân để trình diễn.

**Chưa ưu tiên:** mạng xã hội, bảng xếp hạng, chat nhóm hoặc lớp học giáo viên/học sinh. Chúng mở rộng sang sản phẩm cộng tác, cần tài khoản/backend và chưa giải quyết nhu cầu học từ hằng ngày của app hiện tại.

### 7.3. Tiêu chí nghiệm thu cho ba ưu tiên đầu

| Tính năng | Tiêu chí tối thiểu |
| --- | --- |
| Backup/restore | Khôi phục đúng số lớp/bộ/thẻ/sao; file hỏng/schema không hỗ trợ bị từ chối; Hủy không đổi dữ liệu; thất bại không để dữ liệu nửa cũ nửa mới. |
| Lịch ôn | Đánh giá cập nhật ngày đến hạn nhất quán; hàng đợi không lấy thẻ chưa đến hạn; restart giữ lịch; test múi giờ/đổi ngày; khởi tạo dữ liệu cũ theo quy tắc có tài liệu. |
| Thống kê | Khớp phiên mẫu có kết quả biết trước; không đếm trùng resume; phân biệt mode; xử lý ngày không học; tổng hợp không mất khi lịch sử chi tiết vượt 100 lượt. |

### 7.4. Các giới hạn hiện có cần nói đúng khi demo

- Dữ liệu local, chưa có đăng nhập, cloud sync, backup/restore tích hợp hoặc cộng tác nhiều người.
- JSON chưa mã hóa; chưa hỗ trợ nhiều tiến trình cùng ghi file.
- Sao là đánh dấu đã thuộc, chưa có thuật toán lên lịch ôn.
- Tối đa 20 câu/lượt thường, 6 cặp ghép và 100 kết quả hoàn tất được giữ.
- Một phiên dở duy nhất; bắt đầu phiên mới thay phiên trước sau xác nhận.
- Chấm viết không tự bỏ dấu hoặc đoán mọi từ đồng nghĩa.
- Phát âm dùng giọng OS, không phải chấm chất lượng phát âm của người học.
- Import đọc sheet hiển thị đầu tiên, không gộp nhiều sheet; file có dòng lỗi bị chặn cả lô.
- Nháp không đồng nghĩa thẻ chính đã lưu; lifecycle là best effort khi force-kill/lỗi ghi.

## 8. Điểm đối chiếu mã nguồn

| Nguồn | Quy tắc đối chiếu |
| --- | --- |
| `MauiApp1/Services/VocabularyRules.cs` | Giới hạn tên/thẻ, chuẩn hóa cặp từ |
| `MauiApp1/Services/VocabularySearch.cs` | Tìm không dấu, bộ lọc sao |
| `MauiApp1/Services/JsonVocabularyRepository.cs` | CRUD/cascade/duplicate/nháp/phiên/giới hạn lịch sử |
| `MauiApp1/Services/LearningEngine.cs` | Chấm, nhiều nghĩa, giới hạn câu/cặp, điều kiện bắt đầu |
| `MauiApp1/ViewModels/ImportViewModel.cs` | Preview, chặn lô lỗi và bỏ trùng |
| `MauiApp1/Services/XlsxWorkbook.cs` | Header, sheet, công thức và giới hạn Excel |
| `MauiApp1/ViewModels/CardEditorViewModel.cs` | Nháp và lưu thẻ chính |
| `MauiApp1/ViewModels/LearningViewModel.cs` | Resume, đổi mode, thao tác học |
| `MauiApp1/ViewModels/HistoryViewModel.cs` | Ôn sai từ lịch sử |
| `MauiApp1/ViewModels/SettingsViewModel.cs` | Theme và ngôn ngữ |
| `MauiApp1/Services/Pronunciation.cs` | Phụ thuộc giọng đọc OS |
| `MauiApp1/Views/LearningPage.Windows.cs` | Phím tắt Windows |
| `MauiApp1/Themes/StudyTheme.xaml` | Bảng màu và style |
| `MauiApp1.Tests/DesignSystemTests.cs` | Kiểm tra design system tự động |
