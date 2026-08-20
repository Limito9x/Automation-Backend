using Automation.Inspection.Extensions;
using Automation.Inspection.Features.Inspections;
using Automation.Inspection.Infrastructure.Persistence;
using Automation.SharedKernel.Abstractions.Modules;
using Automation.SharedKernel.Extensions.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Wolverine.RabbitMQ;

using Automation.Inspection.Infrastructure.Services;

namespace Automation.Inspection;

public sealed class InspectionModule : IModule, IPermissionModule
{
    public string Name => "Inspection";
    public string SchemaName => "inspection";

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        services.AddModuleDbContext<InspectionDbContext>(config, SchemaName);
        services.AddScoped<IInspectionApi, InspectionApiService>();
        services.AddInspectionAssetSlots();
    }

    public void ConfigureWolverine(WolverineOptions options)
    {
        // 1. Định tuyến tác vụ kiểm tra ra RabbitMQ queue "tasks.inspect"
        options.PublishMessage<InspectResourceTask>()
               .ToRabbitQueue("tasks.inspect");

        // 2. Lắng nghe kết quả từ queue "inspection_results"
        // Agent không dùng Wolverine envelope → phải dùng DefaultIncomingMessage<T>()
        // để Wolverine biết deserialize JSON body thành SubmitInspectionResultCommand
        options.ListenToRabbitQueue("inspection_results")
               .DefaultIncomingMessage<Automation.Inspection.Features.Inspections.SubmitInspectionResult.SubmitInspectionResultCommand>();
    }

    public Dictionary<string, IReadOnlyList<string>> GetPermissions() 
        => new Constants.InspectionPermissions().GetPermissions();

    public List<Type> Endpoints => [..DiscoveredTypes.All];
}
