namespace Automation.Pipeline.Tools;

public class ToolRegistry(IEnumerable<IResolverTool> tools) : IToolRegistry
{
    private readonly IReadOnlyDictionary<string, IResolverTool> _tools =
        tools
            .GroupBy(t => t.Key, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last(), StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<IResolverTool> GetAll() => _tools.Values.ToList();

    public IResolverTool? GetByKey(string key) => _tools.GetValueOrDefault(key);
}
