param (
    [Parameter(Mandatory=$true)]
    [string]$NewName
)

$OldName = "Automation"

Write-Host "🚀 Đang tiến hành đổi tên dự án từ '$OldName' sang '$NewName'..." -ForegroundColor Cyan

# 1. Thay thế nội dung trong các file text (.cs, .csproj, .sln, .ps1, .json, .yml, .md, .dockerignore)
$extensions = "*.cs", "*.csproj", "*.sln", "*.ps1", "*.json", "*.yml", "*.yaml", "*.md", "*.dockerignore", "*.sbn", "*.hbs", "*.txt"
Get-ChildItem -Recurse -Include $extensions -Exclude bin, obj, .git, .vs, .idea, .codegraph, .agents, .ai | ForEach-Object {
    $content = Get-Content $_.FullName -Raw -Encoding UTF8
    if ($content -match $OldName) {
        $newContent = $content -replace $OldName, $NewName
        Set-Content -Path $_.FullName -Value $newContent -Encoding UTF8
        Write-Host "  [Updated Content] $($_.Name)" -ForegroundColor Gray
    }
}

# 2. Đổi tên các FILE chứa tên dự án (ví dụ Automation.Api.csproj -> NewName.Api.csproj)
Get-ChildItem -Recurse -Include "*$OldName*" -Exclude bin, obj, .git, .vs, .idea, .codegraph, .agents, .ai | Where-Object { -not $_.PSIsContainer } | ForEach-Object {
    $newFileName = $_.Name -replace $OldName, $NewName
    Rename-Item -Path $_.FullName -NewName $newFileName
    Write-Host "  [Renamed File] $($_.Name) -> $newFileName" -ForegroundColor Yellow
}

# 3. Đổi tên các THƯ MỤC chứa tên dự án (Đổi từ thư mục sâu nhất lên thư mục nông)
Get-ChildItem -Recurse -Directory -Exclude bin, obj, .git, .vs, .idea, .codegraph, .agents, .ai | 
    Where-Object { $_.Name -like "*$OldName*" } | 
    Sort-Object { $_.FullName.Length } -Descending | ForEach-Object {
        $newDirName = $_.Name -replace $OldName, $NewName
        Rename-Item -Path $_.FullName -NewName $newDirName
        Write-Host "  [Renamed Directory] $($_.Name) -> $newDirName" -ForegroundColor Green
    }

Write-Host "`n✅ ĐÃ HOÀN TẤT ĐỔI TÊN THÀNH CÔNG THÀNH '$NewName'!" -ForegroundColor Green
Write-Host "👉 Bạn có thể mở file solution mới và chạy 'dotnet build' ngay bây giờ." -ForegroundColor Cyan


