using Automation.Tag.Shared.Dtos;

namespace Automation.Tag.Features.Tags.UpdateTag;

public class UpdateTagEndpoint(IMessageBus bus) : Endpoint<UpdateTagCommand, TagItemDto>
{
    public override void Configure()
    {
        Put("/tags/{id:guid}");
        Permissions(P.Tag.Update);
    }

    public override async Task HandleAsync(UpdateTagCommand command, CancellationToken ct)
    {
        command = command with { Id = Route<Guid>("id") };
        var result = await bus.InvokeAsync<Result<TagItemDto>>(command, ct);
        await this.SendResultAsync(result, ct);
    }
}