# VocabMate — Thiết Kế UX/UI & Wireframes

Tài liệu này định nghĩa toàn bộ nguyên tắc thiết kế, hệ thống component, layout đa nền tảng và chi tiết trải nghiệm người dùng (UX) cho ứng dụng **VocabMate**. Ứng dụng là một sản phẩm thực thụ, tối giản, offline, tập trung vào hiệu suất học tập.

---

## 1. Nguyên Tắc Thiết Kế (Design Principles)
*   **Minimalism & Focus:** Loại bỏ các yếu tố trang trí thừa. Không dùng gradient, không dùng shadow (bóng đổ); bề mặt trang/thẻ/dialog dùng màu đặc. Riêng lớp phủ phía sau dialog dùng alpha để vẫn thấy trang hiện tại bị làm dịu, không thay trang bằng nền trống. Mọi thành phần sinh ra đều có mục đích (chữ rõ ràng, nút bấm dễ chạm).
*   **Native & Adaptive:** Tôn trọng thói quen của người dùng trên từng nền tảng. Windows tận dụng không gian rộng (chuột/bàn phím), Android tối ưu cho thao tác chạm và vuốt (cầm một tay).
*   **Immediate Feedback:** Giao diện phản hồi ngay lập tức (Validation ngay khi gõ, đánh dấu đúng/sai tức thì trong bài học, tự động khôi phục nháp mà không cần hỏi).

---

## 2. Hệ Thống Design Token (Colors & Typography)

### 2.1. Màu sắc (Color Palette - `StudyTheme.xaml`)
Ứng dụng hỗ trợ hai chế độ sáng/tối đồng nhất.

*   **Chế độ Sáng (Light Mode):**
    *   **Background (Nền app):** `#FAFAFA` (Trắng ngà/xám cực nhạt, giúp dịu mắt hơn trắng tinh `#FFFFFF`).
    *   **Surface (Nền của card/list item):** `#FFFFFF` (Trắng nổi nhẹ trên nền `#FAFAFA`).
    *   **Primary/Accent (Nhấn mạnh):** `#E0F2FE` (Xanh dương pastel làm nền nút chọn) / `#0EA5E9` (Xanh dương êm dịu, không chói).
    *   **Text (Chữ):** `#334155` (Xám slate đậm, mềm mại hơn đen tuyền, tránh gắt mắt). Text phụ: `#64748B`.
    *   **Semantic (Trạng thái):** `#4ADE80` (Xanh lá pastel - Đúng), `#F87171` (Đỏ nhạt/hồng - Lỗi/Sai).
    *   **Borders (Viền):** `#F1F5F9` (Xám nhạt, tiệp màu với nền).

*   **Chế độ Tối (Dark Mode):**
    *   **Background:** `#1E293B` (Xanh slate đậm, dịu và sang hơn đen kịt `#121212`).
    *   **Surface:** `#334155` (Xanh slate sáng hơn nền một chút).
    *   **Primary/Accent:** `#38BDF8` (Xanh dương sáng nhẹ, dịu mắt ban đêm).
    *   **Text:** `#F8FAFC` (Trắng xám mềm). Text phụ: `#94A3B8`.
    *   **Borders:** `#475569` (Xám slate đồng điệu).

### 2.2. Khoảng Cách (Spacing) & Hình Khối (Shapes)
*   **Page Padding (Lề ngoài):** 20 - 32 DIP.
*   **Gaps (Khoảng cách giữa các khối):** 20 - 28 DIP.
*   **Border Radius (Độ bo góc):** Thẻ và Nút bấm bo góc vừa phải (khoảng 8px - 12px), tạo cảm giác hiện đại nhưng không quá trẻ con.
*   **Dialog:** Popup nằm trên trang, rộng tối đa 360 DIP cho menu hoặc 400 DIP cho form/xác nhận. Padding 12–16 DIP, gap 6–12 DIP, nội dung cuộn cao tối đa 320 DIP và giảm tiếp theo chiều cao khả dụng. Cỡ chữ nội dung 13–14 DIP, tiêu đề 17–18 DIP theo kích thước cửa sổ; giữ font scaling và DPI của OS. Nút tối thiểu 36 DIP trên Windows, 44 DIP trên Android. Không scale cả dialog bằng transform hoặc lấy số pixel vật lý làm cỡ chữ.

---

## 3. Cấu Trúc Layout Đa Nền Tảng (Windows vs Android)

VocabMate giải quyết bài toán đa màn hình bằng `CollectionWorkspace` và `AdaptiveColumns`.

### 3.1. Phiên bản Android (Mobile - Portrait)
*   **Layout chuẩn:** Dạng danh sách cuộn dọc đơn thuần.
*   **Form nhập liệu:** Nếu có form (tạo Lớp, Bộ, Thẻ), form này sẽ được nhúng vào `CollectionView.Header`. Khi cuộn danh sách, form sẽ cuộn lên trên và biến mất khỏi màn hình, nhường chỗ cho danh sách hiển thị tối đa.

### 3.2. Phiên bản Windows (Desktop - Landscape rộng >= 900 DIP)
*   **Layout 2 cột (Master-Detail):** 
    *   **Cột Trái (Form):** Chiếm ~30% chiều rộng (Clamp trong khoảng 280 - 400 DIP). Cột này ghim cố định và có thanh cuộn (ScrollView) riêng độc lập.
    *   **Cột Phải (Danh sách):** Chiếm phần không gian còn lại (70%), chứa danh sách thẻ/bộ từ dạng Grid nhiều cột (`AdaptiveColumns` tự tính toán từ 1 đến 3 cột).
*   **Dưới 900 DIP (Thu nhỏ cửa sổ):** Fallback về 1 cột. Form ở trên (cao tối đa 48% màn hình), Danh sách ở dưới. Mỗi vùng cuộn độc lập.

---

## 4. Chi Tiết Trải Nghiệm Các Màn Hình Cốt Lõi (Screen-by-Screen)

### 4.1. Cấu trúc AppShell (Điều hướng)
Thanh Bottom Navigation (Android) hoặc Left Menu (Windows) gồm 3 Tab:
1.  **📚 Thư viện (Library):** Màn hình Home, chứa danh sách Class -> Deck.
2.  **🕒 Lịch sử (History):** Liệt kê các kết quả học.
3.  **⚙️ Cài đặt (Settings):** Chỉnh ngôn ngữ, theme.

---

### 4.2. Màn Hình Thư Viện (Library) & Form Nhập Liệu
*   **Header:** Tiêu đề trang (Title). Thanh tìm kiếm (SearchBar) nẳm ngay dưới tiêu đề.
*   **Vùng Form (Tạo mới):** 
    *   1 ô `VocabularyEntry` lớn (Tên lớp/Tên bộ).
    *   1 nút `Button` (Lưu). 
    *   **UX Validation:** Dưới ô nhập có 1 Label màu đỏ báo lỗi (ẩn đi nếu hợp lệ, hiện ra lập tức nếu gõ trùng tên hoặc để trống nhờ `RequiredTermBehavior`). Lỗi hiển thị inline, tuyệt đối **không** dùng Dialog Pop-up cản trở luồng gõ.
*   **Vùng Danh sách (List):** Các Item hiển thị dạng Card. Bấm vào Card để đi sâu vào bên trong (Từ Class -> Deck). Có nút (X) nhỏ góc phải Card để Xóa (Xóa sẽ hiện Dialog xác nhận vì ảnh hưởng dữ liệu con).

---

### 4.3. Màn Hình Chi Tiết Bộ Từ & Import Excel
*   **Header:** Hiển thị Tên bộ từ và Mô tả. Nút mũi tên Back.
*   **Nút Hành Động Nhanh (Action Row):** 
    *   Nút **"▶ Bắt đầu học"** (To và nổi bật nhất, màu Primary).
    *   Nút **"Tạo thẻ mới"** (Thêm thủ công).
    *   Nút **"Import Excel"**.
    *   Nút **"Xuất Excel"** (Chia sẻ file template).
*   **Danh sách thẻ:** Hiển thị 2 cột (Anh/Việt) song song trong một Card để dễ nhìn.
*   **Màn hình Preview Import (rất quan trọng cho UX):**
    *   Sau khi chọn file, hiện trang Preview dạng Bảng.
    *   Cột trạng thái: (Mới, Trùng, Lỗi). Dòng lỗi sẽ highlight viền đỏ, ghi rõ lý do (Ví dụ: "Thiếu nghĩa tiếng Việt").
    *   Nút "Xác nhận Import" bị `Disabled` (xám) nếu còn dù chỉ 1 dòng Lỗi. Người dùng phải sửa file Excel rồi chọn lại. Dòng "Trùng" bị mờ đi (bỏ qua).

---

### 4.4. Trải Nghiệm Học Tập (Learning Mode)
Trước khi học, hiện popup/vùng chọn tùy chỉnh: Chiều học (Anh-Việt, Việt-Anh), Chế độ (Trắc nghiệm, Lật thẻ...).

#### A. Chế độ Lật Thẻ (Flashcards)
*   **Giao diện:** Chỉ có 1 thẻ ở chính giữa màn hình. To, phông chữ bự. Nút 🔊 (Loa) ở góc thẻ để đọc TextToSpeech. 
*   **Tương tác (UX):** 
    *   Chạm vào thẻ -> Animation thẻ thu lại và lật ngang (thời gian 100ms+140ms). Mặt sau hiện ra đáp án.
    *   Lúc này hiện 2 nút ở dưới: ❌ **Chưa nhớ** (Màu đỏ/xám) và ✅ **Đã nhớ** (Màu xanh).
    *   Windows: Bấm phím `Space`/`Enter` để lật, phím `1` (Đã nhớ), `2` (Chưa nhớ).

#### B. Chế độ Trắc Nghiệm (Quiz)
*   **Giao diện:** Câu hỏi ở trên. Bên dưới là các lựa chọn.
*   **Tương tác (UX):**
    *   Adaptive: Màn hẹp (Mobile) thì 4 nút xếp thành 4 hàng dọc. Màn rộng (Windows) xếp lưới 2x2.
    *   Khi bấm Chọn: Nút đúng chuyển xanh lá, nút sai chuyển đỏ. Các nút khác khóa lại (`Disabled`). Hiện đáp án.
    *   Nút **"Tiếp theo"** tự động focus. Windows bấm `Enter` để đi tiếp. Bấm `1,2,3,4` để chọn.

#### C. Chế độ Viết (Typing)
*   **Giao diện:** Câu hỏi ở trên. Vùng giữa là ô `Entry` nhập chữ khổng lồ ở giữa. Nút **"Kiểm tra"**.
*   **Tương tác (UX):**
    *   Focus tự động nhảy vào ô `Entry` khi câu hỏi hiện ra.
    *   Gõ xong nhấn phím `Enter` -> Chấm điểm.
    *   Sai/Đúng sẽ hiện màu phản hồi. Ô text bị khóa (IsReadOnly=true) trong lúc xem đáp án để không vô tình sửa kết quả.

#### D. Chế độ Ghép Cặp (Matching Game)
*   **Giao diện:** Lưới Grid chứa tối đa 12 ô vuông (6 cặp Anh-Việt xáo trộn).
*   **Tương tác (UX):**
    *   Chạm ô thứ 1 -> Đổi màu viền (đang chọn).
    *   Chạm ô thứ 2 -> Nếu đúng cặp: 2 ô mờ đi (Disabled), khóa lại. Nếu sai: Viền nháy đỏ, bỏ chọn cả 2, tăng biến đếm "số lần ghép sai".

---

### 4.5. Màn Hình Lịch Sử & Ôn Lại (History & Review)
*   **Empty State:** "Chưa có kết quả học tập nào. Hãy bắt đầu một bài học!"
*   **Danh sách Card Kết Quả:** Mỗi Card hiển thị: 
    *   *Tiêu đề:* Tên bộ từ + Chiều học (Vd: Animals - Anh -> Việt).
    *   *Thông số:* Điểm số (vd: 18/20), Thời gian (vd: 5m 30s), Số câu sai (vd: 2).
*   **Action:** Nút **"Ôn lại từ sai"** nổi bật trên Card. Bấm vào sẽ nạp snapshot các câu sai của phiên đó và đẩy vào luồng học (Learning Engine) mới.

---

## 5. Tương Tác Vi Mô (Micro-interactions) & Keyboard (Đặc sản Windows)

*   **Lưu Nháp Tự Động (Auto-save):** Giao diện không có nút "Lưu nháp". Chỉ có nút "Lưu hoàn tất". App tự động debounce 400ms khi người dùng ngừng gõ để lưu xuống file tạm. Nút "Back" sẽ giữ nháp, không vứt bỏ công sức người dùng.
*   **Focus Management (Windows):** Focus đi theo vòng đời của câu hỏi. Mở trang -> Focus vào nút lật thẻ/hoặc ô nhập liệu. Chấm xong -> Focus vào nút Next. Chuyển câu -> Trả focus về trạng thái đầu. (Được xử lý bằng Handler và KeyDown trong code-behind, không cản trở accessibility).
*   **Dịch Runtime (Localization):** Khi ở màn hình Settings, bấm đổi "Tiếng Anh" -> Giao diện toàn App (Navigation, Nút bấm, Lỗi) đổi ngay lập tức nhờ `DynamicResource`. (Dữ liệu thẻ từ không bị dịch).
*   **Text To Speech (TTS):** Khi bấm nút Loa, gọi API Native. Nút Loa có thể mờ đi hoặc hiển thị hiệu ứng Loading nhỏ nếu OS mất độ trễ để synthesize giọng nói, sau khi đọc xong phục hồi trạng thái. Tôn trọng âm lượng hệ thống.

---
*Tài liệu này định hướng trực tiếp việc xây dựng XAML, DataTemplates, và Handlers theo kiến trúc đã có của ứng dụng VocabMate.*
