using Cadmus.Cli.Services;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.Graph;
using Fusi.Cli.Commands;
using Fusi.Tools.Data;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using ShellProgressBar;
using System;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

internal sealed class GraphManyCommand : ICommand
{
    private readonly GraphCommandOptions _options;

    public GraphManyCommand(GraphCommandOptions options)
    {
        _options = options;
    }

    public static void Configure(CommandLineApplication app,
        ICliAppContext context)
    {
        app.Description = "Map all the items into the graph " +
            "from a Cadmus MongoDB database, using the specified " +
            "indexer profile.";
        app.HelpOption("-?|-h|--help");

        CommandArgument databaseArgument = app.Argument("[DatabaseName]",
            "The database name");

        CommandArgument mappingsArgument = app.Argument("[MappingsPath]",
            "The mappings JSON file path");

        CommandArgument repositoryTagArgument = app.Argument(
            "[RepoFactoryProviderTag]",
            "The repository factory provider plugin tag.");

        app.OnExecute(() =>
        {
            context.Command = new GraphManyCommand(
                new GraphCommandOptions(context)
                {
                    DatabaseName = databaseArgument.Value,
                    MappingsPath = mappingsArgument.Value,
                    RepositoryPluginTag = repositoryTagArgument.Value
                });
            return 0;
        });
    }

    public Task<int> Run()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("MAP ITEMS TO GRAPH\n");
        Console.ResetColor();

        Console.WriteLine($"Database: {_options.DatabaseName}\n" +
                          $"Mappings: {_options.MappingsPath}\n" +
                          $"Repository plugin tag: {_options.RepositoryPluginTag}\n");
        Serilog.Log.Information("MAP TO GRAPH: " +
                     $"Database: {_options.DatabaseName}, " +
                     $"Mappings: {_options.MappingsPath}, " +
                     $"Repository plugin tag: {_options.RepositoryPluginTag}\n");

        // repository
        Console.WriteLine("Creating repository...");
        Serilog.Log.Information("Creating repository...");
        string cs = string.Format(
          _options.Configuration!.GetConnectionString("Mongo")!,
          _options.DatabaseName);
        ICadmusRepository repository = CliHelper.GetCadmusRepository(
            _options.RepositoryPluginTag!, cs);

        IGraphRepository graphRepository = GraphHelper.GetGraphRepository(
            _options);
        GraphUpdater updater = new(graphRepository);

        ProgressBarOptions options = CliHelper.GetProgressBarOptions();
        using var bar = new ProgressBar(100, "Indexing...", options);

        // first page
        int oldPercent = 0;
        ItemFilter filter = new() { PageSize = 100 };
        DataPage<ItemInfo> page = repository.GetItems(filter);
        if (page.Total == 0) return Task.FromResult(0);

        do
        {
            int done = 0;
            foreach (ItemInfo info in page.Items)
            {
                IItem? item = repository.GetItem(info.Id!, true);
                if (item == null) continue;
                if (item == null)
                {
                    Console.WriteLine("Item not found");
                    return Task.FromResult(2);
                }

                // update graph for item
                updater.Update(item);

                // update graph for its parts
                foreach (IPart part in item.Parts)
                {
                    updater.Update(item, part);
                }
                bar.Message = "item " + (++done);
            }

            // progress
            int percent = filter.PageNumber * 100 / page.PageCount;
            if (percent != oldPercent)
            {
                bar.Tick(percent);
                oldPercent = percent;
            }

            // next page
            if (++filter.PageNumber > page.PageCount) break;
            page = repository.GetItems(filter);
        } while (page.Items.Count != 0);

        return Task.FromResult(0);
    }
}

internal class GraphCommandOptions : CommandOptions<CadmusCliAppContext>
{
    public string? DatabaseName { get; set; }
    public string? MappingsPath { get; set; }
    public string? RepositoryPluginTag { get; set; }

    public GraphCommandOptions(ICliAppContext options)
        : base((CadmusCliAppContext)options)
    {
    }
}
