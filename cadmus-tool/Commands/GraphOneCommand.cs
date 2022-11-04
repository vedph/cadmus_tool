using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.Graph;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace CadmusTool.Commands
{
    internal sealed class GraphOneCommand : ICommand
    {
        private readonly GraphOneCommandOptions _options;

        public GraphOneCommand(GraphOneCommandOptions options)
        {
            _options = options;
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Map a single item/part into the graph " +
                "from a Cadmus MongoDB database.";
            command.HelpOption("-?|-h|--help");

            CommandArgument databaseArgument = command.Argument("[DatabaseName]",
                "The database name");

            CommandArgument mappingsArgument = command.Argument("[MappingsPath]",
                "The indexer profile JSON file path");

            CommandArgument repositoryTagArgument = command.Argument(
                "[RepoFactoryProviderTag]",
                "The repository factory provider plugin tag.");

            CommandArgument idArgument = command.Argument("[ID]",
                "The ID of the item/part to be mapped");

            CommandOption isPartOption = command.Option("-p|--part",
                "The ID refers to a part rather than to an item",
                CommandOptionType.NoValue);

            CommandOption isDeletedOption = command.Option("-d|--deleted",
                "The ID refers to an item/part which was deleted",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                options.Command = new GraphOneCommand(
                    new GraphOneCommandOptions(options)
                    {
                        DatabaseName = databaseArgument.Value,
                        MappingsPath = mappingsArgument.Value,
                        RepositoryPluginTag = repositoryTagArgument.Value,
                        Id = idArgument.Value,
                        IsPart = isPartOption.HasValue(),
                        IsDeleted = isDeletedOption.HasValue()
                    });
                return 0;
            });
        }

        public Task Run()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("MAP SINGLE ITEM/PART TO GRAPH\n");
            Console.ResetColor();

            Console.WriteLine($"Database: {_options.DatabaseName}\n" +
                              $"Mappings file: {_options.MappingsPath}\n" +
                              $"Repository plugin tag: {_options.RepositoryPluginTag}\n" +
                              $"{(_options.IsPart ? "Part" : "Item")} ID: {_options.Id}\n");
            Serilog.Log.Information("MAP TO GRAPH: " +
                         $"Database: {_options.DatabaseName}, " +
                         $"Mappings file: {_options.MappingsPath}, " +
                         $"Repository plugin tag: {_options.RepositoryPluginTag}\n" +
                         $"{(_options.IsPart ? "Part" : "Item")} ID: {_options.Id}\n");

            // repository
            Console.WriteLine("Creating repository...");
            Serilog.Log.Information("Creating repository...");
            string cs = string.Format(
              _options.Configuration.GetConnectionString("Mongo"),
              _options.DatabaseName);
            ICadmusRepository repository = CliHelper.GetCadmusRepository(
                _options.RepositoryPluginTag!, cs);

            if (_options.IsDeleted)
            {
                GraphHelper.UpdateGraphForDeletion(_options.Id!, _options);
                return Task.CompletedTask;
            }

            IItem? item;
            IPart? part = null;

            // get item and part
            if (_options.IsPart)
            {
                part = repository.GetPart<IPart>(_options.Id!);
                if (part == null)
                {
                    Console.WriteLine("Part not found");
                    return Task.CompletedTask;
                }
                item = repository.GetItem(part.ItemId, false);
            }
            else
            {
                item = repository.GetItem(_options.Id!, false);
            }

            if (item == null)
            {
                Console.WriteLine("Item not found");
                return Task.CompletedTask;
            }

            // update graph
            IGraphRepository graphRepository = GraphHelper.GetGraphRepository(
                _options);
            GraphUpdater updater = new(graphRepository);

            if (_options.IsPart)
            {
                updater.Update(item, part!);
            }
            else
            {
                updater.Update(item);
            }
            return Task.CompletedTask;
        }
    }

    internal class GraphOneCommandOptions : GraphCommandOptions
    {
        public GraphOneCommandOptions(AppOptions options) : base(options)
        {
        }

        public string? Id { get; set; }
        public bool IsPart { get; set; }
        public bool IsDeleted { get; set; }
    }
}
