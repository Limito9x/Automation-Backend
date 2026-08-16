using Automation.Files.Contracts;
using Automation.Inspection.Constants;
using Microsoft.Extensions.DependencyInjection;

namespace Automation.Inspection.Extensions;

public static class InspectionAssetExtensions
{
    public static IServiceCollection AddInspectionAssetSlots(this IServiceCollection services)
    {
        services.AddAssetSlot(
            entityType: "InspectorVersion",
            slotKey: InspectionAssetSlots.Script,
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
