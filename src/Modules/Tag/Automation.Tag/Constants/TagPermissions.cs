using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.Tag.Constants;

public class TagPermissions
{
    // 1. Khai báo instance
    // public static SampleFeature Sample { get; } = new();

    // 2. Thêm vào GetPermissions dictionary
    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        // { "Sample", Sample.All }
    };

    // 3. Khai báo cấu trúc quyền
    // public class SampleFeature() : BaseCrudPermission("sample") { }
}

