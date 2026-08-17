namespace Automation.Inspection.Domain.Entities;

public class Inspector : BaseEntity<Guid>
{
    public Guid ProjectId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string ExecutorKey { get; private set; } = string.Empty;
    private readonly List<InspectorVersion> _versions = new();
    public IReadOnlyList<InspectorVersion> Versions => _versions.AsReadOnly();

    protected Inspector() { }

    public Inspector(
        Guid projectId,
        string key,
        string name,
        string executorKey,
        string? description = null
    )
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Key = key;
        Name = name;
        ExecutorKey = executorKey.ToLowerInvariant();
        Description = description;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public static Inspector Create(
        Guid projectId,
        string key,
        string name,
        string executorKey,
        string entryPoint,
        string scriptHash,
        string? description = null
    )
    {
        var inspector = new Inspector(projectId, key, name, executorKey, description);
        inspector.AddNewVersion(entryPoint, scriptHash);
        return inspector;
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void SetPublishedVersion(Guid versionId)
    {
        foreach (var v in _versions)
        {
            v.SetPublished(false);
        }
        _versions.First(x => x.Id == versionId).SetPublished(true);
    }

    public InspectorVersion? GetPublishedVersion()
    {
        return _versions.FirstOrDefault(x => x.IsPublished) ?? _versions.LastOrDefault();
    }

    public void AddNewVersion(
        string entryPoint,
        string scriptHash,
        bool isNewVersionPublished = true
    )
    {
        var newVersion = new InspectorVersion(Id, _versions.Count + 1, entryPoint, scriptHash);
        _versions.Add(newVersion);

        if (isNewVersionPublished)
            SetPublishedVersion(newVersion.Id);
    }
}
