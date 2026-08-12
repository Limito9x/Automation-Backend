using System.Text.Json;

namespace Automation.Pipeline.Domain.Entities;

public class ToolDefinition : BaseEntity<Guid>
{
    public string Key { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public JsonDocument InputPins { get; private set; } = null!;
    public JsonDocument OutputPins { get; private set; } = null!;
    public string HandlerKey { get; private set; } = string.Empty;

    protected ToolDefinition() { }

    public ToolDefinition(string key, string name, JsonDocument inputPins, JsonDocument outputPins, string handlerKey)
    {
        Id = Guid.NewGuid();
        Key = key;
        Name = name;
        InputPins = inputPins;
        OutputPins = outputPins;
        HandlerKey = handlerKey;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

