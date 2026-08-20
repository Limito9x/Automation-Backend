namespace Automation.Pipeline.Tools;

public class ToolRegistry(IEnumerable<IResolverTool> tools) : IToolRegistry
{
    private readonly IReadOnlyDictionary<string, IResolverTool> _tools =
        tools.ToDictionary(t => t.Key, StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<IResolverTool> GetAll() => _tools.Values.ToList();

    public IResolverTool? GetByKey(string key) => _tools.GetValueOrDefault(key);
}
