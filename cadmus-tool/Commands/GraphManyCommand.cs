using Cadmus.Cli.Services;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.Graph;
using Fusi.Tools.Data;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

internal sealed class GraphManyCommand : AsyncCommand<GraphManyCommandSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context,
        GraphManyCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[red underline]MAP ITEMS TO GRAPH[/]");

        AnsiConsole.MarkupLine($"Database: [cyan]{settings.DatabaseName}[/]");
        AnsiConsole.MarkupLine($"Mappings: [cyan]{settings.MappingsPath}[/]");
        if (!string.IsNullOrEmpty(settings.RepositoryPluginTag))
        {
            AnsiConsole.MarkupLine(
                $"Repository plugin tag: [cyan]{settings.RepositoryPluginTag}[/]");
        }
        Serilog.Log.Information("MAP TO GRAPH: " +
                     $"Database: {settings.DatabaseName}, " +
                     $"Mappings: {settings.MappingsPath}, " +
                     $"Repository plugin tag: {settings.RepositoryPluginTag}\n");

        // repository
        AnsiConsole.MarkupLine("Creating repository...");
        Serilog.Log.Information("Creating repository...");
        string cs = string.Format(
          CliAppContext.Configuration.GetConnectionString("Mongo")!,
          settings.DatabaseName);
        ICadmusRepository repository = CliHelper.GetCadmusRepository(
            settings.RepositoryPluginTag!, cs);

        IGraphRepository graphRepository = GraphHelper.GetGraphRepository(
            settings.DatabaseName!);
        GraphUpdater updater = new(graphRepository);

        int oldPercent = 0;
        ItemFilter filter = new() { PageSize = 100 };
        DataPage<ItemInfo> page = repository.GetItems(filter);
        if (page.Total == 0) return Task.FromResult(0);

        bool error = false;
        AnsiConsole.Progress().Start(ctx =>
        {
            ProgressTask task = ctx.AddTask("Mapping items to graph");

            // first page
            do
            {
                foreach (ItemInfo info in page.Items)
                {
                    IItem? item = repository.GetItem(info.Id!, true);
                    if (item == null) continue;
                    if (item == null)
                    {
                        AnsiConsole.MarkupLine("[red]Item not found[/]");
                        error = true;
                        break;
                    }

                    // update graph for item
                    updater.Update(item);

                    // update graph for its parts
                    foreach (IPart part in item.Parts)
                        updater.Update(item, part);
                }

                // progress
                int percent = filter.PageNumber * 100 / page.PageCount;
                if (percent != oldPercent)
                {
                    task.Increment(percent - oldPercent);
                    oldPercent = percent;
                }

                // next page
                if (++filter.PageNumber > page.PageCount) break;
                page = repository.GetItems(filter);
            } while (!error && page.Items.Count != 0);
        });

        return error? Task.FromResult(2) : Task.FromResult(0);
    }
}

internal class GraphManyCommandSettings : CommandSettings
{
    [CommandArgument(0, "<DatabaseName>")]
    [Description("The database name")]
    public string? DatabaseName { get; set; }

    [CommandArgument(1, "<MappingsPath>")]
    [Description("The path to the mappings file")]
    public string? MappingsPath { get; set; }

    [CommandOption("-t|--tag <RepositoryPluginTag>")]
    [Description("The repository factory plugin tag")]
    public string? RepositoryPluginTag { get; set; }
}
