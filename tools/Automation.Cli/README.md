# Automation CLI Tool

Đây là công cụ tự động hóa (Scaffolding Tool) được viết riêng cho dự án **Automation**. Công cụ giúp tạo nhanh cấu trúc chuẩn của Kiến trúc Vertical Slice (VSA) bên trong một hệ thống Modular Monolith, đồng thời giảm thiểu sai sót do cấu hình thủ công.

## Yêu cầu
- Đã cài đặt .NET SDK (phiên bản tương thích với dự án).
- Chạy lệnh từ thư mục chứa tool: `tools/Automation.Cli` (hoặc có thể chạy trực tiếp từ thư mục gốc thông qua tham số `--project tools/Automation.Cli`).

## Các Lệnh (Commands) Cung Cấp

---

### 1. Tạo Module Mới (`add-module`)
Lệnh này sẽ tạo ra một Module hoàn chỉnh với đầy đủ các file cần thiết:
- Cấu trúc thư mục chuẩn `src/Modules/<TênModule>/Automation.<TênModule>`
- Khởi tạo file `.csproj` và tự động liên kết vào file Solution (`.sln`).
- Khởi tạo file `GlobalUsing.cs` để chia sẻ các namespace mặc định (FastEndpoints, Wolverine, v.v).
- Khởi tạo lớp `<TênModule>DbContext` trong thư mục `Infrastructure/Persistence`.
- Tự động đăng ký module vào `ModuleRegistry.cs` của tầng API.

**Cú pháp:**
```bash
dotnet run -- add-module <ModuleName>
```
**Ví dụ:**
```bash
dotnet run -- add-module Billing
```

---

### 2. Tạo Trọn Bộ CRUD Tính Năng (`add-crud`)
Lệnh này sẽ tự động sinh ra một nhóm Endpoint đầy đủ các thao tác Create, Update, Delete, GetById, và GetList phân trang (Gridify) cho một thực thể (Entity) cụ thể.
- Sinh ra `Entity` và `Dto` cơ bản.
- Sinh ra Endpoint Group cấu hình cho Swagger.
- Tất cả các Handlers được cấu hình theo chuẩn Class Injection (Tiêm `DbContext` thông qua constructor) và sử dụng `FluentValidation`.

**Cú pháp:**
```bash
dotnet run -- add-crud <ModuleName> <EntityName>
```
**Ví dụ:**
```bash
dotnet run -- add-crud Billing Invoice
```

---

### 3. Tạo Tính Năng Lẻ - Command (`add-command`)
Dùng khi bạn chỉ muốn tạo ra một Use-Case xử lý thay đổi dữ liệu đơn lẻ (Command).

**Cú pháp:**
```bash
dotnet run -- add-command <ModuleName> <FeatureGroup> <FeatureName>
```
**Ví dụ:**
```bash
dotnet run -- add-command Billing Invoices PayInvoice
```

---

### 4. Tạo Tính Năng Lẻ - Query (`add-query`)
Dùng khi bạn chỉ muốn tạo ra một Use-Case xử lý truy vấn dữ liệu đơn lẻ (Query).

**Cú pháp:**
```bash
dotnet run -- add-query <ModuleName> <FeatureGroup> <FeatureName>
```
**Ví dụ:**
```bash
dotnet run -- add-query Billing Invoices GetUnpaidInvoices
```

---

### 5. Gỡ Bỏ Module An Toàn (`remove-module`) ⚠️ DANGER ZONE
Dùng để dọn dẹp và xóa hoàn toàn một module khỏi dự án. Để tránh rủi ro xóa nhầm mã nguồn quan trọng, công cụ sẽ hiển thị cảnh báo đỏ và bắt buộc bạn phải nhập chính xác tên module để xác nhận xóa.
- Xóa vĩnh viễn toàn bộ thư mục `src/Modules/<ModuleName>`.
- Gỡ bỏ project khỏi `.sln`.
- Gỡ bỏ tham chiếu (Project Reference) khỏi `Automation.Api`.
- Tự động xóa khai báo của module khỏi `ModuleRegistry.cs`.

**Cú pháp:**
```bash
dotnet run -- remove-module <ModuleName>
```
**Ví dụ:**
```bash
dotnet run -- remove-module TestRemove
```

---

## Lưu Ý Quan Trọng
- **PascalCase**: Đặt tên Module, Entity và Feature luôn viết hoa chữ cái đầu (Ví dụ: `Catalog`, `Product`, `CreateProduct`).
- **Sau khi sinh code**: Bạn hãy chạy lệnh `dotnet build` tại thư mục gốc của giải pháp để chắc chắn code được liên kết và biên dịch thành công. Sau đó vào thư mục tính năng vừa tạo để tùy chỉnh `Properties`, `Validator` và logic nghiệp vụ bên trong `Handler`.


