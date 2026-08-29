using Automation.Pipeline.Engine.StructRegistry;

namespace Automation.Pipeline.Tools;

public interface IPinResolutionContext
{
    IEntityStructRegistry? StructRegistry { get; }
}

public sealed class PinResolutionContext(IEntityStructRegistry? structRegistry) : IPinResolutionContext
{
    public IEntityStructRegistry? StructRegistry => structRegistry;
}
