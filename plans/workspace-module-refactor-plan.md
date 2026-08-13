# Plan Cải Tổ Module Resource → Workspace

## 1. Mục tiêu

Cải tổ `Automation.Resource` thành `Automation.Workspace`, làm rõ:

- **Workspace**: không gian làm việc logic của Project/team, nơi tập hợp Resource.
- **WorkspaceAgent**: physical realization của Workspace trên một Agent cụ thể, có `RootPath`.
- **Resource**: identity logic của asset trong Workspace.
- **ResourceVersion**: identity logic của một phiên bản nội dung cụ thể của Resource (xác định bởi Hash), **không gắn với vị trí vật lý**.
- **ResourceVersionLocation**: một nơi cụ thể (WorkspaceAgent + RelativePath) mà ResourceVersion đang tồn tại trên đĩa.

MVP tập trung vào **local workspace + Agent**. Chưa giải quyết permission/access control phức tạp và chưa đưa remote storage vào làm một loại Workspace.

> **Thay đổi so với bản trước:** tách `ResourceVersion` (nội dung) khỏi vị trí vật lý. Lý do: cùng một nội dung file (cùng Hash) có thể tồn tại đồng thời trên nhiều Agent khác nhau (VD: đã sync từ Alice-PC sang Render-PC) — đây vẫn là **cùng một Version**, không phải hai Version riêng biệt. Gắn `WorkspaceAgentId` trực tiếp lên `ResourceVersion` buộc hệ thống phải tạo Version mới mỗi lần file được sync sang Agent khác, làm sai lệch lịch sử version thật.

---

## 2. Boundary sau cải tổ

```text
Project
│
└── Workspace
    ├── Resource
    │   └── ResourceVersion
    │       └── ResourceVersionLocation
    │           └── WorkspaceAgent
    │
    └── WorkspaceAgent
        └── Agent
```

Các boundary liên quan:

```text
Agent
  = machine/runtime

Platform
  = software/capability

Workspace
  = logical team/project working space

WorkspaceAgent
  = Workspace trên một Agent + RootPath

Resource
  = asset identity trong Workspace

ResourceVersion
  = phiên bản nội dung cụ thể của Resource, xác định bởi Hash — KHÔNG gắn vị trí vật lý

ResourceVersionLocation
  = một nơi cụ thể (WorkspaceAgent + RelativePath) mà ResourceVersion đang tồn tại trên đĩa

Pipeline
  = transformation/orchestration

Storage Provider
  = nơi lưu trữ/transport khi cần publish hoặc vận chuyển data
```

Nguyên tắc:

- Workspace **không có `PlatformId`**.
- Workspace **không có `AgentId` trực tiếp**.
- Workspace có nhiều `WorkspaceAgent`.
- Agent là entity global, không thuộc Project.
- Resource thuộc Workspace.
- ResourceVersion thuộc Resource, xác định bởi nội dung (Hash), **không tham chiếu WorkspaceAgent trực tiếp**.
- ResourceVersionLocation thuộc ResourceVersion và tham chiếu WorkspaceAgent — **một ResourceVersion có thể có nhiều ResourceVersionLocation**.
- Remote R2 **không phải một loại Workspace trong MVP**.
- Upload/transfer chỉ xảy ra khi operation như publish, export hoặc pipeline yêu cầu.
- Discovery Resource không đồng nghĩa với upload binary file.

---

## 3. Domain Model

### Workspace

```text
Workspace
├── Id
├── ProjectId
├── Name
└── ...
```

Workspace đại diện cho không gian làm việc chung của team — thuần túy logic, không tự nhận là một "loại" lưu trữ nào.

Không chứa:

- `PlatformId`
- `AgentId`
- `RootPath`
- `Kind` (Local/R2...) — "Local hay không" là thuộc tính của từng `WorkspaceAgent`, không phải của `Workspace`. Một Workspace có thể có nhiều `WorkspaceAgent` cùng lúc, nên gán 1 `Kind` cố định cho cả Workspace là vô nghĩa.

### WorkspaceAgent

```text
WorkspaceAgent
├── Id
├── WorkspaceId
├── AgentId
├── RootPath
└── ...
```

Ý nghĩa:

> Workspace này có một physical working location trên Agent này.

Ví dụ:

```text
Workspace: Character Production

WorkspaceAgent A
├── Agent = Alice-PC
└── RootPath = D:\Projects\Characters

WorkspaceAgent B
├── Agent = Render-PC
└── RootPath = D:\Studio\Characters
```

Quan hệ:

```text
Workspace N ── N Agent
        thông qua WorkspaceAgent
```

MVP chưa cần access mode, visibility, public/private hay sync status phức tạp.

### Resource

```text
Resource
├── Id
├── WorkspaceId
├── Name
└── ...
```

Resource là identity logic của asset trong Workspace.

Không trỏ trực tiếp tới Agent.

### ResourceVersion

```text
ResourceVersion
├── Id
├── ResourceId
├── Version
├── Hash
└── ...
```

Ý nghĩa:

> Một phiên bản NỘI DUNG cụ thể của Resource, xác định duy nhất bởi `Hash`.
> KHÔNG chứa thông tin về việc nó đang nằm ở đâu — điều đó thuộc về `ResourceVersionLocation`.

`Version` (số thứ tự tăng dần) và `Hash` cùng xác định một ResourceVersion là duy nhất trong phạm vi một Resource. Hai lần scan cho ra cùng Hash (nội dung không đổi) sẽ **không tạo Version mới**, chỉ cập nhật/thêm `ResourceVersionLocation` nếu vị trí phát hiện khác trước.

### ResourceVersionLocation

```text
ResourceVersionLocation
├── Id
├── ResourceVersionId
├── WorkspaceAgentId
├── RelativePath
├── IsOrigin
├── DiscoveredAt
└── ...
```

Ý nghĩa:

> ResourceVersion này đang tồn tại vật lý tại location cụ thể này (WorkspaceAgent + RelativePath).

`WorkspaceAgent.RootPath + ResourceVersionLocation.RelativePath` tạo thành physical path.

`IsOrigin`: đánh dấu location đầu tiên mà ResourceVersion này được phát hiện/tạo ra. Chỉ mang tính thông tin (audit/hiển thị), không ảnh hưởng tính hợp lệ của các Location khác — nếu location gốc bị xóa khỏi đĩa, ResourceVersion vẫn tồn tại hợp lệ miễn còn ít nhất một Location khác.

Quan hệ:

```text
ResourceVersion 1 ── N ResourceVersionLocation
```

Ví dụ minh họa:

```text
Resource: base_body.blend

ResourceVersion v3 (Hash: a1b2c3...)
├── ResourceVersionLocation 1
│   ├── WorkspaceAgent = Alice-PC / D:\Projects\Characters
│   ├── RelativePath = base_body.blend
│   └── IsOrigin = true
│
└── ResourceVersionLocation 2
    ├── WorkspaceAgent = Render-PC / D:\Studio\Characters
    ├── RelativePath = base_body.blend
    └── IsOrigin = false
```

Cả hai location đều là **cùng một ResourceVersion v3** — vì nội dung (Hash) giống hệt nhau, chỉ khác nơi lưu trữ vật lý.

---

## 4. Loại bỏ Platform khỏi Workspace

Xóa:

```text
Workspace.PlatformId
```

Platform vẫn tồn tại ở boundary riêng.

Pipeline Step mới là nơi reference Platform:

```text
PipelineStep
├── PlatformId
└── ...
```

Một Workspace có thể chứa Daz, Blender, Unreal assets, textures và exports cùng lúc.

Platform không còn quyết định Workspace là Workspace của Daz, Blender hay Unreal.

---

## 5. Local Workspace Flow

MVP chỉ tập trung Local Workspace.

```text
User
 ↓
Create Workspace
 ↓
Select Agent
 ↓
Browse / validate filesystem
 ↓
Select RootPath
 ↓
Create Workspace
 ↓
Create WorkspaceAgent
```

Sau đó:

```text
WorkspaceAgent
      ↓
Agent filesystem
      ↓
Scanner
      ↓
Resource / ResourceVersion / ResourceVersionLocation
```

Discovery không upload file.

### Filesystem Browser

Agent là nguồn dữ liệu filesystem:

```text
Frontend
   ↓
Backend
   ↓
Agent
   ↓
Filesystem
```

API tối thiểu:

```text
ListRoots()
ListDirectory(path)
ValidateDirectory(path)
```

Folder tree dùng lazy loading theo directory, không scan toàn bộ filesystem chỉ để render picker.

Tauri chỉ là tiện ích desktop nếu cần native integration; không phải dependency của Workspace architecture.

---

## 6. Resource Sync

```text
WorkspaceAgent
      ↓
Local Scanner
      ↓
Enumerate files
      ↓
Hash
      ↓
Create/update Resource
      ↓
Tìm ResourceVersion theo Hash
      ↓
   Hash đã tồn tại?
      │
      ├── CÓ → chỉ tạo/cập nhật ResourceVersionLocation
      │        (gắn WorkspaceAgent + RelativePath vào ResourceVersion đã có)
      │
      └── KHÔNG → tạo ResourceVersion mới + ResourceVersionLocation (IsOrigin = true)
      ↓
Run Inspector (nếu ResourceVersion là mới hoặc chưa từng inspect)
```

Phân biệt:

```text
Filesystem discovery
= đọc filesystem

Resource synchronization
= cập nhật metadata Resource/Version/Location

File upload
= không mặc định xảy ra
```

**Quy tắc chống trùng Version:** trước khi tạo `ResourceVersion` mới, Scanner luôn tra `Hash` trong phạm vi `Resource` hiện tại. Nếu đã có `ResourceVersion` cùng Hash, chỉ thêm `ResourceVersionLocation` mới (nếu chưa từng có Location tại WorkspaceAgent + RelativePath đó) — không tăng `Version`, không chạy lại Inspector.

---

## 7. File Transfer / Storage Provider

Không tạo `RemoteWorkspace` trong MVP.

Khi cần đưa data sang nơi khác:

```text
Local Workspace
      ↓
Publish / Transfer
      ↓
Storage Provider
      ↓
R2
```

Hoặc sau này:

```text
Agent A
   ↓
Transfer
   ↓
Storage Provider / shared storage
   ↓
Agent B
```

Khi transfer thành công tới một Agent khác trong cùng Workspace, kết quả nên tạo thêm `ResourceVersionLocation` mới cho `ResourceVersion` đã tồn tại (nếu Hash không đổi) thay vì tạo Resource/Version độc lập.

Storage Provider chịu trách nhiệm cho persistence/transport. Workspace không cần biết R2 là gì.

---

## 8. Migration Plan

### Phase 1 — Domain rename/boundary

- [ ] Đổi module/project namespace từ `Automation.Resource` → `Automation.Workspace`.
- [ ] Đổi folder/module boundary tương ứng.
- [ ] Giữ Resource, ResourceVersion và ResourceVersionLocation trong Workspace module.
- [ ] Không thay đổi business behavior ngoài các thay đổi được liệt kê.

### Phase 2 — Workspace model

- [ ] Xóa `PlatformId` khỏi Workspace.
- [ ] Xóa `AgentId` trực tiếp khỏi Workspace nếu đang tồn tại.
- [ ] Xóa `Kind` (Local/R2...) khỏi Workspace nếu đang tồn tại — Workspace là không gian logic thuần túy, không tự nhận là "loại lưu trữ" nào. "Local hay không" là thuộc tính của từng `WorkspaceAgent` (có `RootPath`), không phải của `Workspace`.
- [ ] Xác nhận Workspace chỉ reference `ProjectId`.

### Phase 3 — WorkspaceAgent

- [ ] Tạo entity `WorkspaceAgent`.
- [ ] Tạo relationship `Workspace → WorkspaceAgent → Agent`.
- [ ] Thêm `RootPath`.
- [ ] Tạo migration/database configuration.
- [ ] Thêm use case attach Agent và cấu hình RootPath.
- [ ] Validate Agent tồn tại.
- [ ] Validate RootPath thông qua Agent khi cần.

### Phase 4 — Resource / ResourceVersion / ResourceVersionLocation relationship

- [ ] Đưa Resource vào Workspace boundary.
- [ ] `Resource.WorkspaceId` là ownership relation.
- [ ] `ResourceVersion.ResourceId`.
- [ ] `ResourceVersion.Hash` — **không còn `WorkspaceAgentId` hay `RelativePath` trên ResourceVersion**.
- [ ] Tạo entity `ResourceVersionLocation` với `ResourceVersionId`, `WorkspaceAgentId`, `RelativePath`, `IsOrigin`, `DiscoveredAt`.
- [ ] Ràng buộc unique: `(ResourceId, Hash)` cho ResourceVersion — tránh tạo trùng Version khi nội dung không đổi.
- [ ] Ràng buộc unique: `(WorkspaceAgentId, RelativePath)` cho ResourceVersionLocation — một vị trí vật lý chỉ trỏ về đúng một ResourceVersion tại một thời điểm.
- [ ] Không cho Resource trỏ trực tiếp tới Agent.

### Phase 5 — Resource discovery/sync

- [ ] Scanner nhận `WorkspaceAgentId`.
- [ ] Resolve `Agent + RootPath`.
- [ ] Scan filesystem.
- [ ] Map physical file → Resource.
- [ ] Tính Hash, tra `ResourceVersion` theo `(ResourceId, Hash)`.
- [ ] Nếu chưa có → tạo `ResourceVersion` mới + `ResourceVersionLocation` (`IsOrigin = true`).
- [ ] Nếu đã có → chỉ tạo/cập nhật `ResourceVersionLocation` tương ứng, không tăng Version.
- [ ] Không upload binary file trong local MVP.
- [ ] Trigger Inspector chỉ khi `ResourceVersion` là mới (chưa từng chạy Inspection cho Hash này).

### Phase 6 — API

Refactor endpoint theo boundary mới:

```text
/api/workspaces
/api/workspaces/{workspaceId}
/api/workspaces/{workspaceId}/agents
/api/workspaces/{workspaceId}/resources
/api/resources/{resourceId}/versions
/api/resource-versions/{resourceVersionId}/locations
```

Các endpoint cần filesystem access phải đi qua WorkspaceAgent/Agent.

Không expose filesystem access trực tiếp từ Backend.

### Phase 7 — Frontend

- [ ] Đổi route/menu/module từ Resource → Workspace.
- [ ] Workspace list/detail.
- [ ] WorkspaceAgent management.
- [ ] Chọn Agent.
- [ ] Browse filesystem của Agent.
- [ ] Chọn RootPath.
- [ ] Hiển thị Resource trong Workspace.
- [ ] Hiển thị ResourceVersion (theo Hash/Version, không gắn 1 vị trí cố định).
- [ ] Hiển thị danh sách ResourceVersionLocation của một ResourceVersion (có thể nhiều Agent).
- [ ] Đánh dấu rõ Location nào là `IsOrigin` khi hiển thị.

### Phase 8 — Cleanup

- [ ] Xóa các relation cũ `Workspace → Platform`.
- [ ] Xóa logic `Workspace → Agent` trực tiếp.
- [ ] Xóa `WorkspaceAgentId`/`RelativePath` cũ trên `ResourceVersion` (đã chuyển sang `ResourceVersionLocation`).
- [ ] Xóa assumptions về Remote Workspace.
- [ ] Kiểm tra query/specification/handler/endpoint/frontend đang dùng model cũ.
- [ ] Chạy migration và integration tests.
- [ ] Kiểm tra flow end-to-end, đặc biệt case sync cùng file sang nhiều Agent không tạo Version trùng.

---

## 9. MVP Acceptance Flow

```text
Create Project
    ↓
Create Workspace
    ↓
Attach Agent
    ↓
Select RootPath
    ↓
Create WorkspaceAgent
    ↓
Scan filesystem
    ↓
Resource discovered
    ↓
ResourceVersion created (theo Hash)
    ↓
ResourceVersionLocation created (IsOrigin = true)
    ↓
Inspection executed
    ↓
Resource visible in Workspace
```

Pipeline:

```text
Workspace
   ↓
Resource
   ↓
ResourceVersion
   ↓
ResourceVersionLocation
   ↓
Pipeline
   ↓
Platform
   ↓
Agent
   ↓
WorkspaceAgent
   ↓
Filesystem
```

Scenario chính:

```text
Daz3D → Blender → Unreal
```

có thể chạy trên cùng một Local Workspace và cùng Agent mà **không cần upload file qua R2**.

Chỉ khi pipeline cần đưa data sang location khác mới phát sinh:

```text
Publish / Transfer
        ↓
Storage Provider
```

**Scenario phụ — đồng bộ đa Agent:** nếu cùng một `ResourceVersion` (cùng Hash) được phát hiện tại Alice-PC và sau đó sync thủ công hoặc qua Storage Provider sang Render-PC, hệ thống chỉ thêm một `ResourceVersionLocation` mới, **không tạo Version mới** — giữ đúng lịch sử "đây vẫn là v3, chỉ khác nơi lưu".

---

## 10. Chưa giải quyết trong MVP

- Permission / ACL chi tiết.
- Public/private Workspace.
- Workspace access mode.
- Multi-agent synchronization conflict (VD: cùng RelativePath nhưng nội dung khác nhau giữa 2 Agent — cần chiến lược resolve, chưa thiết kế).
- Distributed cache.
- Remote Workspace.
- R2 integration bắt buộc.
- Binary version storage tập trung.
- Direct Agent-to-Agent transfer optimization.
- Heartbeat/accessibility nâng cao.
- Xử lý khi `ResourceVersionLocation` bị "mồ côi" (file bị xóa khỏi Agent nhưng hệ thống chưa biết) — cần cơ chế re-scan/invalidate, chưa thiết kế trong MVP này.

Mục tiêu của lần cải tổ là **chốt đúng domain boundary và relation**, sau đó mới tiếp tục xây pipeline/storage transfer trên nền model đã rõ ràng.