using Automation.Content.Shared.Dtos;

namespace Automation.Content.Features.ContentTypes.CreateContentType;

internal class CreateContentTypeEndpoint(IMessageBus bus)
    : Endpoint<CreateContentTypeCommand, ContentTypeDto>
{
    public override void Configure()
    {
        Post("/"); // Change this method/route accordingly
        Group<ContentTypesGroup>();
        Permissions(P.ContentType.Create);
        Description(x => x.WithName("CreateContentType"));
    }

    public override async Task HandleAsync(
        CreateContentTypeCommand req,
        CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<ContentTypeDto>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}
