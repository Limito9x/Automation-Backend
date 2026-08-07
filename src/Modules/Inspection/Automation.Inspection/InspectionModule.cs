using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Automation.Inspection.Infrastructure.Persistence;

namespace Automation.Inspection;

public sealed class InspectionModule : IModule, IPermissionModule
{
    public string Name => "Inspection";
    public string SchemaName => "inspection";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<InspectionDbContext>(config, SchemaName);
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.InspectionPermissions().GetPermissions();

    public List<Type> Endpoints => [];
}
