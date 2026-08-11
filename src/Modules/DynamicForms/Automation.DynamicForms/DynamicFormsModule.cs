using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Automation.DynamicForms.Infrastructure.Persistence;
using Automation.DynamicForms.Infrastructure.Api;
using Automation.DynamicForms.Contracts;
using Automation.DynamicForms.Extensions;

namespace Automation.DynamicForms;

public sealed class DynamicFormsModule : IModule, IPermissionModule
{
    public string Name => "DynamicForms";
    public string SchemaName => "dynamicforms";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<DynamicFormsDbContext>(config, SchemaName);
        services.AddScoped<ISchemaApi, SchemaApi>();
        services.AddScoped<Automation.DynamicForms.Services.IDynamicFormEngine, Automation.DynamicForms.Services.DynamicFormEngine>();
        services.AddDynamicFormAssets();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // Configure Wolverine if needed
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.DynamicFormsPermissions().GetPermissions();

    public List<Type> Endpoints => new();
}
