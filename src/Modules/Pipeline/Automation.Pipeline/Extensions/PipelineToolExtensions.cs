using Automation.Pipeline.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Pipeline.Extensions;

public static class PipelineToolExtensions
{
    public static IServiceCollection AddPipelineTools(this IServiceCollection services)
    {
        var toolTypes = typeof(PipelineModule).Assembly
            .GetTypes()
            .Where(t => typeof(IResolverTool).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false });

        foreach (var toolType in toolTypes)
        {
            services.AddScoped(typeof(IResolverTool), toolType);
        }

        services.AddScoped<IToolRegistry, ToolRegistry>();

        return services;
    }
}
