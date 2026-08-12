# Inspection Module — Plan

## 1. Mục tiêu

Xây dựng module Inspection để:

- Định nghĩa các Inspector mà hệ thống hỗ trợ.
- Định nghĩa version của Inspector implementation.
- Cấu hình rule để tự động chạy Inspector khi Resource phù hợp.
- Lưu kết quả inspection theo từng version/snapshot.
- Cho phép TagLink gắn tag vào một vị trí cụ thể trong kết quả inspection.
- Hỗ trợ re-inspection mà không phá vỡ lịch sử inspection hoặc annotation cũ.

## 2. Mô hình tổng thể

```text
Platform (global)
      │
      └──< PlatformExtension
               │
               │ PlatformExtensionId
               ▼
        InspectorRule ─────────► Inspector
               │                    │
               │                    └── InspectorVersion
               │
               │ match
               ▼
Resource ─────────────────────► Inspection
                                  │
                                  └── TagLink
                                       │
                                       └── Metadata.bindKey
```

| Entity | Responsibility |
|---|---|
| Platform | Catalog global các platform được hệ thống hỗ trợ |
| PlatformExtension | Catalog các extension mà một Platform hỗ trợ |
| Inspector | Định nghĩa một loại inspector |
| InspectorVersion | Phiên bản implementation của Inspector |
| InspectorRule | Cấu hình trong Project để quyết định khi nào Inspector được chạy |
| Inspection | Một snapshot/version của kết quả inspect trên Resource |
| TagLink | Gắn Tag vào một target cụ thể trong Inspection |
| Asset / AssetLink | Quản lý asset vật lý/script được đăng ký và sử dụng |

## 3. Platform

Platform là **global catalog**, không thuộc riêng Project.

Ví dụ:

- Blender
- Daz Studio
- Unreal Engine

Platform đại diện cho hệ sinh thái/capability mà hệ thống hỗ trợ và có thể liên quan tới worker/adapter tương ứng.

Platform cũng sở hữu catalog các extension mà nó hỗ trợ thông qua `PlatformExtension`.

```text
Platform
├── Blender
│   ├── .blend
│   └── .blend1
├── Daz Studio
│   ├── .duf
│   └── .dsf
└── Unreal Engine
    ├── .uasset
    └── .umap
```

Một extension có thể được nhiều Platform hỗ trợ, vì vậy `.fbx` có thể xuất hiện dưới nhiều Platform khác nhau.

Project không tạo Platform hoặc PlatformExtension mới trong phạm vi Inspector Rule; Project chỉ reference catalog global.

## 4. PlatformExtension

`PlatformExtension` là catalog global thuộc Platform, đại diện cho một file extension mà Platform có thể nhận diện/xử lý.

```text
PlatformExtension
├── Id
├── PlatformId
├── Extension
└── ...
```

Ví dụ:

```text
Blender + .blend
Blender + .fbx
Unreal Engine + .fbx
```

Extension không còn là raw string được cấu hình trực tiếp trên `InspectorRule`.

## 5. Inspector

Inspector đại diện cho một khả năng phân tích.

Ví dụ:

- Blender Object Inspector
- Blender Material Inspector
- Daz Object Inspector

```text
Inspector
├── Id
├── Key
├── Name
└── Description?
```

Inspector không chứa trực tiếp script implementation.

Một Inspector có nhiều InspectorVersion.

## 6. InspectorVersion

InspectorVersion đại diện cho một phiên bản implementation cụ thể của Inspector.

```text
InspectorVersion
├── Id
├── InspectorId
├── Version
├── AssetLink
├── EntryPoint
├── CreatedAt
└── ...
```

### AssetLink

InspectorVersion **không trực tiếp sở hữu `FileId`**.

Script/implementation được quản lý thông qua cơ chế Asset/AssetLink hiện có của hệ thống.

InspectorVersion chỉ đăng ký và sử dụng asset implementation thông qua AssetLink.

## 7. InspectorRule

InspectorRule là cấu hình **project-level**:

> Khi Resource thỏa các điều kiện này thì tự động chạy Inspector nào.

```text
InspectorRule
├── Id
├── ProjectId
├── PlatformExtensionId
├── ContentTypeId?
├── InspectorId
├── Enabled
└── ...
```

- `ProjectId`: rule thuộc Project nào.
- `PlatformExtensionId`: extension thuộc Platform global nào và format nào được match.
- `ContentTypeId?`: nullable; null nghĩa là áp dụng cho mọi Content Type trong Project.
- `InspectorId`: Inspector sẽ được chạy.
- `Enabled`: bật/tắt rule.

Ví dụ:

```text
Project A
├── Blender + .blend + Character
│      → Blender Character Inspector
├── Blender + .blend + All Content Types
│      → Blender Object Inspector
└── Daz + .duf + Character
       → Daz Character Inspector
```

Một Rule chỉ trỏ tới **một Inspector**. Nếu cùng điều kiện cần chạy nhiều Inspector thì tạo nhiều Rule.

## 8. Inspection

Inspection là **một snapshot cụ thể của kết quả Inspector chạy trên Resource**.

Không tạo entity/bảng `InspectionVersion` riêng.

```text
Inspection
├── Id
├── ResourceId
├── InspectorVersionId
├── Version
├── Data / Value (JSONB)
├── CreatedAt
└── ...
```

Hai loại version cần phân biệt:

- `InspectorVersion.Version`: implementation/script của Inspector ở version nào.
- `Inspection.Version`: Resource đã được inspect bao nhiêu lần.

Ví dụ:

```text
Resource: Eva.blend

Blender Object Inspector
├── Inspection v1 → InspectorVersion v1
├── Inspection v2 → InspectorVersion v1
└── Inspection v3 → InspectorVersion v2
```

`Inspection` chỉ cần reference `InspectorVersionId`.

Không cần `Inspection → InspectorId` hay `Inspection → FileId`.

## 9. TagLink trên Inspection

Tag có thể được gắn vào nhiều vị trí khác nhau trong cùng một Inspection.

Mỗi vị trí là **một TagLink riêng**.

```text
TagLink
├── TagId
├── InspectionId
└── Metadata (JSONB)
      └── bindKey
```

Ví dụ:

```json
{
  "bindKey": "objects[2].name"
}
```

Nếu cùng một Tag gắn vào ba object:

```text
TagLink #1 → objects[0].name
TagLink #2 → objects[2].name
TagLink #3 → objects[7].name
```

Remove một target = xóa đúng TagLink đó.

## 10. Re-inspection và Tag preservation

Inspection không cần đảm bảo identity của từng phần tử trong JSON giữa các lần chạy.

Ví dụ v1:

```text
objects:
[
  Body,
  Hair,
  Dress
]
```

Tag:

```text
Hair → objects[1].name
```

Sau khi chạy lại:

```text
objects:
[
  Body,
  Dress,
  Hair
]
```

Không cố coi `objects[1]` là identity bất biến.

Khi Inspection có TagLink:

1. Phát hiện annotation cần preserve.
2. Hỏi user có muốn giữ các tag hay không.
3. Tạo Inspection version mới.
4. Resolve target tương ứng trong kết quả mới.
5. Tạo TagLink mới với `InspectionId` và `bindKey` mới.
6. Target không còn tồn tại thì không tạo TagLink mới.
7. Inspection và TagLink cũ vẫn giữ nguyên.

```text
Inspection v1
  Hair → objects[1]
        │
        │ re-inspect + preserve
        ▼
Inspection v2
  Hair → objects[2]
```

Không cần global identity tracking cho mọi object.

## 11. Automatic inspection flow

```text
Resource created/imported
        │
        ▼
Match InspectorRule
        │
        ├── Project
        ├── PlatformExtension
        └── ContentType?
        │
        ▼
Inspector
        │
        ▼
Published InspectorVersion
        │
        ▼
Worker / Script Adapter
        │
        ▼
Inspection vN
        │
        └── Data / Value JSONB
```

## 12. Inspector script lifecycle

Khi implementation thay đổi:

```text
Inspector
├── v1 → AssetLink A
└── v2 → AssetLink B
```

Không overwrite version cũ.

```text
Edit / register new script
        ↓
Create InspectorVersion
        ↓
Associate AssetLink
        ↓
Validate / test
        ↓
Publish
        ↓
New executions use published version
```

Inspection cũ vẫn reference đúng `InspectorVersionId`.

## 13. Không thuộc phạm vi MVP

Chưa cần thêm:

- InspectionVersion entity riêng.
- InspectionProfile.
- InspectorExecution/Run entity.
- InspectorTarget entity.
- Một Rule chứa nhiều Inspector.
- Global object identity tracking.
- Complex visual rule builder.
- Tự động migrate mọi TagLink mà không cần user confirmation.

## 14. Constraint / consistency đề xuất

### Inspector

```text
UNIQUE(Key)
```

### InspectorVersion

```text
UNIQUE(InspectorId, Version)
```

### PlatformExtension

```text
UNIQUE(PlatformId, Extension)
```

### InspectorRule

Một Rule biểu diễn một điều kiện → một Inspector.

Candidate unique key:

```text
ProjectId
+ PlatformExtensionId
+ ContentTypeId
+ InspectorId
```

Cần xử lý rõ semantics của nullable `ContentTypeId` trong PostgreSQL trước khi migration.

### Inspection

Version phải duy nhất trong cùng context của Resource và Inspector. Constraint cụ thể nên chốt cùng schema thực tế.

## 15. Boundary cuối cùng

```text
Platform
    = hệ thống/platform mà Automation Studio hỗ trợ

PlatformExtension
    = extension được một Platform hỗ trợ

Inspector
    = khả năng phân tích

InspectorVersion
    = implementation version cụ thể

InspectorRule
    = Project muốn tự động chạy Inspector nào khi nào

Inspection
    = snapshot kết quả của một lần inspect

TagLink
    = annotation của user vào một vị trí trong Inspection

Asset / AssetLink
    = physical asset và cơ chế đăng ký/sử dụng asset
```

Đây là model tối giản cho MVP; chỉ thêm abstraction khi có nhu cầu thực tế buộc phải tách.
