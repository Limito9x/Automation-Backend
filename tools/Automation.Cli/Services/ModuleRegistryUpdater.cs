using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Automation.Cli.Services;

public class ModuleRegistryUpdater
{
    public async Task UpdateRegistryAsync(string moduleName)
    {
        // Giả sử đường dẫn gốc của project
        var currentDir = Directory.GetCurrentDirectory();
        
        // Tìm file ModuleRegistry.cs (thường ở src/Automation.Api/ModuleRegistry.cs)
        // Vì tools nằm ngang hàng src, ta sẽ lùi 1 cấp nếu đang ở trong tools
        var rootDir = currentDir;
        if (Path.GetFileName(currentDir).Equals("Automation.Cli", StringComparison.OrdinalIgnoreCase))
        {
            rootDir = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
        }

        var registryPath = Path.Combine(rootDir, "src", "Automation.Api", "ModuleRegistry.cs");

        if (!File.Exists(registryPath))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warning: Could not find ModuleRegistry.cs at {registryPath}. You need to register the module manually.");
            Console.ResetColor();
            return;
        }

        var content = await File.ReadAllTextAsync(registryPath);

        var usingStatement = $"using Automation.{moduleName};";
        if (content.Contains(usingStatement))
        {
            Console.WriteLine($"Module {moduleName} is already registered.");
            return;
        }

        // Thêm using vào cuối block using
        var lastUsingIndex = content.LastIndexOf("using ");
        if (lastUsingIndex != -1)
        {
            var endOfLine = content.IndexOf('\n', lastUsingIndex);
            if (endOfLine != -1)
            {
                content = content.Insert(endOfLine + 1, usingStatement + "\n");
            }
        }
        else
        {
            content = usingStatement + "\n\n" + content;
        }

        // Tìm mảng All = [ ... ]
        var pattern = @"public static readonly IModule\[\] All = \s*\[(.*?)\];";
        var match = Regex.Match(content, pattern, RegexOptions.Singleline);
        if (match.Success)
        {
            var arrayContent = match.Groups[1].Value;
            var newModuleEntry = $"new {moduleName}Module()";
            
            if (!string.IsNullOrWhiteSpace(arrayContent))
            {
                // Xóa khoảng trắng dư ở cuối để thêm dấu phẩy
                arrayContent = arrayContent.TrimEnd();
                if (!arrayContent.EndsWith(","))
                {
                    arrayContent += ",\n        ";
                }
                else
                {
                    arrayContent += "\n        ";
                }
            }
            arrayContent += newModuleEntry + "\n    ";

            var replacement = $"public static readonly IModule[] All = \n    [\n        {arrayContent.Trim()}\n    ];";
            content = content.Substring(0, match.Index) + replacement + content.Substring(match.Index + match.Length);
        }

        await File.WriteAllTextAsync(registryPath, content);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Updated ModuleRegistry.cs with {moduleName}Module");
        Console.ResetColor();
    }

    public async Task RemoveRegistryAsync(string moduleName)
    {
        var currentDir = Directory.GetCurrentDirectory();
        var rootDir = currentDir;
        if (Path.GetFileName(currentDir).Equals("Automation.Cli", StringComparison.OrdinalIgnoreCase))
        {
            rootDir = Path.GetFullPath(Path.Combine(currentDir, "..", ".."));
        }

        var registryPath = Path.Combine(rootDir, "src", "Automation.Api", "ModuleRegistry.cs");

        if (!File.Exists(registryPath))
        {
            return;
        }

        var content = await File.ReadAllTextAsync(registryPath);

        // Remove using statement
        var usingStatementRegex = new Regex($@"using\s+Automation\.{moduleName};\s*\n?", RegexOptions.Compiled);
        content = usingStatementRegex.Replace(content, "");

        // Remove module from array
        var moduleArrayRegex = new Regex($@"\s*new\s+{moduleName}Module\(\)\s*,?", RegexOptions.Compiled);
        content = moduleArrayRegex.Replace(content, "");

        // Clean up any trailing comma in the array before closing bracket
        content = Regex.Replace(content, @",\s*\]", "\n    ]");

        await File.WriteAllTextAsync(registryPath, content);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Removed {moduleName}Module from ModuleRegistry.cs");
        Console.ResetColor();
    }
}


