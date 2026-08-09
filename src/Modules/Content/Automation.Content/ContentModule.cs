using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Automation.Content.Infrastructure.Persistence;
using Automation.DynamicForms.Contracts;

namespace Automation.Content;

public sealed class ContentModule : IModule, IPermissionModule
{
    public string Name => "Content";
    public string SchemaName => "content";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<ContentDbContext>(config, SchemaName);
        services.AddDynamicSchema("ContentType");
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.ContentPermissions().GetPermissions();

    public List<Type> Endpoints => [..DiscoveredTypes.All];
}
