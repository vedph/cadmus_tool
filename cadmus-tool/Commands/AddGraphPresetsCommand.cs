using Cadmus.Index.Graph;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
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
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ADD PRESET DATA TO GRAPH\n");
            Console.ResetColor();

            Console.WriteLine(
                $"Source: {_options.Source}\n" +
                $"Mappings: {(_options.Mappings ? "yes" : "no")}\n" +
                $"Database: {_options.DatabaseName}\n" +
                $"Profile file: {_options.ProfilePath}\n" +
                $"Repository plugin tag: {_options.RepositoryPluginTag}\n" +
                $"Dry mode: {(_options.IsDry ? "yes" : "no")}\n");

            IGraphRepository graphRepository = GraphHelper.GetGraphRepository(
                _options);
            if (graphRepository == null) return;

            using Stream source = new FileStream(_options.Source, FileMode.Open,
                FileAccess.Read, FileShare.Read);

            JsonGraphPresetReader reader = new();
            if (_options.Mappings)
            {
                // source id : graph id
                Dictionary<int, int> ids = new Dictionary<int, int>();

                foreach (NodeMapping mapping in
                    await reader.ReadMappingsAsync(source))
                {
                    Console.WriteLine(mapping);
                    if (!_options.IsDry)
                    {
                        // adjust IDs
                        int sourceId = mapping.Id;
                        mapping.Id = 0;
                        if (mapping.ParentId != 0)
                            mapping.ParentId = ids[mapping.ParentId];
                        graphRepository.AddMapping(mapping);
                        ids[sourceId] = mapping.Id;
                    }
                }
            }
            else
            {
                foreach (UriNode node in await reader.ReadNodesAsync(source))
                {
                    if (!_options.IsDry)
                    {
                        node.Id = graphRepository.AddUri(node.Uri);
                        Console.WriteLine(node);
                        graphRepository.AddNode(node);
                    }
                    else Console.WriteLine(node);
                }
            }
        }
    }

    /// <summary>
    /// Options for <see cref="AddGraphPresetsCommand"/>
    /// </summary>
    public class AddGraphPresetsCommandOptions : GraphCommandOptions
    {
        public string Source { get; set; }
        public bool Mappings { get; set; }
        public bool IsDry { get; set; }
    }
}
