namespace Automation.Pipeline.Engine.StructRegistry;

public interface IEntityStructRegistry
{
    IEntityStructDefinition? Get(string structType);
    IReadOnlyList<IEntityStructDefinition> GetAll();
}

public class EntityStructRegistry(IEnumerable<IEntityStructDefinition> structDefinitions) : IEntityStructRegistry
{
    private readonly Dictionary<string, IEntityStructDefinition> _registry =
        structDefinitions.ToDictionary(x => x.StructType, StringComparer.OrdinalIgnoreCase);

    public IEntityStructDefinition? Get(string structType)
    {
        return !string.IsNullOrWhiteSpace(structType) && _registry.TryGetValue(structType, out var def)
            ? def
            : null;
    }

    public IReadOnlyList<IEntityStructDefinition> GetAll() => _registry.Values.ToList();
}
