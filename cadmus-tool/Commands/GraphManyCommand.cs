using Cadmus.Cli.Core;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.Index;
using Cadmus.Index.Config;
using CadmusTool.Services;
using Fusi.Tools.Data;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using ShellProgressBar;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CadmusTool.Commands
{
    internal sealed class GraphManyCommand : ICommand
    {
        private readonly GraphCommandOptions _options;

        public GraphManyCommand(GraphCommandOptions options)
        {
            _options = options;
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Map all the items into the graph " +
                "from a Cadmus MongoDB database, using the specified " +
                "indexer profile.";
            command.HelpOption("-?|-h|--help");

            CommandArgument databaseArgument = command.Argument("[database]",
                "The database name");

            CommandArgument profileArgument = command.Argument("[profile]",
                "The indexer profile JSON file path");

            CommandArgument repositoryTagArgument = command.Argument("[tag]",
                "The repository factory provider plugin tag.");

            command.OnExecute(() =>
            {
                options.Command = new GraphManyCommand(
                    new GraphCommandOptions(options)
                    {
                        DatabaseName = databaseArgument.Value,
                        ProfilePath = profileArgument.Value,
                        RepositoryPluginTag = repositoryTagArgument.Value
                    });
                return 0;
            });
        }

        public async Task Run()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("MAP ITEMS TO GRAPH\n");
            Console.ResetColor();

            Console.WriteLine($"Database: {_options.DatabaseName}\n" +
                              $"Profile file: {_options.ProfilePath}\n" +
                              $"Repository plugin tag: {_options.RepositoryPluginTag}\n");
            Serilog.Log.Information("MAP TO GRAPH: " +
                         $"Database: {_options.DatabaseName}, " +
                         $"Profile file: {_options.ProfilePath}, " +
                         $"Repository plugin tag: {_options.RepositoryPluginTag}\n");

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

            ProgressBarOptions options = CliHelper.GetProgressBarOptions();
            using var bar = new ProgressBar(100, "Indexing...", options);

            // first page
            int oldPercent = 0;
            ItemFilter filter = new() { PageSize = 100 };
            DataPage<ItemInfo> page = repository.GetItems(filter);
            if (page.Total == 0) return;

            ItemIndexFactory factory = provider.GetFactory(profileContent);
            IItemIndexWriter writer = factory.GetItemIndexWriter(true);
            do
            {
                int done = 0;
                foreach (ItemInfo info in page.Items)
                {
                    IItem item = repository.GetItem(info.Id, true);
                    if (item == null) continue;
                    if (item == null)
                    {
                        Console.WriteLine("Item not found");
                        return;
                    }
                    await writer.WriteItem(item);
                    // update graph for item
                    GraphHelper.UpdateGraph(item, _options);

                    // update graph for its parts
                    foreach (IPart part in item.Parts)
                    {
                        GraphHelper.UpdateGraph(item, part,
                            part.GetDataPins()
                                .Select(p => Tuple.Create(p.Name, p.Value))
                                .ToArray(),
                            _options);
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
            writer.Close();
        }
    }

    internal class GraphCommandOptions : CommandOptions
    {
        public GraphCommandOptions(AppOptions options) : base(options)
        {
        }

        public string DatabaseName { get; set; }
        public string ProfilePath { get; set; }
        public string RepositoryPluginTag { get; set; }
    }
}
