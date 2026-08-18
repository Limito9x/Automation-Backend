using Automation.Inspection.Shared.Dtos;
using Automation.Tag.Contracts.Dtos;

namespace Automation.Inspection.Features.Inspections.GetInspectionsByResourceVersion;

public record GetInspectionsByResourceVersionQuery(Guid ResourceVersionId);

public record InspectionDetailDto(
    InspectionDto Inspection,
    Dictionary<string, IReadOnlyList<TagLinkDetailDto>> TagMap // 1 field trong inspection chứa các tag link
);
