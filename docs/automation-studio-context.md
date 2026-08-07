# Automation Studio — Entity Schema & Module Breakdown

> Tổng hợp toàn bộ entity đã chốt qua các buổi thảo luận, chia theo Bounded Context để áp vào kiến trúc Modular Monolith (`Automation`).

---

## Nguyên tắc chia module

1. **Module hạ tầng** (`Identity`, `Files`) — generic tuyệt đối, không biết bất kỳ khái niệm nghiệp vụ nào của module khác.
2. **Module nghiệp vụ** — sở hữu business logic, chỉ tham chiếu module hạ tầng qua FK/contract, không đảo ngược.
3. Quan hệ polymorphic (`EntityType` + `EntityId`) dùng để module hạ tầng phục vụ nhiều module nghiệp vụ mà không cần biết chúng là gì.
4. Không tạo bảng khi 1 field JSONB đã đủ diễn tả (VD: `ContentManifest`, `ParamsConfig`) — chỉ tách bảng khi cần **query trực tiếp bằng SQL/index** hoặc cần **gán quan hệ thật** (VD: `TagLink`).

---

## 1. Module `Identity` (đã có sẵn — ASP.NET Identity, không sửa)

```
AspNetUsers
AspNetRoles
AspNetUserRoles
```

Chỉ dùng cho **System Role** (SuperAdmin/User), không dùng cho phân quyền theo Project.

---

## 2. Module `Projects`

```csharp

Project {
  Id, Name
}

ProjectMember {
  Id, ProjectId (FK), UserId (FK → AspNetUsers, KHÔNG gộp module)
  ProjectRole: Owner | Editor | Viewer
}
```

**Vai trò:** business entity gốc — mọi module nghiệp vụ khác đều FK về `Project.Id`. Phân quyền theo Project (Scoped Authorization) check qua `ProjectMember`, tách biệt hoàn toàn khỏi System Role của Identity.

---

## 3. Module `Content`

```csharp
ContentType {
  Id, ProjectId (FK), Key (string cố định, dùng trong code)
  Name (user tự đặt, hiển thị UI)
  FieldsConfig: JSONB    // [{ key, type, label, required, constraints }]
  DisplayConfig: JSONB   // { mode: "table"|"list-card", thumbnailField, titleField }
}

Content {
  Id, ContentTypeId (FK), ProjectId (FK)
  Values: JSONB          // dữ liệu thật theo đúng FieldsConfig
}
```

**Vai trò:** đại diện cho Character, Building, Environment... — mọi loại "vật" trong game, không cần bảng riêng cho từng loại. Field kiểu `asset_link` trong `FieldsConfig` (VD: `portrait`) tham chiếu `Asset.Id` bên module Files.

**Đã bỏ:** `Build → Collection → Asset → Instance` 4 tầng cũ (combo logic không cần thiết — combo xảy ra bên trong Daz Studio trước khi export).

---

## 4. Module `Tag`

```csharp
TagCategory {
  Id, ProjectId (FK), Name
  AppliesTo: "Content"   // chỉ dùng thật cho Content, KHÔNG dùng cho InspectionItem (xem lý do ở mục 7)
}

Tag {
  Id, TagCategoryId (FK), Name
}

TagLink {
  Id, TagId (FK)
  EntityType: string     // "Content" | "InspectionItem"
  EntityId: Guid          // polymorphic, không FK cứng
}
```

**Vai trò:** gắn nhãn để tìm kiếm/lọc, generic cho bất kỳ entity nào có `Id` thật. Không dùng cho object bên trong file (`.duf`/`.blend`) — cái đó đã tách thành `InspectionItem` (mục 7) để có Id thật, mới gán được `TagLink`.

---

## 5. Module `Storage`

```csharp
Storage {
  Id, ProjectId (FK), Name
  Kind: Local | R2
  RootPath: nullable string   // chỉ có ý nghĩa nếu Kind = Local
}
```

**Vai trò:** chỉ trả lời "Local hay Cloud". **Không có `PlatformId`** — đã bỏ vì chuyển sang hash-based storage key (SHA256), không cần convention path theo Platform nữa.

---

## 6. Module `Platform` (catalog nhẹ)

```csharp
Platform {
  Id, Key: "blender" | "daz" | "unreal" | "unity" | "godot"
  Name
}
```

**Vai trò:** seed data thuần, không có logic, không sở hữu Inspector/Executor. Các module khác chỉ tham chiếu `Platform.Key` (string), không FK cứng bắt buộc.

---

## 7. Module `Files` (đã có sẵn — generic, không sửa)

```csharp
Asset {
  Id, Path (R2 key theo hash), FileExtension, Sha256Hash
}

AssetLink {
  Id, AssetId (FK)
  EntityType: string   // "Content", v.v.
  EntityId: Guid
  Slot: string          // "DazSource" | "BlendMaster" | "FbxExport" | "portrait"
}
```

**Vai trò:** hạ tầng lưu trữ file thuần túy — không biết Content/Character là gì. Upload/Download luôn đi qua module này.

---

## 8. Module `Resource` (mở rộng từ Files, gắn business context)

```csharp
Resource {
  Id, ProjectId (FK), StorageId (FK)
  AssetId (FK → Files.Asset)   // liên kết tới file vật lý thật
}

ResourceVersion {
  Id, ResourceId (FK)
  VersionNo: int
  AssetId (FK)                 // mỗi version trỏ 1 Asset khác nhau (immutable versioning)
  CreatedAt
}
```

**Vai trò:** đại diện "1 file được quản lý trong Project", có lịch sử version. `Resource` liên kết với `Content` qua `AssetLink` (module Files) với `Slot` tương ứng.

---

## 9. Module `Inspection`

```csharp
Inspector {
  Id, Key, PlatformKey            // string, không FK cứng
  SupportedExtension: string
  ScriptPath: string               // dùng chung hạ tầng Worker để chạy
  PrimaryFieldPath: string         // VD "objects" — field nào trong ResultJson là danh sách chính
}

ContentTypeInspectorConfig {
  Id, ContentTypeId (FK), InspectorKey (string)
  RelevantFieldPath: nullable string   // override PrimaryFieldPath nếu cần field khác
  DisplayLabel: string
}

Inspection {
  Id, ResourceVersionId (FK)
  InspectorKey: string              // string, KHÔNG FK cứng — Inspection là snapshot bất biến
  ResultJson: JSONB                  // dữ liệu thô, cấu trúc tự do theo từng Inspector
  InspectedAt
}

InspectionItem {
  Id, InspectionId (FK)
  Name: string                       // bắt buộc — dùng làm object name khi resolve cho Pipeline
  RawData: JSONB                     // phần dữ liệu còn lại của item đó (type, vertex_count...)
}
```

**Vai trò:** tự động (hoặc thủ công) đọc file, sinh metadata. `InspectionItem` được tạo tự động từ `Inspection.ResultJson[PrimaryFieldPath]` — mỗi phần tử trong mảng đó thành 1 row, có `Id` thật để `TagLink` gán vào (user tự tag "body"/"hair" sau khi xem kết quả — không kỳ vọng Inspector tự đoán đúng 100%).

**3 trigger chạy Inspection** (không loại trừ nhau):
- Auto theo `ContentTypeInspectorConfig` khi Resource mới tạo
- Thủ công — user bấm chạy thêm Inspector cho Resource đã có
- Tự động sau khi Pipeline sinh ra Resource mới (spawn_instance)

---

## 10. Module `Pipeline`

```csharp
Script {
  Id, Name, WorkerType: "Blender" | "Daz" | "Unreal"
  ScriptPath
  ParamsConfig: JSONB   // [{ key, type, uiControl, min, max, options, default }]
}

ToolDefinition {
  Id, Key, Name
  InputPins: JSONB      // [{ key, dataType }]
  OutputPins: JSONB     // [{ key, dataType }]
  HandlerKey: string     // định danh code C# thực thi (IResolverTool)
}

SessionDefinition {
  Id, Name, WorkerType
  Flow: JSONB   // [{ type: "Script"|"ToolCall", refId, order }]
}

NodeDefinition {
  Id, Kind: "Session" | "Tool"
  RefId: Guid    // trỏ SessionDefinition.Id hoặc ToolDefinition.Id
}

Pipeline {
  Id, ProjectId (FK), Name
}

PipelineNode {
  Id, PipelineId (FK), NodeDefinitionId (FK)
  PositionX, PositionY
}

PipelineEdge {
  Id, PipelineId (FK)
  SourcePipelineNodeId (FK), SourcePin: string
  TargetPipelineNodeId (FK), TargetPin: string
}

PipelineExecution {
  Id, PipelineId (FK)
  Status: Pending | Running | Succeeded | Failed
  StartedAt, FinishedAt
}

NodeExecution {
  Id, PipelineExecutionId (FK), PipelineNodeId (FK)
  Status
  Progress: JSONB   // [{ stepName, status }] — thay StepExecution riêng
}
```

**Vai trò:** thiết kế và thực thi quy trình tự động hóa. `Script` tái sử dụng nhiều nơi (Script Library). `SessionDefinition` = "công thức" 1 phiên Worker (giữ RAM xuyên suốt nếu là Blender). `Tool` (`IResolverTool`) chạy đồng bộ trong `.NET`, không qua RabbitMQ — chỉ `Session` mới publish message cho Worker.

**Đã bỏ:** `StepExecution` riêng — gộp vào `NodeExecution.Progress` (JSONB) để giảm số bảng, chấp nhận đánh đổi query SQL trực tiếp lấy đơn giản hóa schema.

---

## Sơ đồ phụ thuộc giữa các module

```
Identity (hạ tầng)
   ↑ (chỉ FK UserId, không đảo chiều)
Project
   ↑
   ├── Content ──┐
   ├── Tag        │
   ├── Storage     │ (đều FK → ProjectId)
   ├── Resource    │
   ├── Inspection  │
   └── Pipeline ──┘

Platform (catalog nhẹ, đứng riêng — Storage/Inspector/Script chỉ tham chiếu Key)

Files (hạ tầng, generic)
   ↑ (Resource, Content dùng qua AssetLink)
```

---

## Điểm mấu chốt cần giữ khi implement

1. **Không FK cứng cho quan hệ polymorphic** (`EntityType` + `EntityId` trong `AssetLink`, `TagLink`) — giữ generic đúng nghĩa.
2. **`InspectorKey` / `HandlerKey` là string, không FK cứng** — Inspection/Tool là snapshot/logic độc lập với entity định nghĩa, tránh vỡ khi code thay đổi.
3. **Worker luôn Dumb** — chỉ `SessionDefinition` (qua `NodeDefinition.Kind = "Session"`) mới publish message cho Worker qua RabbitMQ. `Tool` luôn chạy đồng bộ trong `.NET`.
4. **Validate tại boundary, không tin script tuyệt đối** — mọi file Script tự ghi ra local (`save_scene.py`...) phải qua 1 Node `upload` riêng (Resolver Tool) kiểm tra path/format trước khi persist vào `Files` module.
