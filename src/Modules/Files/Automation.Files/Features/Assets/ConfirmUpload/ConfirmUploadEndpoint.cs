using Automation.SharedKernel.Extensions.Results;
using FastEndpoints;
using Wolverine;

namespace Automation.Files.Features.Assets.ConfirmUpload;

public class ConfirmUploadEndpoint(IMessageBus bus) : Endpoint<ConfirmUploadCommand>
{
    public override void Configure()
    {
        Post("confirm-upload");
        Group<AssetsGroup>();
        AllowAnonymous(); // Depending on auth setup
    }

    public override async Task HandleAsync(ConfirmUploadCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<FluentResults.Result>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

