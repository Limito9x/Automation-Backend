\**# Cấu trúc Backend Module và Vertical Slice Architecture (VSA)**

Khi xây dựng hoặc chỉnh sửa tính năng cho Backend, BẮT BUỘC tuân thủ cấu trúc sau (dựa trên module \`\_Samples\`):

1\. **\*\*Cấu trúc lồng Project và Đặt tên (Nesting & Naming):\*\***
   - Đặt folder ngoài cùng mang tên Module (vd: \`src/Modules/Orders\`).
   - Bên trong folder đó, bắt buộc tạo một thư mục con mang tiền tố project: \`Automation.Orders\`.
   - File project (.csproj) và Root Namespace bắt buộc phải mang tên: \`Automation.Orders.csproj\` và \`Automation.Orders\`.
   - Mọi thành phần (Domain, Infrastructure, API, Application) ưu tiên gói gọn trong project này, KHÔNG chia nhỏ thành các tầng layer khác nhau (như \`Orders.Domain.csproj\`). Trừ trường hợp cần tách riêng giao tiếp (Contracts), có thể tạo project \`Automation.Orders.Contracts\` nằm ngang hàng trong thư mục gốc của module.

2\. **\*\*Cấu trúc thư mục của Module:\*\***
   - \`Domain/\`: Chứa Entities, Value Objects, Domain Events, Interfaces của riêng module.
   - \`Infrastructure/\`: Chứa EF Core Configurations, DbContext (nếu có riêng), Repositories, và các dịch vụ gọi ra ngoài (External integrations).
   - \`Features/\`: Chứa các tính năng tổ chức theo chiều dọc (Vertical Slices).
   - \`Shared/\`: Các thành phần dùng chung nội bộ trong module (như DTOs chung).

3\. **\*\*Kiến trúc tính năng theo chiều dọc (Vertical Slice - VSA):\*\***
   - Đặt tính năng trong thư mục: \`Features/\<FeatureGroup>/\<FeatureName>/\` (VD: \`Features/Orders/CreateOrder/\`).
   - **\*\*ĐẶC BIỆT LƯU Ý:\*\*** Tại thư mục \`Features/\<FeatureGroup>/\`, bắt buộc phải có một file Endpoint Group (VD: \`OrdersGroup.cs\` hoặc \`SampleItemsGroup.cs\`) để gom nhóm các API (thường kế thừa \`Group\` của FastEndpoints).
     - Khi cấu hình cho \`Group\`, luôn phải thêm \`.WithTags("\<Tên\_Group>")\` để Swagger hiển thị nhóm (ví dụ: \`ep.Description(x => x.WithTags("Users"));\`).
   - Một slice tính năng hoàn chỉnh thường bao gồm các file sau nằm CÙNG một thư mục (\`Features/\<FeatureGroup>/\<FeatureName>/\`):
     - \`\*Command.cs\` hoặc \`\*Query.cs\`: Request model (đầu vào của tính năng).
     - \`\*Endpoint.cs\`: API Endpoint (thường kế thừa từ \`Endpoint\` của FastEndpoints) và phải khai báo \`Group\<FeatureGroup>()\` trỏ về Endpoint Group tương ứng.
     - \`\*Handler.cs\`: Xử lý nghiệp vụ chính (thường dùng Wolverine handler để nhận Command/Query).
     - \`\*Validator.cs\`: Khai báo FluentValidation cho request.
\
4\. **\*\*Sử dụng CLI để tạo Module và CRUD:\*\***
   - TUYỆT ĐỐI KHÔNG tự scaffold (tạo tay) các thư mục và file cho một Module mới hoặc các tính năng CRUD.
   - Luôn sử dụng công cụ CLI có sẵn tại \`tools/Automation.Cli\`.
   - Lệnh tạo Module: \`dotnet run --project tools/Automation.Cli -- add-module \<TênModule>\` (Ví dụ: \`Billing\`).
   - Lệnh tạo CRUD: \`dotnet run --project tools/Automation.Cli -- add-crud \<TênModule> \<TênEntity>\` (Ví dụ: \`Billing Invoice\`).
   - Sau khi sinh code bằng CLI, bạn có thể vào chỉnh sửa các thuộc tính của Entity, cấu hình DTO, Validator, và logic trong Handler.

5\. **\*\*Global Using Check:\*\***
   - Trước khi viết hoặc chỉnh sửa một tính năng (feature), LUÔN kiểm tra file \`GlobalUsing.cs\` ở thư mục gốc của module đó (VD: \`src/Modules/\<ModuleName>/GlobalUsing.cs\`) để xem những namespace nào đã được import global.
   - Tránh việc khai báo lại (using thừa) các namespace đã có sẵn trong file \`GlobalUsing.cs\` tại các file con (endpoint, handler, dto...).

6\. **\*\*Tôn trọng Triết Lý Kiến Trúc (Architecture Philosophy):\*\***
   - Trước khi đề xuất bất cứ sự thay đổi nào về mặt kiến trúc, cấu trúc thư mục, hoặc áp dụng Design Pattern mới, BẮT BUỘC phải đọc và thấm nhuần tài liệu \`docs/ARCHITECTURE.md\`.
   - Mọi đoạn code được sinh ra đều phải tuân thủ tinh thần tinh gọn, đề cao Use Case (VSA), và tránh rườm rà (Ceremony) như đã nêu trong tài liệu.

7\. **\*\*Sử dụng Mapster cho Data Mapping:\*\***
   - LUÔN LUÔN sử dụng thư viện Mapster (bằng cách dùng \`.Adapt\<TDto>()\`, \`.ProjectToType\<TDto>()\` hoặc \`.BuildAdapter().AdaptToType\<TDto>()\` đối với các thuộc tính cần truyền thêm) để ánh xạ dữ liệu giữa Entities và DTOs.
   - TUYỆT ĐỐI KHÔNG tự map bằng tay từng trường (như \`var dto = new Dto(entity.A, entity.B);\`) nhằm tiết kiệm thời gian code và giữ code sạch sẽ. Trừ khi mapping cực kỳ phức tạp và không thể dùng cấu hình Mapster.

8\. **\*\*Quy định về Access Modifiers (Sử dụng public):\*\***
   - **\*\*Sử dụng public cho toàn bộ class/interface:\*\*** Mọi class, interface, enum, struct được sinh ra trong Backend (bao gồm Wolverine Handlers, Endpoints, DbContexts, EF Configurations, Validators, DTOs, Commands, Queries, Entities) BẮT BUỘC để ở mức \`public\`.
   - **\*\*Lý do:\*\*** Tránh triệt để các lỗi tự động quét assembly (Assembly Scanning) và DI registration của Wolverine, FastEndpoints, EF Core hoặc Mapster do không phát hiện được class \`internal\`.

9\. **\*\*Tắt Background Server Sau Khi Dùng:\*\***
   - Khi chạy các lệnh như \`.\cli run\`, \`dotnet run\` hoặc \`npm run dev\` để verify hoặc test, BẮT BUỘC phải dùng tool \`manage\_task\` để tắt (kill) tiến trình này ngay sau khi xác nhận xong, tránh việc giam (lock) file \`.dll\` hoặc port ảnh hưởng đến các lệnh build hoặc thao tác tiếp theo của người dùng.

10\. **\*\*Tổ Chức Giao Tiếp & Cấu Hình Liên Module:\*\***
   - Khi một module cần cấu hình hoặc giao tiếp với một module khác (ví dụ: module Identity đăng ký SystemSetting, hoặc module A đăng ký AssetSlot của module Files), TUYỆT ĐỐI KHÔNG viết các đoạn mã cấu hình này trực tiếp và rời rạc bên trong phương thức ConfigureServices của file \`*Module.cs\`.
   - BẮT BUỘC phải tạo các Extension Methods riêng (ví dụ: IdentitySystemSettingExtensions.cs hoặc IdentityAssetExtensions.cs) bên trong thư mục Extensions/ của module để nhóm các cấu hình theo từng phạm vi (scope).
   - Sau đó, file \`*Module.cs\` chỉ việc gọi các extension method này (ví dụ: services.AddIdentitySystemSettings()) nhằm giữ cho file cấu hình luôn gọn gàng, dễ quản lý và phân tách rõ ràng trách nhiệm.

11\. **\*\*Chuẩn Hóa Phân Quyền (Permissions):\*\***
   - Mọi module phải định nghĩa các quyền (permissions) tập trung trong thư mục Constants thông qua một lớp như \<Module>Permissions.cs.
   - Lớp này cần khai báo các Feature dưới dạng inner class kế thừa BaseCrudPermission('tên\_feature') hoặc BasePermission('tên\_feature'), và bộc lộ thông qua các thuộc tính tĩnh (static properties).
   - BẮT BUỘC phải ghi đè hàm GetPermissions() trả về Dictionary ánh xạ tất cả các tính năng đó.
   - Module class (\*Module.cs) phải implement IPermissionModule và trả về new Constants.\<Module>Permissions().GetPermissions();.
   - File GlobalUsing.cs của module phải chứa global using P = Automation.\<ModuleName>.Constants.\<Module>Permissions;.
   - Các API Endpoint của tính năng tương ứng BẮT BUỘC phải sử dụng .Permissions(P.\<FeatureName>.\<Action>); thay vì AllowAnonymous() (trừ những API thực sự public).


12. **Ưu Tiên Sử Dụng CodeGraph MCP Cho Phân Tích & Task Phức Tạp:**
   - Khi cần phân tích luồng kiến trúc, dependency, CQRS, giao tiếp chéo Module (Wolverine MessageBus), impact analysis, hoặc chuẩn bị Implementation Plan, **BẮT BUỘC ưu tiên sử dụng CodeGraph MCP**.
   - Khi gọi CodeGraph MCP, **BẮT BUỘC truyền trực tiếp tham số `projectPath`** trỏ tới root của repository/project cần phân tích.
   - **Không sử dụng CLI command gốc của CodeGraph** khi MCP tool đã cung cấp tham số `projectPath`.
   - Ưu tiên semantic exploration, symbol tracing, callers/callees và dependency analysis thay vì grep hoặc đọc file rời rạc khi task cần hiểu quan hệ giữa các thành phần.
   - Chỉ dùng text/file search thông thường cho các thao tác đơn giản như tìm chính xác tên file, symbol, string hoặc nội dung không cần semantic analysis.
   - Trước khi lập Implementation Plan cho task phức tạp, phải dùng CodeGraph để xác định implementation, dependency và pattern hiện có; không tự suy đoán kiến trúc chỉ từ tên file/thư mục.

**## 13. Terminal & Background Process Management**
\- **\*\*MANDATORY TERMINAL CONTROL:\*\*** Bắt buộc ưu tiên sử dụng terminal để thực thi lệnh. KHÔNG ĐƯỢC để server/tiến trình tự chạy ngầm (background) ngoại trừ trường hợp thực sự cần thiết trong thời gian rất ngắn.
\- **\*\*CLEAN UP BACKGROUND TASKS:\*\*** Nếu bắt buộc phải chạy background task, LUÔN LUÔN phải kill task đó ngay lập tức sau khi dùng xong. Không bao giờ để lại tiến trình treo ngầm.
\- **\*\*User Control:\*\*** Trả lại hoàn toàn quyền kiểm soát terminal cho User sau khi xác nhận code hoạt động.

**## 14. Quy tắc kiến trúc Shared Kernel**
\- **\*\*Tuân thủ nghiêm ngặt Abstraction & Infrastructure\*\***: Khi viết hoặc sửa đổi code trong SharedKernel, bắt buộc phải tuân thủ sự phân tách rõ ràng giữa lớp Abstractions (chỉ chứa interfaces, base classes, models) và Infrastructure (chứa concrete implementations, cấu hình services).
\- **\*\*Phân tách Extensions\*\***: Các cấu hình extension dành riêng cho WebApp, Services hay Host phải được giữ riêng biệt (ví dụ: Automation.SharedKernel.Extensions). Không được để các tầng không liên quan phụ thuộc chéo hoặc nhồi nhét chung logic vào core của SharedKernel.

**## 15\. **\*\*Endpoint Return Types and FluentResults Wrapping\*\***
\- **\*\*Không bọc kiểu trả về của Endpoint trong Result/Result\<T>:\*\*** Khi định nghĩa các API Endpoints (kế thừa \`Endpoint\<TRequest, TResponse>\`), kiểu trả về \`TResponse\` của endpoint tuyệt đối KHÔNG ĐƯỢC bọc trong \`Result\` hay \`Result\<T>\` của FluentResults. Trả trực tiếp kiểu dữ liệu gốc (ví dụ: \`CursorPage\<NotificationDto>\`, \`RoleDto\` thay vì \`Result\<CursorPage\<NotificationDto>>\`). Việc bọc \`Result\` ở lớp này làm sai lệch OpenAPI spec và gây sinh kiểu dữ liệu sai/phức tạp ở frontend khi gen bằng Orval. Logic xử lý lỗi FluentResults vẫn nằm trong Service/Handler, nhưng khi truyền ra Endpoint để gửi về Client, phải được unwrapped thông qua Extension Method \`.SendResultAsync()\` và khai báo kiểu trả về của Endpoint là kiểu dữ liệu thô.

16\. **\*\*Quy định về Transaction Control trong Wolverine Handlers:\*\***
\- **\*\*Thêm/Sửa/Xóa (Write / Mutation Handlers):\*\*** Khi Handler xử lý các thao tác ghi (Create, Update, Delete) hoặc có gọi service giao tiếp chéo Module (liên quan 2 DB module như \`IAssetApi\` / \`ISchemaApi\`), BẮT BUỘC khai báo attribute \`[Transactional(typeof(<ModuleName>DbContext))]\` trỏ tới DbContext của CHÍNH MODULE ĐÓ (hoặc \`[NonTransactional]\` nếu tự điều phối thủ công \`SaveChangesAsync\` và gọi external service). **TUYỆT ĐỐI KHÔNG import hay reference DbContext của module khác.**
\- **\*\*Truy vấn (Read-Only / Query Handlers):\*\*** BẮT BUỘC khai báo attribute \`[NonTransactional]\` lên Handler class để tránh việc Wolverine tự mở database transaction thừa cho các tác vụ chỉ đọc dữ liệu.
