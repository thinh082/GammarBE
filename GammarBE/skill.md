---
name: aspnet-backend
description: Xây dựng chức năng backend cho dự án ASP.NET Core Web API theo đúng quy ước dự án. Kích hoạt skill này khi người dùng yêu cầu tạo mới hoặc chỉnh sửa Controller, Service, API endpoint, xử lý nghiệp vụ, validation, transaction, hoặc bất kỳ tác vụ backend nào trong dự án ASP.NET Core. Bắt buộc dùng khi nhắc đến các từ khóa: "tạo API", "viết service", "thêm endpoint", "xử lý nghiệp vụ", "LINQ", "Entity Framework".
---

# ASP.NET Core Backend Skill

Skill này hướng dẫn Claude tạo ra mã nguồn backend ASP.NET Core Web API **đúng chuẩn dự án**, nhất quán từ cấu trúc file đến cách xử lý lỗi.

> **Nguyên tắc vàng**: Mọi phân tích, giải thích, kế hoạch đều viết bằng **Tiếng Việt**. Mã nguồn (tên class, method, biến) dùng tiếng Anh. Comment trong code dùng **Tiếng Việt**.

---

## Quy trình thực hiện (Workflow)

Với mỗi yêu cầu backend, Claude cần đi qua các bước sau theo thứ tự:

### Bước 1 — Phân tích & Lên kế hoạch
- Đọc kỹ yêu cầu, xác định: cần tạo/sửa file nào, tác động tới bảng DB nào
- Nếu cần thay đổi file trong thư mục `Models/` (Entities) → **dừng lại, giải thích lý do và xin phép người dùng trước**
- Viết plan ngắn gọn bằng Tiếng Việt trước khi sinh code

### Bước 2 — Tạo Service
Tạo file `<FeatureName>Services.cs` theo cấu trúc:

```csharp
// Interface và class nằm cùng 1 file, KHÔNG tạo file Interface riêng
public interface I<FeatureName>Services
{
    // Chỉ expose các method được gọi từ Controller
    Task<dynamic> GetListAsync(...);
    Task<dynamic> CreateAsync(...);
}

public class <FeatureName>Services : I<FeatureName>Services
{
    private readonly AppDbContext _context;
    // Inject thêm CommonServices nếu cần logic dùng chung

    public <FeatureName>Services(AppDbContext context)
    {
        _context = context;
    }

    // === PUBLIC METHODS (expose qua Interface) ===

    public async Task<dynamic> GetListAsync(...)
    {
        // Logic nghiệp vụ ở đây
    }

    // === PRIVATE METHODS (nội bộ, không expose) ===

    private async Task<bool> CheckExistsAsync(int id)
    {
        return await _context.TableName.AnyAsync(x => x.Id == id);
    }
}
```

**Checklist khi viết Service:**
- [ ] Tất cả method dùng `async/await`
- [ ] Ưu tiên trả về `dynamic` để dễ map với JSON cho client (trường hợp cần thiết mới dùng kiểu khác)
- [ ] Query đọc dữ liệu có `.AsNoTracking()`
- [ ] Kiểm tra tồn tại dùng `.AnyAsync()`, không dùng `.Count()` hay `.FirstOrDefault()`
- [ ] `Select()` chỉ lấy đúng fields cần thiết
- [ ] Logic phức tạp bọc trong `try...catch`
- [ ] Chuỗi Insert/Update/Delete nhiều bảng phải dùng Transaction

### Bước 3 — Tạo Controller
Tạo file `<FeatureName>Controller.cs`:

```csharp
[ApiController]
[Route("api/[controller]")]
public class <FeatureName>Controller : ControllerBase
{
    private readonly I<FeatureName>Services _services;

    public <FeatureName>Controller(I<FeatureName>Services services)
    {
        _services = services;
    }

    // Ưu tiên chỉ dùng [HttpGet] và [HttpPost]
    [HttpPost("create")]
    public async Task<dynamic> Create([FromBody] CreateRequest request)
    {
        return await _services.CreateAsync(request);
    }
}
```

**Checklist khi viết Controller:**
- [ ] Chỉ dùng `[HttpGet]` và `[HttpPost]`
- [ ] Kiểu trả về là `dynamic`
- [ ] Không đặt logic nghiệp vụ trong Controller, chỉ gọi Service

### Bước 4 — Đăng ký Dependency Injection
Thêm vào `Program.cs` (cẩn thận khi sửa file global này):

```csharp
builder.Services.AddScoped<I<FeatureName>Services, <FeatureName>Services>();
```

---

## Chuẩn API Response

Mọi API **bắt buộc** trả về đúng format này:

```json
// Thành công
{ "code": 200, "message": "Thêm mới thành công", "data": { ... } }

// Lỗi nghiệp vụ
{ "code": 400, "message": "Email đã tồn tại trong hệ thống" }

// Không tìm thấy
{ "code": 404, "message": "Không tìm thấy dữ liệu" }

// Lỗi hệ thống
{ "code": 500, "message": "Đã xảy ra lỗi, vui lòng thử lại" }
```

---

## Patterns quan trọng

### Transaction (bắt buộc khi tác động nhiều bảng)
```csharp
await using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    // Thực hiện các thao tác DB
    await _context.SaveChangesAsync();
    await transaction.CommitAsync();
    return new { code = 200, message = "Thành công" };
}
catch (Exception ex)
{
    await transaction.RollbackAsync();
    return new { code = 500, message = "Đã xảy ra lỗi: " + ex.Message };
}
```

> [!NOTE]
> Khi sử dụng `BeginTransactionAsync`, hãy cân nhắc cấu hình `IsolationLevel` (ví dụ: `IsolationLevel.Serializable`) nếu trong transaction có các tác vụ đọc dữ liệu quan trọng hoặc các nghiệp vụ cộng/trừ số dư, tồn kho... để tránh các vấn đề như **Dirty Read**, **Race Condition** và đảm bảo tính nhất quán tuyệt đối.

### Validate DTO đầu vào
> Nếu xử lý validate DTO đầu vào quá nhiều trường thì nên viết hàm riêng và hàm đó nằm trong class DTO luôn.
```csharp
public class CreateRequest 
{
    public string Name { get; set; }
    // ...nhiều trường khác

    public string Validate()
    {
        if (string.IsNullOrEmpty(Name)) 
            return "Tên không được để trống";
        // ...các validation khác
        return null; // Hợp lệ
    }
}
```

### Validation trước khi Insert
```csharp
// Kiểm tra trùng lặp (unique constraint)
bool emailExists = await _context.Users
    .AnyAsync(u => u.Email == request.Email);
if (emailExists)
    return new { code = 400, message = "Email đã tồn tại trong hệ thống" };
```

### Validation trước khi Update/Delete
```csharp
// Kiểm tra bản ghi tồn tại
bool exists = await _context.Users.AnyAsync(u => u.Id == request.Id);
if (!exists)
    return new { code = 404, message = "Không tìm thấy người dùng" };
```

### Query tối ưu
```csharp
// ✅ Đúng: AsNoTracking + Select đúng fields
var list = await _context.Users
    .AsNoTracking()
    .Where(u => u.IsActive)
    .Select(u => new { u.Id, u.FullName, u.Email })
    .ToListAsync();

// ❌ Sai: Lấy cả bảng, có tracking
var list = await _context.Users.ToListAsync();
```

### Logic dùng chung → CommonServices
```csharp
// Các helper dùng ở nhiều module đặt vào CommonServices
public class CommonServices
{
    // Ví dụ: format ngày, sinh mã, kiểm tra quyền...
    public string GenerateCode(string prefix) { ... }
}
```

---

## Cấu trúc dự án tham chiếu

```
Project/
├── Controllers/      ← Chỉ routing + gọi Service
├── Services/         ← Logic nghiệp vụ + Interface cùng file
│   └── CommonServices.cs
├── Models/           ← ⚠️ KHÔNG tự ý sửa (Entities/DB schema)
├── Views/            ← Nếu có Razor Views
├── PDFDocument/      ← Template/luồng xuất PDF
├── Middleware.cs     ← ⚠️ Cẩn thận khi sửa (global HTTP pipeline)
└── Program.cs        ← ⚠️ Cẩn thận khi sửa (cấu hình global)
```

> Các file đánh dấu ⚠️ là **global**, mọi thay đổi phải báo và giải thích rõ với người dùng trước.