using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Automation.Resource.Infrastructure.Persistence;

namespace Automation.Resource;

public sealed class ResourceModule : IModule, IPermissionModule
{
    public string Name => "Resource";
    public string SchemaName => "resource";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<ResourceDbContext>(config, SchemaName);
        services.AddResourceAssetSlots();
        services.AddScoped<Automation.SharedKernel.Abstractions.Auth.ICurrentAgent, Automation.SharedKernel.Infrastructure.Auth.CurrentAgent>();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.ResourcePermissions().GetPermissions();

    public List<Type> Endpoints => [];
}
