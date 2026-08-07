namespace Automation.Inspection.Domain.Entities;

public class Inspector : BaseEntity<Guid>
{
    public string Key { get; private set; } = string.Empty;
    public string PlatformKey { get; private set; } = string.Empty;
    public string SupportedExtension { get; private set; } = string.Empty;
    public string ScriptPath { get; private set; } = string.Empty;
    public string PrimaryFieldPath { get; private set; } = string.Empty;

    protected Inspector() { }

    public Inspector(string key, string platformKey, string supportedExtension, string scriptPath, string primaryFieldPath)
    {
        Id = Guid.NewGuid();
        Key = key;
        PlatformKey = platformKey;
        SupportedExtension = supportedExtension;
        ScriptPath = scriptPath;
        PrimaryFieldPath = primaryFieldPath;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
