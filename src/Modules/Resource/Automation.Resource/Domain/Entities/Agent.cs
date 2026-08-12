namespace Automation.Resource.Domain.Entities;

public class Agent : BaseEntity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string MachineKey { get; private set; } = string.Empty;
    public string RegistrationToken { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset? LastSeenAt { get; private set; }

    public ICollection<Workspace> Workspaces { get; private set; } = new List<Workspace>();

    protected Agent() { }

    public Agent(string name, string machineKey, string registrationToken)
    {
        Id = Guid.NewGuid();
        Name = name;
        MachineKey = machineKey;
        RegistrationToken = registrationToken;
        IsActive = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public void UpdateLastSeen()
    {
        LastSeenAt = DateTimeOffset.UtcNow;
    }

    public void SetActiveStatus(bool isActive)
    {
        IsActive = isActive;
    }
}
