using Cadmus.Cli.Core;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.Index;
using Cadmus.Index.Config;
using Cadmus.Index.Graph;
using Cadmus.Index.MySql;
using Cadmus.Index.Sql;
using CadmusTool.Services;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CadmusTool.Commands
{
    public sealed class MapToGraphCommand : ICommand
    {
        private readonly MapToGraphCommandOptions _options;

        public MapToGraphCommand(MapToGraphCommandOptions options)
        {
            _options = options;
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Map an item/part into the graph from a Cadmus " +
                "MongoDB database, using the specified indexer profile.";
            command.HelpOption("-?|-h|--help");

            CommandArgument databaseArgument = command.Argument("[database]",
                "The database name");

            CommandArgument profileArgument = command.Argument("[profile]",
                "The indexer profile JSON file path");

            CommandArgument repositoryTagArgument = command.Argument("[tag]",
                "The repository factory provider plugin tag.");

            CommandArgument itemIdArgument = command.Argument("[id]",
                "The ID of the item/part to be mapped");

            CommandOption isPartOption = command.Option("-p|--part",
                "The ID refers to a part rather than to an item",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                options.Command = new MapToGraphCommand(
                    new MapToGraphCommandOptions
                    {
                        AppOptions = options,
                        DatabaseName = databaseArgument.Value,
                        ProfilePath = profileArgument.Value,
                        Id = itemIdArgument.Value,
                        IsPart = isPartOption.HasValue()
                    });
                return 0;
            });
        }

        private static string LoadProfile(string path)
        {
            using StreamReader reader = File.OpenText(path);
            return reader.ReadToEnd();
        }

        private IGraphRepository GetGraphRepository()
        {
            string cs = string.Format(_options.AppOptions.Configuration
                .GetConnectionString("Index"), _options.DatabaseName);

            var repository = new MySqlGraphRepository();
            repository.Configure(new SqlOptions
            {
                ConnectionString = cs
            });
            return repository;
        }

        private void UpdateGraph(IItem item)
        {
            IGraphRepository graphRepository = GetGraphRepository();
            if (graphRepository == null) return;

            _options.AppOptions.Logger.LogInformation("Mapping " + item);
            NodeMapper mapper = new NodeMapper(graphRepository)
            {
                Logger = _options.AppOptions.Logger
            };
            GraphSet set = mapper.MapItem(item);

            _options.AppOptions.Logger.LogInformation("Updating graph " + set);
            GraphUpdater updater = new GraphUpdater(graphRepository);
            updater.Update(set);
        }

        private void UpdateGraph(IItem item, IPart part,
            IList<Tuple<string, string>> pins)
        {
            IGraphRepository graphRepository = GetGraphRepository();
            if (graphRepository == null) return;

            _options.AppOptions.Logger.LogInformation("Mapping " + part);
            NodeMapper mapper = new NodeMapper(graphRepository)
            {
                Logger = _options.AppOptions.Logger
            };
            GraphSet set = mapper.MapPins(item, part, pins);

            _options.AppOptions.Logger.LogInformation("Updating graph " + set);
            GraphUpdater updater = new GraphUpdater(graphRepository);
            updater.Update(set);
        }

        private void UpdateGraphForDeletion(string id)
        {
            IGraphRepository graphRepository = GetGraphRepository();
            if (graphRepository == null) return;

            _options.AppOptions.Logger.LogInformation("Updating graph for deleted " + id);
            GraphUpdater updater = new GraphUpdater(graphRepository);
            updater.Delete(id);
        }

        public async Task Run()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("MAP TO GRAPH\n");
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

            string profileContent = LoadProfile(_options.ProfilePath);

            string cs = string.Format(_options.AppOptions.Configuration
                .GetConnectionString("Index"), _options.DatabaseName);
            IItemIndexFactoryProvider provider =
                new StandardItemIndexFactoryProvider(cs);

            // repository
            Console.WriteLine("Creating repository...");
            Serilog.Log.Information("Creating repository...");

            var repositoryProvider = PluginFactoryProvider
                .GetFromTag<ICliRepositoryFactoryProvider>(
                _options.RepositoryPluginTag);
            if (repositoryProvider == null)
            {
                throw new FileNotFoundException(
                    "The requested repository provider tag " +
                    _options.RepositoryPluginTag +
                    " was not found among plugins in " +
                    PluginFactoryProvider.GetPluginsDir());
            }
            repositoryProvider.ConnectionString = _options.AppOptions.Configuration
                .GetConnectionString("Mongo");
            ICadmusRepository repository = repositoryProvider.CreateRepository(
                _options.DatabaseName);

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
                UpdateGraph(item, part, pins);
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
                UpdateGraph(item);
            }
        }
    }

    public class MapToGraphCommandOptions : CommandOptions
    {
        public string DatabaseName { get; set; }
        public string ProfilePath { get; set; }
        public string RepositoryPluginTag { get; set; }
        public string Id { get; set; }
        public bool IsPart { get; set; }
    }

}
