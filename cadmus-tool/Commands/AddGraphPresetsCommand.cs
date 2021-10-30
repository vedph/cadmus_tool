using Cadmus.Index.Graph;
using Cadmus.Index.MySql;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CadmusTool.Commands
{
    /// <summary>
    /// Add graph presets command.
    /// </summary>
    /// <seealso cref="ICommand" />
    public sealed class AddGraphPresetsCommand : ICommand
    {
        private readonly AddGraphPresetsCommandOptions _options;

        public AddGraphPresetsCommand(AddGraphPresetsCommandOptions options)
        {
            _options = options;
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Add preset nodes and mappings into the " +
                "specified database index.";
            command.HelpOption("-?|-h|--help");

            CommandArgument sourceArgument = command.Argument("[source]",
                "The path to the source JSON file with nodes or mappings");

            CommandArgument databaseArgument = command.Argument("[database]",
                "The database name");

            CommandArgument profileArgument = command.Argument("[profile]",
                "The indexer profile JSON file path");

            CommandArgument repositoryTagArgument = command.Argument("[tag]",
                "The repository factory provider plugin tag.");

            CommandOption mappingsOption = command.Option("-m|--mappings",
                "Source has node mappings instead of nodes",
                CommandOptionType.NoValue);

            CommandOption dryOption = command.Option("-d|--dry",
                "Dry mode - don't write to database",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                options.Command = new AddGraphPresetsCommand(
                    new AddGraphPresetsCommandOptions
                    {
                        AppOptions = options,
                        Source = sourceArgument.Value,
                        DatabaseName = databaseArgument.Value,
                        ProfilePath = profileArgument.Value,
                        RepositoryPluginTag = repositoryTagArgument.Value,
                        Mappings = mappingsOption.HasValue(),
                        IsDry = dryOption.HasValue()
                    });
                return 0;
            });
        }

        public async Task Run()
        {
            IGraphRepository graphRepository = GraphHelper.GetGraphRepository(
                _options);
            if (graphRepository == null) return;

            using Stream source = new FileStream(_options.Source, FileMode.Open,
                FileAccess.Read, FileShare.Read);

            JsonGraphPresetReader reader = new();
            if (_options.Mappings)
            {
                foreach (NodeMapping mapping in
                    await reader.ReadMappingsAsync(source))
                {
                    Console.WriteLine(mapping);
                    if (!_options.IsDry) graphRepository.AddMapping(mapping);
                }
            }
            else
            {
                foreach (UriNode node in await reader.ReadNodesAsync(source))
                {
                    Console.WriteLine(node);
                    if (!_options.IsDry)
                    {
                        node.Id = graphRepository.AddUri(node.Uri);
                        graphRepository.AddNode(node);
                    }
                }
            }
        }
    }

    public class AddGraphPresetsCommandOptions : GraphCommandOptions
    {
        public string Source { get; set; }
        public bool Mappings { get; set; }
        public bool IsDry { get; set; }
    }
}
