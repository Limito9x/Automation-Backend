namespace Automation.Identity.Features.Users.CreateUser;

internal class CreateUserEndpoint(IMessageBus bus) : Endpoint<CreateUserCommand, Guid>
{
    public override void Configure()
    {
        Post("/");
        Group<UsersGroup>();
        Permissions(P.Users.Create);
    }

    public override async Task HandleAsync(CreateUserCommand req, CancellationToken ct)
    {
        var result = await bus.InvokeAsync<Result<Guid>>(req, ct);
        await this.SendResultAsync(result, ct);
    }
}

