using Automation.Inspection.Contracts.Dtos;
using FluentResults;

namespace Automation.Inspection.Contracts;

public interface IInspectionApi
{
    /// <summary>
    /// Lấy danh sách kết quả inspection chi tiết kèm TagMap cho một ResourceVersionId
    /// </summary>
    Task<Result<IReadOnlyList<InspectionDetailDto>>> GetInspectionsByResourceVersionAsync(
        Guid resourceVersionId,
        CancellationToken ct = default
    );

    /// <summary>
    /// Lấy thông tin inspection chi tiết kèm TagMap theo InspectionId
    /// </summary>
    Task<Result<InspectionDetailDto>> GetInspectionWithTagsAsync(
        Guid inspectionId,
        CancellationToken ct = default
    );

    /// <summary>
    /// Lấy kết quả inspection mới nhất của một ResourceVersion theo InspectorId
    /// </summary>
    Task<Result<InspectionDetailDto>> GetLatestInspectionByInspectorAsync(
        Guid resourceVersionId,
        Guid inspectorId,
        CancellationToken ct = default
    );
}
