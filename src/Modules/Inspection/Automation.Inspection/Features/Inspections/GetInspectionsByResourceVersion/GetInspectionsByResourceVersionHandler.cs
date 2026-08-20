using Wolverine.Attributes;

namespace Automation.Inspection.Features.Inspections.GetInspectionsByResourceVersion;

[NonTransactional]
public class GetInspectionsByResourceVersionHandler(IInspectionApi inspectionApi)
{
    public async Task<Result<IReadOnlyList<InspectionDetailDto>>> HandleAsync(
        GetInspectionsByResourceVersionQuery query,
        CancellationToken ct
    )
    {
        return await inspectionApi.GetInspectionsByResourceVersionAsync(query.ResourceVersionId, ct);
    }
}
