# Bối cảnh Dự án: GammarBE

## Techstack (Công nghệ sử dụng)
- **Framework**: .NET 8.0 (ASP.NET Core Web API)
- **Cơ sở dữ liệu chính**: PostgreSQL (Neon.tech) sử dụng Entity Framework Core
- **Bộ nhớ đệm (Caching)**: Redis (Upstash)
- **Hệ thống tin nhắn (Messaging/Queueing)**: RabbitMQ
- **Xác thực (Authentication)**: JWT (JSON Web Token) & Google OAuth
- **Tích hợp thanh toán**: PayOS
- **Bảo mật**: BCrypt.Net-Next (Mã hóa mật khẩu)
- **Tài liệu API**: Swagger/OpenAPI (Swashbuckle)

## Cấu trúc thư mục (Folder Structure)
- `Controllers/`: Chứa các điểm cuối API (endpoints) cho xác thực, quản lý người dùng, ví và các tiện ích chung.
  - `AuthController.cs`, `UserController.cs`, `WalletController.cs`, `CommonController.cs`
- `Services/`: Chứa các triển khai logic nghiệp vụ (business logic).
  - `AuthService.cs`, `UserService.cs`, `WalletServices.cs`, `CommonServices.cs`, `GenerationService.cs`
- `Models/`: Các mô hình và cấu trúc dữ liệu.
  - `Entities/`: Định nghĩa các thực thể cơ sở dữ liệu (ví dụ: `AppDbContext`).
  - `DTO/`: Các đối tượng chuyển đổi dữ liệu (Data Transfer Objects) để giao tiếp giữa client và server.
  - `Model/`: Các biểu diễn dữ liệu bổ sung.
- `Properties/`: Cấu hình cho các thiết lập khởi chạy dự án.
- `Program.cs`: Điểm đầu vào của ứng dụng và cấu hình dịch vụ (DI, Auth, Middleware).
- `appsettings.json`: Cấu hình cho các kết nối (DB, Redis), JWT, PayOS và các thiết lập Email.

## Trạng thái hiện tại (Current State)
- **Các khu vực chức năng**:
  - Quy trình xác thực đầy đủ (Đăng nhập, Đăng ký, Google Auth) với JWT được lưu trữ trong cookies.
  - Quản lý người dùng và ví được tích hợp với PayOS để thanh toán.
  - Tích hợp RabbitMQ để xử lý tin nhắn bất đồng bộ.
  - Redis được sử dụng để lưu trữ bộ nhớ đệm (được cấu hình trong `appsettings.json`).
- **Triển khai/Môi trường**:
  - Được cấu hình cho các yêu cầu đa nguồn (CORS) từ cả môi trường phát triển cục bộ và frontend được lưu trữ trên Vercel.
  - Cơ sở dữ liệu bên ngoài (Neon PostgreSQL).
  - Đang tích cực sử dụng Swagger để kiểm thử API trong chế độ phát triển.
