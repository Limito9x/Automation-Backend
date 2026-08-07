using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentTypes.GetContentTypeById;

internal class GetContentTypeByIdEndpoint(IMessageBus bus)
    : Endpoint<GetContentTypeByIdQuery, ContentTypeDto>
{
    public override void Configure()
    {
        Get("/{id}");
        Group<ContentTypesGroup>();
        Permissions(P.ContentType.GetById);
        Description(x => x.WithName("GetContentTypeById"));
    }

    public override async Task HandleAsync(
        GetContentTypeByIdQuery req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ContentTypeDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
