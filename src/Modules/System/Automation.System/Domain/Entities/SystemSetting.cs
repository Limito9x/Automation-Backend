using Automation.SharedKernel.Domain.Entities;

namespace Automation.SystemModule.Domain.Entities;

public class SystemSetting : AuditableEntity<Guid>
{
    public string Key { get; private set; } = default!;
    public string Value { get; private set; } = default!;
    public string ValueType { get; private set; } = default!;
    public string? Description { get; private set; }
    
    private SystemSetting() { } // EF Core
    
    public SystemSetting(string key, string value, string valueType, string? description)
    {
        Id = Guid.NewGuid();
        Key = key;
        Value = value;
        ValueType = valueType;
        Description = description;
    }
    
    public void UpdateValue(string value)
    {
        Value = value;
    }
}


