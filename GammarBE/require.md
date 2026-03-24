### ĐỊNH HƯỚNG VÀ QUY ƯỚC MÃ NGUỒN (AI CODING GUIDELINES)
name:xây dựng chức năng backend
description:Định hướng và quy ước mã nguồn cho dự án backend ASP.NET Coreweb API.
Dưới đây là bộ quy tắc chuẩn mực dành cho AI/Developer khi làm việc với dự án này. Hãy tuân thủ nghiêm ngặt để đảm bảo sự đồng nhất và chất lượng mã nguồn.

#### 1. Giao tiếp và Lên kế hoạch (Communication & Planning)
- Luôn phân tích, giải thích và viết kế hoạch (plan) bằng **Tiếng Việt**.
- **Không tự ý sửa các file trong thư mục Models (Entities)**. Nếu giải pháp yêu cầu thay đổi cấu trúc Database, phải giải thích rõ lý do và xin phép tôi trước.

#### 2. Cấu trúc Project & Naming Convention
- **Tên file, class, method, biến**: Sử dụng tiếng Anh cơ bản, rõ nghĩa (vd: `AuthServices`, `AuthController`), nhưng **comment giải thích logic trong code phải viết bằng Tiếng Việt**.
- **Services và Interfaces**: 
  - Interface cho các service phải được viết cùng trong file với class triển khai service đó (**không tạo file Interface riêng biệt**). 
  - Interface chỉ nên phơi bày (expose) các phương thức thực sự cần thiết được gọi từ Controller, ẩn các phương thức nội bộ.
- **Tiện ích dùng chung**: Các logic dùng chung cho nhiều module phải được gom lại và đặt trong class `CommonServices`.

#### 3. Chuẩn hóa Controller & API Response
- Các endpoint ưu tiên chỉ sử dụng phương thức `[HttpGet]` và `[HttpPost]`.
- Kiểu dữ liệu trả về của Controller/API nên sử dụng `dynamic` nếu có thể để linh hoạt.
- **Format Response**: Mọi API trả về phải bao gồm các trạng thái thống nhất theo định dạng Key-Value:
- `code`: Căn cứ theo quy ước HTTP status code (200, 400, 404, 500...) hoặc custom code nội bộ.
- `message`: Nêu rõ thông điệp hay kết quả thực thi của nghiệp vụ (vd: "Thêm mới thành công", "Không tìm thấy dữ liệu").

#### 4. Logic Nghiệp vụ & Xử lý lỗi (Business Logic & Exception Handling)
- **Validation**: Luôn validate đầu vào cực kỳ cẩn thận cho mọi rủi ro có thể xảy ra. Điển hình: Ràng buộc tính duy nhất (Email, Username không được trùng trong DB), kiểm tra bản ghi có tồn tại hay không trước khi Cập nhật (Update)/Xóa (Delete).
- **Try/Catch**: Luôn bọc logic của các chuỗi nghiệp vụ phức tạp bằng `try...catch` để bắt và xử lý lỗi một cách triệt để, tránh ứng dụng bị dừng đột ngột (crash).
- **Transaction**: Bắt buộc dùng tính năng Transaction khi thực thi các chuỗi tác vụ liên quan đến Cập nhật/Thêm/Xóa dữ liệu hoặc tác động tới nhiều bảng DB cùng lúc. Phải đảm bảo luôn `Commit` hoặc `Rollback` đầy đủ để giữ tính toàn vẹn dữ liệu.

#### 5. Entity Framework Core, LINQ & Async/Await
- **Async/Await**: 100% các hàm nghiệp vụ, I/O hoặc gọi Database đều phải dùng hàm bất đồng bộ (`async` và `await`).
- **AsNoTracking**: Bắt buộc sử dụng `.AsNoTracking()` cho tất cả các truy vấn đọc dữ liệu (quá trình tính toán không làm thay đổi state DB) để tối ưu hóa bộ nhớ và tốc độ xử lý.
- **Tối ưu LINQ**: Cố gắng tối ưu số lần gọi và hiệu suất truy vấn Database:
  - Dùng `.AnyAsync()` để kiểm tra dữ liệu tồn tại, thay vì đếm số lượng/lấy cả object (tránh tốn cost query).
  - Ưu tiên lệnh `.Select()` và lấy đúng các trường (fields) dữ liệu cần thiết thay vì lấy toàn bộ bảng.

#### 6. Dựa theo cấu trúc dự án này
- Dự án là một kiến trúc ASP.NET Core có thể có chia view hoặc xài API. Gồm các thành phần quan trọng: `Controllers`, `Models`, `Services`, `Views`, cấu hình tập trung vào `Program.cs`, khai triển tài liệu PDF thông qua folder `PDFDocument` và luồng HTTP request có qua custom `Middleware.cs`. Khi làm việc, mọi thay đổi nếu đụng chạm các phần global này cần cực kỳ cẩn thận.