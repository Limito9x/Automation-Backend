using System.CommandLine;
using Automation.Cli.Commands;

namespace Automation.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Automation CLI Scaffold Tool");

        rootCommand.AddCommand(AddModuleCommand.Create());
        rootCommand.AddCommand(RemoveModuleCommand.Create());
        rootCommand.AddCommand(AddCommandSliceCommand.Create());
        rootCommand.AddCommand(AddQuerySliceCommand.Create());
        rootCommand.AddCommand(AddCrudCommand.Create());

        return await rootCommand.InvokeAsync(args);
    }
}

