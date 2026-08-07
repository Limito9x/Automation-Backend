using Automation.Files.Contracts;
using FluentResults;

namespace Automation.Files.Features.Assets.RequestUpload;

public class RequestUploadHandler(IAssetApi assetApi)
{
    public async Task<Result<IEnumerable<AssetUploadDto>>> HandleAsync(RequestUploadCommand command, CancellationToken ct)
    {
        return await assetApi.RequestUploadAsync(command.Items, ct);
    }
}


