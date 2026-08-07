using Scriban;
using System.IO;
using System.Threading.Tasks;
using System;

namespace Automation.Cli.Services;

public class ScaffoldingService
{
    private readonly string _templatesBasePath;

    public ScaffoldingService()
    {
        var exePath = AppDomain.CurrentDomain.BaseDirectory;
        _templatesBasePath = Path.Combine(exePath, "Templates");
        
        // Nếu chạy ở chế độ dev (dotnet run), BaseDirectory sẽ ở bin/Debug/net10.0/
        // Ta cần cấu hình csproj để copy thư mục Templates sang output folder.
    }

    public async Task RenderTemplateAsync(string templatePath, string destinationPath, object model)
    {
        var fullTemplatePath = Path.Combine(_templatesBasePath, templatePath);
        if (!File.Exists(fullTemplatePath))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: Template not found at {fullTemplatePath}");
            Console.ResetColor();
            return;
        }

        if (File.Exists(destinationPath))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Skipping {destinationPath} (File already exists)");
            Console.ResetColor();
            return;
        }

        var destDir = Path.GetDirectoryName(destinationPath);
        if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        var templateContent = await File.ReadAllTextAsync(fullTemplatePath);
        var template = Template.Parse(templateContent);

        if (template.HasErrors)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error parsing template {templatePath}:");
            foreach (var error in template.Messages)
            {
                Console.WriteLine(error);
            }
            Console.ResetColor();
            return;
        }

        var result = await template.RenderAsync(model);
        await File.WriteAllTextAsync(destinationPath, result);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Created: {destinationPath}");
        Console.ResetColor();
    }
}


