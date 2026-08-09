<RULE[project]>
# Cấu trúc Backend Module và Vertical Slice Architecture (VSA)

Khi xây dựng hoặc chỉnh sửa tính năng cho Backend, BẮT BUỘC tuân thủ cấu trúc sau (dựa trên module `_Samples`):

1. **Cấu trúc lồng Project và Đặt tên (Nesting & Naming):**
   - Đặt folder ngoài cùng mang tên Module (vd: `src/Modules/Orders`).
   - Bên trong folder đó, bắt buộc tạo một thư mục con mang tiền tố project: `Automation.Orders`.
   - File project (.csproj) và Root Namespace bắt buộc phải mang tên: `Automation.Orders.csproj` và `Automation.Orders`.
   - Mọi thành phần (Domain, Infrastructure, API, Application) ưu tiên gói gọn trong project này, KHÔNG chia nhỏ thành các tầng layer khác nhau (như `Orders.Domain.csproj`). Trừ trường hợp cần tách riêng giao tiếp (Contracts), có thể tạo project `Automation.Orders.Contracts` nằm ngang hàng trong thư mục gốc của module.

2. **Cấu trúc thư mục của Module:**
   - `Domain/`: Chứa Entities, Value Objects, Domain Events, Interfaces của riêng module.
   - `Infrastructure/`: Chứa EF Core Configurations, DbContext (nếu có riêng), Repositories, và các dịch vụ gọi ra ngoài (External integrations).
   - `Features/`: Chứa các tính năng tổ chức theo chiều dọc (Vertical Slices).
   - `Shared/`: Các thành phần dùng chung nội bộ trong module (như DTOs chung).

3. **Kiến trúc tính năng theo chiều dọc (Vertical Slice - VSA):**
   - Đặt tính năng trong thư mục: `Features/<FeatureGroup>/<FeatureName>/` (VD: `Features/Orders/CreateOrder/`).
   - **ĐẶC BIỆT LƯU Ý:** Tại thư mục `Features/<FeatureGroup>/`, bắt buộc phải có một file Endpoint Group (VD: `OrdersGroup.cs` hoặc `SampleItemsGroup.cs`) để gom nhóm các API (thường kế thừa `Group` của FastEndpoints).
     - Khi cấu hình cho `Group`, luôn phải thêm `.WithTags("<Tên_Group>")` để Swagger hiển thị nhóm (ví dụ: `ep.Description(x => x.WithTags("Users"));`).
   - Một slice tính năng hoàn chỉnh thường bao gồm các file sau nằm CÙNG một thư mục (`Features/<FeatureGroup>/<FeatureName>/`):
     - `*Command.cs` hoặc `*Query.cs`: Request model (đầu vào của tính năng).
     - `*Endpoint.cs`: API Endpoint (thường kế thừa từ `Endpoint` của FastEndpoints) và phải khai báo `Group<FeatureGroup>()` trỏ về Endpoint Group tương ứng.
     - `*Handler.cs`: Xử lý nghiệp vụ chính (thường dùng Wolverine handler để nhận Command/Query).
     - `*Validator.cs`: Khai báo FluentValidation cho request.
</RULE[project]>

4. **Sử dụng CLI để tạo Module và CRUD:**
   - TUYỆT ĐỐI KHÔNG tự scaffold (tạo tay) các thư mục và file cho một Module mới hoặc các tính năng CRUD.
   - Luôn sử dụng công cụ CLI có sẵn tại `tools/Automation.Cli`.
   - Lệnh tạo Module: `dotnet run --project tools/Automation.Cli -- add-module <TênModule>` (Ví dụ: `Billing`).
   - Lệnh tạo CRUD: `dotnet run --project tools/Automation.Cli -- add-crud <TênModule> <TênEntity>` (Ví dụ: `Billing Invoice`).
   - Sau khi sinh code bằng CLI, bạn có thể vào chỉnh sửa các thuộc tính của Entity, cấu hình DTO, Validator, và logic trong Handler.

5. **Global Using Check:**
   - Trước khi viết hoặc chỉnh sửa một tính năng (feature), LUÔN kiểm tra file `GlobalUsing.cs` ở thư mục gốc của module đó (VD: `src/Modules/<ModuleName>/GlobalUsing.cs`) để xem những namespace nào đã được import global.
   - Tránh việc khai báo lại (using thừa) các namespace đã có sẵn trong file `GlobalUsing.cs` tại các file con (endpoint, handler, dto...).

6. **Tôn trọng Triết Lý Kiến Trúc (Architecture Philosophy):**
   - Trước khi đề xuất bất cứ sự thay đổi nào về mặt kiến trúc, cấu trúc thư mục, hoặc áp dụng Design Pattern mới, BẮT BUỘC phải đọc và thấm nhuần tài liệu `docs/ARCHITECTURE.md`.
   - Mọi đoạn code được sinh ra đều phải tuân thủ tinh thần tinh gọn, đề cao Use Case (VSA), và tránh rườm rà (Ceremony) như đã nêu trong tài liệu.

7. **Sử dụng Mapster cho Data Mapping:**
   - LUÔN LUÔN sử dụng thư viện Mapster (bằng cách dùng `.Adapt<TDto>()`, `.ProjectToType<TDto>()` hoặc `.BuildAdapter().AdaptToType<TDto>()` đối với các thuộc tính cần truyền thêm) để ánh xạ dữ liệu giữa Entities và DTOs.
   - TUYỆT ĐỐI KHÔNG tự map bằng tay từng trường (như `var dto = new Dto(entity.A, entity.B);`) nhằm tiết kiệm thời gian code và giữ code sạch sẽ. Trừ khi mapping cực kỳ phức tạp và không thể dùng cấu hình Mapster.

8. **Quy định về Access Modifiers (public vs internal):**
   - **Mặc định là internal:** Mọi class, interface sinh ra bên trong module (thuộc các tầng Infrastructure, Domain, ) phải để ở mức internal. Điều này đảm bảo tính đóng gói (encapsulation) của Modular Monolith, ngăn các module khác gọi trực tiếp vào chi tiết nội bộ.
   - **Chỉ dùng public cho Cổng giao tiếp:** Chỉ sử dụng public cho các class/interface cần giao tiếp ra ngoài như: API Endpoints (kế thừa Endpoint), các file trong thư mục Contracts, DTOs/Commands/Queries, các Wolverine Handlers, và class cấu hình DI (ví dụ ModuleRegistry).
   - **Technical Requirement:** Trong file .csproj của mỗi module, phải luôn có <InternalsVisibleTo Include="Automation.Api" /> để cấp quyền cho project Host (Web API) có thể quét và sinh code tự động (Wolverine) cũng như chạy các lệnh Entity Framework Core Migrations trên các class internal.

9. **Tắt Background Server Sau Khi Dùng:**
   - Khi chạy các lệnh như `.\cli run`, `dotnet run` hoặc `npm run dev` để verify hoặc test, BẮT BUỘC phải dùng tool `manage_task` để tắt (kill) tiến trình này ngay sau khi xác nhận xong, tránh việc giam (lock) file `.dll` hoặc port ảnh hưởng đến các lệnh build hoặc thao tác tiếp theo của người dùng.

10. **T? Ch?c Giao Ti?p & C?u H�nh Li�n Module:**
   - Khi m?t module c?n c?u h�nh ho?c giao ti?p v?i m?t module kh�c (v� d?: module Identity dang k� SystemSetting, ho?c module A dang k� AssetSlot c?a module Files), TUY?T �?I KH�NG vi?t c�c do?n m� c?u h�nh n�y tr?c ti?p v� r?i r?c b�n trong phuong th?c ConfigureServices c?a file *Module.cs.
   - B?T BU?C ph?i t?o c�c Extension Methods ri�ng (v� d?: IdentitySystemSettingExtensions.cs ho?c IdentityAssetExtensions.cs) b�n trong thu m?c Extensions/ c?a module d? nh�m c�c c?u h�nh theo t?ng ph?m vi (scope).
   - Sau d�, file *Module.cs ch? vi?c g?i c�c extension method n�y (v� d?: services.AddIdentitySystemSettings()) nh?m gi? cho file c?u h�nh lu�n g?n g�ng, d? qu?n l� v� ph�n t�ch r� r�ng tr�ch nhi?m.

11. **Chuẩn Hóa Phân Quyền (Permissions):**
   - Mọi module phải định nghĩa các quyền (permissions) tập trung trong thư mục Constants thông qua một lớp như <Module>Permissions.cs.
   - Lớp này cần khai báo các Feature dưới dạng inner class kế thừa BaseCrudPermission('tên_feature') hoặc BasePermission('tên_feature'), và bộc lộ thông qua các thuộc tính tĩnh (static properties).
   - BẮT BUỘC phải ghi đè hàm GetPermissions() trả về Dictionary ánh xạ tất cả các tính năng đó.
   - Module class (*Module.cs) phải implement IPermissionModule và trả về new Constants.<Module>Permissions().GetPermissions();.
   - File GlobalUsing.cs của module phải chứa global using P = Automation.<ModuleName>.Constants.<Module>Permissions;.
   - Các API Endpoint của tính năng tương ứng BẮT BUỘC phải sử dụng .Permissions(P.<FeatureName>.<Action>); thay vì AllowAnonymous() (trừ những API thực sự public).

12. **Ưu Tiên Sử Dụng CodeGraph Cho Phân Tích & Task Phức Tạp:**
   - Khi cần phân tích luồng kiến trúc của hệ thống CQRS và giao tiếp chéo Module (Wolverine MessageBus) hoặc chuẩn bị lập Implementation Plan, **BẮT BUỘC ưu tiên sử dụng CodeGraph** (codegraph_explore MCP tool) thay vì dùng lệnh tìm kiếm văn bản như grep hay iew_file độc lập.
   - Ưu điểm: CodeGraph hỗ trợ theo dõi ngữ nghĩa (semantic tracking), ngay lập tức truy vết được những đầu mối gọi tới (callers) và đích đến (callees) băng qua các abstract Interface. Nó trả về mã nguồn đầy đủ của toàn bộ file liên quan trong một lần gọi duy nhất, tối ưu mạnh mẽ quá trình Impact Analysis và giảm token so với cách đọc từng file.

9. **Always Use Public Modifier**: M?c d?nh s? d?ng public access modifier cho t?t c? c�c class, record, endpoint, handler, query, command, v.v... tr? khi c� y�u c?u c? th? kh�c, d? tr�nh c�c l?i li�n quan d?n reflection/Wolverine kh�ng t�m th?y file.


## 11. Terminal & Background Process Management
- **MANDATORY TERMINAL CONTROL:** Bắt buộc ưu tiên sử dụng terminal để thực thi lệnh. KHÔNG ĐƯỢC để server/tiến trình tự chạy ngầm (background) ngoại trừ trường hợp thực sự cần thiết trong thời gian rất ngắn.
- **CLEAN UP BACKGROUND TASKS:** Nếu bắt buộc phải chạy background task, LUÔN LUÔN phải kill task đó ngay lập tức sau khi dùng xong. Không bao giờ để lại tiến trình treo ngầm.
- **User Control:** Trả lại hoàn toàn quyền kiểm soát terminal cho User sau khi xác nhận code hoạt động.

## 13. Quy tắc kiến trúc Shared Kernel
- **Tuân thủ nghiêm ngặt Abstraction & Infrastructure**: Khi viết hoặc sửa đổi code trong SharedKernel, bắt buộc phải tuân thủ sự phân tách rõ ràng giữa lớp Abstractions (chỉ chứa interfaces, base classes, models) và Infrastructure (chứa concrete implementations, cấu hình services).
- **Phân tách Extensions**: Các cấu hình extension dành riêng cho WebApp, Services hay Host phải được giữ riêng biệt (ví dụ: Automation.SharedKernel.Extensions). Không được để các tầng không liên quan phụ thuộc chéo hoặc nhồi nhét chung logic vào core của SharedKernel.

## 14. Endpoint Return Types and FluentResults Wrapping
- **Không bọc kiểu trả về của Endpoint trong Result/Result<T>:** Khi định nghĩa các API Endpoints (kế thừa `Endpoint<TRequest, TResponse>`), kiểu trả về `TResponse` của endpoint tuyệt đối KHÔNG ĐƯỢC bọc trong `Result` hay `Result<T>` của FluentResults. Trả trực tiếp kiểu dữ liệu gốc (ví dụ: `CursorPage<NotificationDto>`, `RoleDto` thay vì `Result<CursorPage<NotificationDto>>`). Việc bọc `Result` ở lớp này làm sai lệch OpenAPI spec và gây sinh kiểu dữ liệu sai/phức tạp ở frontend khi gen bằng Orval. Logic xử lý lỗi FluentResults vẫn nằm trong Service/Handler, nhưng khi truyền ra Endpoint để gửi về Client, phải được unwrapped thông qua Extension Method `.SendResultAsync()` và khai báo kiểu trả về của Endpoint là kiểu dữ liệu thô.

## 15. Quy định về Route Params và JSON Body cho Request/Command
- **Không expose Route Param ra JSON Body:** Nếu một property (ví dụ `Id`, `ProjectId`, `ContentTypeKey`) được lấy từ Route Param (vd: `/{projectId}/content-types/{Id}`), BẮT BUỘC phải khai báo request model (Command/Query) dưới dạng `record { ... }` (hoặc class) có properties thay vì primary constructor `record(Type Prop);`.
- **Sử dụng `[JsonIgnore]`:** Phải gắn attribute `[JsonIgnore]` (từ `System.Text.Json.Serialization`) lên các property lấy từ route đó. Điều này đảm bảo Swagger và Orval frontend không sinh ra các trường này trong payload JSON Body, tránh việc người dùng/frontend phải truyền dư thừa dữ liệu.
- **Ví dụ:**
  ```csharp
  public record UpdateItemCommand {
      [JsonIgnore]
      public Guid Id { get; set; } // Lấy từ route
      
      public string Name { get; set; } = null!; // Lấy từ body
  }
  ```
