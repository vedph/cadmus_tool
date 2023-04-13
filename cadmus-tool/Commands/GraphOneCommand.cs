using Cadmus.Cli.Services;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.Graph;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

internal sealed class GraphOneCommand : AsyncCommand<GraphOneCommandSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context,
        GraphOneCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[red underline]MAP ITEM/PART TO GRAPH[/]");
        AnsiConsole.MarkupLine($"Database: [cyan]{settings.DatabaseName}[/]");
        AnsiConsole.MarkupLine($"Mappings file: [cyan]{settings.MappingsPath}[/]");
        if (!string.IsNullOrEmpty(settings.RepositoryPluginTag))
        {
            AnsiConsole.MarkupLine(
                $"Repository plugin tag: [cyan]{settings.RepositoryPluginTag}[/]");
        }
        AnsiConsole.MarkupLine(
            $"{(settings.IsPart ? "Part" : "Item")} ID: [cyan]{settings.Id}[/]");

        Serilog.Log.Information("MAP TO GRAPH: " +
                     $"Database: {settings.DatabaseName}, " +
                     $"Mappings file: {settings.MappingsPath}, " +
                     $"Repository plugin tag: {settings.RepositoryPluginTag}\n" +
                     $"{(settings.IsPart ? "Part" : "Item")} ID: {settings.Id}\n");

        // repository
        AnsiConsole.MarkupLine("Creating repository...");

        string cs = string.Format(
          CliAppContext.Configuration.GetConnectionString("Mongo")!,
          settings.DatabaseName);
        ICadmusRepository repository = CliHelper.GetCadmusRepository(
            settings.RepositoryPluginTag, cs);

        if (settings.IsDeleted)
        {
            GraphHelper.UpdateGraphForDeletion(settings.Id!,
                settings.DatabaseName!);
            return Task.FromResult(0);
        }

        IItem? item;
        IPart? part = null;

        // get item and part
        if (settings.IsPart)
        {
            part = repository.GetPart<IPart>(settings.Id!);
            if (part == null)
            {
                Console.WriteLine("Part not found");
                return Task.FromResult(2);
            }
            item = repository.GetItem(part.ItemId, false);
        }
        else
        {
            item = repository.GetItem(settings.Id!, false);
        }

        if (item == null)
        {
            Console.WriteLine("Item not found");
            return Task.FromResult(2);
        }

        // update graph
        IGraphRepository graphRepository = GraphHelper.GetGraphRepository(
            settings.DatabaseName!);
        GraphUpdater updater = new(graphRepository);

        if (settings.IsPart)
        {
            updater.Update(item, part!);
        }
        else
        {
            updater.Update(item);
        }
        return Task.FromResult(0);
    }
}

internal class GraphOneCommandSettings : CommandSettings
{
    [CommandArgument(0, "<DatabaseName>")]
    [Description("The database name")]
    public string? DatabaseName { get; set; }

    [CommandArgument(1, "<MappingsPath>")]
    [Description("The path to the mappings file")]
    public string? MappingsPath { get; set; }

    [CommandArgument(2, "<ID>")]
    [Description("The item/part ID")]
    public string? Id { get; set; }

    [CommandOption("-t|--tag <RepositoryPluginTag>")]
    [Description("The repository factory plugin tag")]
    public string? RepositoryPluginTag { get; set; }

    [CommandOption("-p|--part")]
    [Description("The ID refers to a part rather than to an item")]
    public bool IsPart { get; set; }

    [CommandOption("-d|--deleted")]
    [Description("The ID refers to an item/part which was deleted")]
    public bool IsDeleted { get; set; }
}
