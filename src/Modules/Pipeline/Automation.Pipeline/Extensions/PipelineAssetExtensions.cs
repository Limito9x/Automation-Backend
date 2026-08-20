using Automation.Files.Contracts;
using Automation.Pipeline.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Pipeline.Extensions;

public static class PipelineAssetExtensions
{
    public static IServiceCollection AddPipelineAssetSlots(this IServiceCollection services)
    {
        services.AddAssetSlot(
            entityType: "NodeDefinition",
            slotKey: PipelineAssetSlots.CustomScript,
            options: new AssetCategoryOptions
            {
                AllowMultiple = false,
                MaxCount = 1,
                MaxSizeBytes = 50 * 1024 * 1024, // 50MB
                AllowedContentTypes = []
            }
        );
        return services;
    }
}
