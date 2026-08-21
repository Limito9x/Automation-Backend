# Thiết Kế Hệ Thống Dynamic Data Nodes, Struct Break & Naming Conventions Cho Visual Pipeline

Tài liệu này định hình cấu trúc và kế hoạch triển khai bộ công cụ **Pure Data Nodes (Node Dữ Liệu Thuần)**, **Struct Break Nodes (Phân Rã DTO)** và **Convention Builders** nhằm nâng cấp hệ thống Automation Pipeline lên chuẩn Enterprise và tái sử dụng 100% cho mọi 3D Asset.

---

## 1. Triết Lý Thiết Kế (Architectural Philosophy)

Dựa trên tiêu chuẩn đồ thị chuyên nghiệp (Unreal Engine Blueprint, Blender Geometry Nodes, Houdini):

```
┌──────────────────────────────────────────────────────────┐
│ 1. PURE DATA NODES (Không có Exec Pins)                  │
│    - Nhiệm vụ: Tra cứu DB, trích xuất DTO, ghép chuỗi.    │
│    - Thực thi: Chạy tức thì trên .NET / In-Memory State. │
└────────────────────────────┬─────────────────────────────┘
                             │ (Piped Output Pins)
                             ▼
┌──────────────────────────────────────────────────────────┐
│ 2. CONTROL / ACTION NODES (Có Exec Pins Trắng)           │
│    - Nhiệm vụ: Import, Bake, Pack UV, Save Blend, Export │
│    - Thực thi: Agent Worker (Blender/Python Subprocess)  │
└──────────────────────────────────────────────────────────┘
```

---

## 2. Danh Mục Các Dynamic Node Cần Triển Khai

### 📦 Nhóm 1: Resource & Content Extraction Nodes

#### 1.1. `Break Resource Info`
- **Mục đích**: Nhận diện thông tin vật lý của file 3D trong Workspace.
- **Input Pin**:
  - `Resource` (`EntityRef` hoặc `Path`)
- **Output Pins (Tách trường DTO)**:
  - 🟢 `ResourceId` (`String` / `Guid`)
  - 🟢 `FileName` (`String` - ví dụ: `"Maria_HD.duf"`)
  - 🟢 `BaseName` (`String` - ví dụ: `"Maria_HD"` - đã cắt bỏ extension)
  - 🟢 `Extension` (`String` - ví dụ: `".duf"`)
  - 🟢 `DirectoryPath` (`Path` - thư mục chứa file)
  - 🟢 `AbsolutePath` (`Path` - đường dẫn đầy đủ)
  - 🟢 `AttachedContentId` (`EntityRef` - Content Item gắn kèm nếu có)

#### 1.2. `Get Attached Content` / `Break Content`
- **Mục đích**: Lấy các trường Metadata nghiệp vụ (Character Name, Generation, Category, Dynamic Form Fields) gắn liền với file 3D.
- **Input Pin**:
  - `Resource` (`EntityRef`) hoặc `ContentId` (`Guid`)
- **Output Pins**:
  - 🟢 `ContentName` (`String`)
  - 🟢 `ContentType` (`String` - `"Character"`, `"Clothing"`, `"Prop"`)
  - 🟢 `Properties` (`JSON` - toàn bộ Dynamic Form values)
  - 🟢 `Tags` (`Array[String]`)
  - 🟢 `CreatedBy` / `CreatedAt`

---

### 🔍 Nhóm 2: Inspection & 3D Geometry Metadata Nodes

#### 2.1. `Get Inspection Data` / `Break Inspection`
- **Mục đích**: Trích xuất kết quả phân tích hình học sâu của file 3D từ module Inspection mà không cần mở Blender để quét lại.
- **Input Pin**:
  - `Target` (`EntityRef` Resource hoặc `Path` file 3D)
- **Output Pins**:
  - 🟢 `Status` (`String` - `"Passed"`, `"Failed"`, `"Warning"`)
  - 🟢 `MainObjects` (`Array[String]` - Danh sách tên Meshes / Figures mục tiêu)
    > 💡 **Điểm mấu chốt**: Output này nối thẳng trực tiếp vào Pin `target_objects` của Node `Native Bake` và `Generate UV`!
  - 🟢 `SkeletonBones` (`Array[String]` - Danh sách xương)
  - 🟢 `Dependencies` (`Array[Path]` - Các file `.dsf`, `.dbz` phụ thuộc)
  - 🟢 `MaterialsCount` (`Number`)
  - 🟢 `Summary` (`String`)

---

### 🏷️ Nhóm 3: Tag & Metadata Filtering Nodes

#### 3.1. `Get Tag Value`
- **Mục đích**: Đọc giá trị của một Tag cụ thể được gắn trên Entity (Resource / Content / Inspection).
- **Input Pins**:
  - `Entity` (`EntityRef`)
  - `TagKey` (`String` - ví dụ: `"Gender"`, `"LOD"`, `"BodyType"`)
- **Output Pins**:
  - 🟢 `TagValue` (`String` / `Any`)
  - 🟢 `Exists` (`Boolean`)

#### 3.2. `Branch By Tag` (Control Flow Node)
- **Input Pins**: `Exec In`, `Entity`, `TagKey`, `ExpectedValue`
- **Output Pins**: `Exec True`, `Exec False`

---

### 🛠️ Nhóm 4: Convention Builders & Path Formatter Nodes (UE5 Style)

#### 4.1. `Build Naming Pattern` / `Format String`
- **Mục đích**: Tự động sinh tên file chuẩn theo quy ước công ty (Game Convention Naming).
- **Input Pins**:
  - `FormatTemplate` (`String` - ví dụ: `"SK_{Category}_{CharacterName}_{LOD}"`)
  - `Variables` (`JSON` hoặc Dynamic Pins)
- **Output Pins**:
  - 🟢 `ResultString` (`String` - ví dụ: `"SK_Character_Maria_LOD0"`)

#### 4.2. `Combine Path`
- **Mục đích**: Ghép đường dẫn an toàn theo chuẩn hệ điều hành.
- **Input Pins**:
  - `BasePath` (`Path` - ví dụ: `"D:/Games/Projects"`)
  - `SubFolder` (`String` - ví dụ: `"Textures"`)
  - `FileName` (`String` - ví dụ: `"Maria_Diffuse"`)
  - `Extension` (`String` - ví dụ: `".png"`)
- **Output Pins**:
  - 🟢 `FullPath` (`Path` - `"D:/Games/Projects/Textures/Maria_Diffuse.png"`)

#### 4.3. `Append String (Variadic Pin)`
- **Mục đích**: Nối các đoạn chuỗi với nhau.
- **Tính năng UI**: Nút **`[ + Add Pin ]`** cho phép thêm không giới hạn các trường `A`, `B`, `C`, `D`... và tùy chọn `Delimiter` (như `_` hoặc `-`).

---

## 3. Cơ Chế Thực Thi & Tối Ưu Hóa Dữ Liệu (Execution Pipeline)

```
┌───────────────────────────────────────────────────────────────┐
│ Bước 1: User bấm Run Pipeline trên Frontend                   │
│         - Chỉ cần chọn 1 Resource File: "Maria.duf"           │
└──────────────────────────────┬────────────────────────────────┘
                               │
                               ▼
┌───────────────────────────────────────────────────────────────┐
│ Bước 2: Backend Resolver (.NET Engine)                        │
│         1. Node "Break Resource" -> Trích xuất "Maria", ".duf"│
│         2. Node "Get Inspection" -> Query DB lấy MainObjects  │
│            ["Maria HD", "Maria Hair", "Eyelashes"]            │
│         3. Node "Combine Path"   -> Sinh "D:/Output/Maria.blend│
│         * Toàn bộ diễn ra trong RAM .NET (< 5ms)              │
└──────────────────────────────┬────────────────────────────────┘
                               │
                               ▼
┌───────────────────────────────────────────────────────────────┐
│ Bước 3: Đóng gói Stage Task gửi sang Agent Worker (Blender)   │
│         - Task chỉ chứa các Control Nodes thực sự:            │
│           Step 1: Diffeomorphic Import ("Maria.duf")          │
│           Step 2: Generate UV ($ref.objects)                  │
│           Step 3: Native Bake (target_objects: ["Maria HD..."])│
│           Step 4: Save Blend (path: "D:/Output/Maria.blend")  │
└───────────────────────────────────────────────────────────────┘
```

---

## 4. Kế Hoạch Triển Khai (Action Plan)

### Giai Đoạn 1: Backend Pure Data Resolvers
- [ ] Bổ sung các Built-in Data Handlers trong `Automation.Pipeline/Engine/BuiltIn/`:
  - `ResourceInfoResolver`: Tích hợp `IWorkspaceApi` lấy metadata file.
  - `InspectionDataResolver`: Tích hợp `IInspectionApi` giải nén `MainObjects[]`, `Bones[]`.
  - `TagValueResolver`: Tích hợp `ITagApi`.
- [ ] Cập nhật `InputResolver.cs` để tự động kích hoạt Pure Handlers khi resolve DAG.

### Giai Đoạn 2: Frontend Canvas UI (Variadic Pins & Break Struct Badges)
- [ ] Thêm cờ `isPure: true` cho Node Definition để ẩn thanh Header Exec Pins trên Canvas.
- [ ] Xây dựng tính năng **Dynamic / Variadic Pins** (Nút `+ Add Pin` / `- Remove Pin` trên các Node chuỗi & mảng).
- [ ] Cập nhật Node Library hiển thị nhóm danh mục `Data / Inspection / Utilities`.
