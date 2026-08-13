using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Automation.Workspace.Infrastructure.Persistence;

namespace Automation.Workspace;

public sealed class WorkspaceModule : IModule, IPermissionModule
{
    public string Name => "Resource";
    public string SchemaName => "resource";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<WorkspaceDbContext>(config, SchemaName);
        services.AddResourceAssetSlots();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.WorkspacePermissions().GetPermissions();

    public List<Type> Endpoints => [];
}

