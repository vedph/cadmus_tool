using Cadmus.Graph;
using SharpCompress.Common;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

internal sealed class GraphDerefMappingsCommand :
    AsyncCommand<GraphDerefMappingsCommandSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context,
        GraphDerefMappingsCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[green underline]DEREFERENCE MAPPINGS DOCUMENT[/]");
        AnsiConsole.MarkupLine($"Input: [cyan]{settings.InputPath}[/]");
        AnsiConsole.MarkupLine($"Output: [cyan]{settings.OutputPath}[/]");

        string json = File.ReadAllText(settings.InputPath!);
        NodeMappingDocument? doc =
            JsonSerializer.Deserialize<NodeMappingDocument>(json,
            new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true,
            }) ?? throw new InvalidFormatException("Invalid JSON mappings document");

        List<NodeMapping> mappings = doc.GetMappings().ToList();
        using StreamWriter writer = new(settings.OutputPath!, false, Encoding.UTF8);
        writer.Write(JsonSerializer.Serialize(mappings));
        writer.Flush();

        return Task.FromResult(0);
    }
}

internal class GraphDerefMappingsCommandSettings : CommandSettings
{
    [CommandArgument(0, "<InputPath>")]
    [Description("The input JSON mappings file path")]
    public string? InputPath { get; set; }

    [CommandArgument(1, "<OutputPath>")]
    [Description("The output JSON mappings file path")]
    public string? OutputPath { get; set; }
}