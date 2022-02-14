using Cadmus.Cli.Core;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.Index;
using Cadmus.Index.Config;
using CadmusTool.Services;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
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
                "from a Cadmus MongoDB database, using the specified " +
                "indexer profile.";
            command.HelpOption("-?|-h|--help");

            CommandArgument databaseArgument = command.Argument("[database]",
                "The database name");

            CommandArgument profileArgument = command.Argument("[profile]",
                "The indexer profile JSON file path");

            CommandArgument repositoryTagArgument = command.Argument("[tag]",
                "The repository factory provider plugin tag.");

            CommandArgument idArgument = command.Argument("[id]",
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
                        ProfilePath = profileArgument.Value,
                        RepositoryPluginTag = repositoryTagArgument.Value,
                        Id = idArgument.Value,
                        IsPart = isPartOption.HasValue(),
                        IsDeleted = isDeletedOption.HasValue()
                    });
                return 0;
            });
        }

        public async Task Run()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("MAP SINGLE ITEM/PART TO GRAPH\n");
            Console.ResetColor();

            Console.WriteLine($"Database: {_options.DatabaseName}\n" +
                              $"Profile file: {_options.ProfilePath}\n" +
                              $"Repository plugin tag: {_options.RepositoryPluginTag}\n" +
                              $"{(_options.IsPart ? "Part" : "Item")} ID: {_options.Id}\n");
            Serilog.Log.Information("MAP TO GRAPH: " +
                         $"Database: {_options.DatabaseName}, " +
                         $"Profile file: {_options.ProfilePath}, " +
                         $"Repository plugin tag: {_options.RepositoryPluginTag}\n" +
                         $"{(_options.IsPart ? "Part" : "Item")} ID: {_options.Id}\n");

            string profileContent = GraphHelper.LoadProfile(_options.ProfilePath);

            string cs = string.Format(_options.Configuration
                .GetConnectionString("Index"), _options.DatabaseName);
            IItemIndexFactoryProvider provider =
                new StandardItemIndexFactoryProvider(cs);

            // repository
            Console.WriteLine("Creating repository...");
            Serilog.Log.Information("Creating repository...");

            var repositoryProvider = PluginFactoryProvider
                .GetFromTag<ICliCadmusRepositoryProvider>(
                _options.RepositoryPluginTag);
            if (repositoryProvider == null)
            {
                throw new FileNotFoundException(
                    "The requested repository provider tag " +
                    _options.RepositoryPluginTag +
                    " was not found among plugins in " +
                    PluginFactoryProvider.GetPluginsDir());
            }
            repositoryProvider.ConnectionString = _options.Configuration
                .GetConnectionString("Mongo");
            ICadmusRepository repository = repositoryProvider.CreateRepository(
                _options.DatabaseName);

            if (_options.IsDeleted)
            {
                GraphHelper.UpdateGraphForDeletion(_options.Id, _options);
                return;
            }

            // index
            ItemIndexFactory factory = provider.GetFactory(profileContent);
            IItemIndexWriter writer = factory.GetItemIndexWriter(true);

            if (_options.IsPart)
            {
                IPart part = repository.GetPart<IPart>(_options.Id);
                if (part == null)
                {
                    Console.WriteLine("Part not found");
                    return;
                }
                await writer.WritePart(repository.GetItem(_options.Id), part);
                writer.Close();

                IItem item = repository.GetItem(part.ItemId);
                if (item == null)
                {
                    Console.WriteLine("Item not found");
                    return;
                }
                GraphDataPinFilter filter = (GraphDataPinFilter)writer.DataPinFilter;
                var pins = filter.GetSortedGraphPins()
                    .Select(p => Tuple.Create(p.Name, p.Value))
                    .ToList();
                GraphHelper.UpdateGraph(item, part, pins, _options);
            }
            else
            {
                // rebuild item pins
                IItem item = repository.GetItem(_options.Id, false);
                if (item == null)
                {
                    Console.WriteLine("Item not found");
                    return;
                }
                await writer.WriteItem(item);
                writer.Close();
                // update graph for item
                GraphHelper.UpdateGraph(item, _options);
            }
        }
    }

    internal class GraphOneCommandOptions : GraphCommandOptions
    {
        public GraphOneCommandOptions(AppOptions options) : base(options)
        {
        }

        public string Id { get; set; }
        public bool IsPart { get; set; }
        public bool IsDeleted { get; set; }
    }
}
