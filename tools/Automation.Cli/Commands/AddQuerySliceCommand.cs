using System.CommandLine;
using System.IO;
using Automation.Cli.Services;

namespace Automation.Cli.Commands;

public static class AddQuerySliceCommand
{
    public static Command Create()
    {
        var moduleArgument = new Argument<string>("module", "The name of the module (e.g. Orders)");
        var actionArgument = new Argument<string>("action", "The name of the action/slice (e.g. GetOrders)");

        var command = new Command("add-query", "Scaffolds a new query slice (Query, Endpoint, Handler)")
        {
            moduleArgument,
            actionArgument
        };

        command.SetHandler(async (string module, string action) =>
        {
            Console.WriteLine($"Scaffolding query slice: {action} in module {module}...");

            var scaffoldingService = new ScaffoldingService();

            var currentDir = Directory.GetCurrentDirectory();
            var rootDir = currentDir;
            if (Path.GetFileName(currentDir).Equals("Automation.Cli", StringComparison.OrdinalIgnoreCase))
            {
                rootDir = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
            }

            var sliceDir = Path.Combine(rootDir, "src", "Modules", module, $"Automation.{module}", "Features", module, action);

            var model = new { ModuleName = module, ActionName = action };

            await scaffoldingService.RenderTemplateAsync(
                "Query/ByIdQuery.sbn", // Mặc định dùng ByIdQuery cho đơn giản
                Path.Combine(sliceDir, $"{action}Query.cs"),
                model
            );

            await scaffoldingService.RenderTemplateAsync(
                "Query/QueryEndpoint.sbn",
                Path.Combine(sliceDir, $"{action}Endpoint.cs"),
                model
            );

            await scaffoldingService.RenderTemplateAsync(
                "Query/QueryHandler.sbn",
                Path.Combine(sliceDir, $"{action}Handler.cs"),
                model
            );

            var groupDir = Path.Combine(rootDir, "src", "Modules", module, $"Automation.{module}", "Features", module);
            var groupFile = Path.Combine(groupDir, $"{module}Group.cs");
            if (!File.Exists(groupFile))
            {
                await scaffoldingService.RenderTemplateAsync(
                    "Crud/EndpointGroup.sbn",
                    groupFile,
                    model
                );
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Query slice {action} created successfully in module {module}!");
            Console.ResetColor();

        }, moduleArgument, actionArgument);

        return command;
    }
}


