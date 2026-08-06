using System.CommandLine;
using System.IO;
using Automation.Cli.Services;

namespace Automation.Cli.Commands;

public static class AddCommandSliceCommand
{
    public static Command Create()
    {
        var moduleArgument = new Argument<string>("module", "The name of the module (e.g. Orders)");
        var actionArgument = new Argument<string>("action", "The name of the action/slice (e.g. CreateOrder)");

        var command = new Command("add-command", "Scaffolds a new command slice (Command, Endpoint, Handler, Validator)")
        {
            moduleArgument,
            actionArgument
        };

        command.SetHandler(async (string module, string action) =>
        {
            Console.WriteLine($"Scaffolding command slice: {action} in module {module}...");

            var scaffoldingService = new ScaffoldingService();

            var currentDir = Directory.GetCurrentDirectory();
            var rootDir = currentDir;
            if (Path.GetFileName(currentDir).Equals("Automation.Cli", StringComparison.OrdinalIgnoreCase))
            {
                rootDir = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
            }

            // Theo chuẩn: src/Modules/Orders/Automation.Orders/Features/Orders/CreateOrder
            var sliceDir = Path.Combine(rootDir, "src", "Modules", module, $"Automation.{module}", "Features", module, action);

            var model = new { ModuleName = module, ActionName = action };

            await scaffoldingService.RenderTemplateAsync(
                "Slice/Command.sbn",
                Path.Combine(sliceDir, $"{action}Command.cs"),
                model
            );

            await scaffoldingService.RenderTemplateAsync(
                "Slice/CommandEndpoint.sbn",
                Path.Combine(sliceDir, $"{action}Endpoint.cs"),
                model
            );

            await scaffoldingService.RenderTemplateAsync(
                "Slice/CommandHandler.sbn",
                Path.Combine(sliceDir, $"{action}Handler.cs"),
                model
            );

            await scaffoldingService.RenderTemplateAsync(
                "Slice/CommandValidator.sbn",
                Path.Combine(sliceDir, $"{action}Validator.cs"),
                model
            );
            
            // Check if group exists, if not, create it
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
            Console.WriteLine($"Command slice {action} created successfully in module {module}!");
            Console.ResetColor();

        }, moduleArgument, actionArgument);

        return command;
    }
}

