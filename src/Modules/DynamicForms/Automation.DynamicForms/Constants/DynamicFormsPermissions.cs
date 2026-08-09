using Automation.SharedKernel.Abstractions.Auth;

namespace Automation.DynamicForms.Constants;

public class DynamicFormsPermissions
{
    // 1. Khai b�o instance
    // public static SampleFeature Sample { get; } = new();

    // 2. Th�m v�o GetPermissions dictionary
    public Dictionary<string, IReadOnlyList<string>> GetPermissions() => new()
    {
        // { "Sample", Sample.All }
    };

    // 3. Khai b�o c?u tr�c quy?n
    // public class SampleFeature() : BaseCrudPermission("sample") { }
}
