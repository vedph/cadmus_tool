using Cadmus.Core.Config;
using Cadmus.Index.Graph;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CadmusTool.Commands
{
    /// <summary>
    /// Add graph presets command.
    /// </summary>
    /// <seealso cref="ICommand" />
    internal sealed class AddGraphPresetsCommand : ICommand
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

            CommandOption typeOption = command.Option("-t|--type",
                "The type of data to import: [N]odes, [M]appings, [T]hesauri",
                CommandOptionType.SingleValue);

            CommandOption thesIdAsRootOption = command.Option("-r|--root",
                "Add the thesaurus' ID as the root class node",
                CommandOptionType.NoValue);

            CommandOption thesIdPrefix = command.Option("-p|--prefix",
                "Set the prefix to add to each thesaurus' class node",
                CommandOptionType.SingleValue);

            CommandOption dryOption = command.Option("-d|--dry",
                "Dry mode - don't write to database",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                char type = 'N';
                if (typeOption.HasValue() && typeOption.Value().Length == 1)
                    type = char.ToUpperInvariant(typeOption.Value()[0]);

                options.Command = new AddGraphPresetsCommand(
                    new AddGraphPresetsCommandOptions(options)
                    {
                        Source = sourceArgument.Value,
                        DatabaseName = databaseArgument.Value,
                        ProfilePath = profileArgument.Value,
                        RepositoryPluginTag = repositoryTagArgument.Value,
                        Type = type,
                        ThesaurusIdAsRoot = thesIdAsRootOption.HasValue(),
                        ThesaurusIdPrefix = thesIdPrefix.Value(),
                        IsDry = dryOption.HasValue()
                    });
                return 0;
            });
        }

        private void ImportMappings(Stream source, IGraphRepository repository)
        {
            // source id : graph id
            Dictionary<int, int> ids = new Dictionary<int, int>();

            JsonGraphPresetReader reader = new();
            foreach (NodeMapping mapping in
                reader.ReadMappings(source))
            {
                Console.WriteLine(mapping);
                if (!_options.IsDry)
                {
                    // adjust IDs
                    int sourceId = mapping.Id;
                    mapping.Id = 0;
                    if (mapping.ParentId != 0)
                        mapping.ParentId = ids[mapping.ParentId];
                    repository.AddMapping(mapping);
                    ids[sourceId] = mapping.Id;
                }
            }
        }

        private void ImportNodes(Stream source, IGraphRepository repository)
        {
            JsonGraphPresetReader reader = new();

            foreach (UriNode node in reader.ReadNodes(source))
            {
                if (!_options.IsDry)
                {
                    node.Id = repository.AddUri(node.Uri);
                    Console.WriteLine(node);
                    repository.AddNode(node);
                }
                else Console.WriteLine(node);
            }
        }

        private void ImportThesauri(Stream source, IGraphRepository repository)
        {
            string json = new StreamReader(source, Encoding.UTF8).ReadToEnd();
            Thesaurus[] thesauri = JsonSerializer.Deserialize<Thesaurus[]>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true
                });
            foreach (Thesaurus thesaurus in thesauri)
            {
                if (!_options.IsDry)
                {
                    repository.AddThesaurus(thesaurus,
                        _options.ThesaurusIdAsRoot,
                        _options.ThesaurusIdPrefix);
                }
                Console.WriteLine(thesaurus);
            }
        }

        public Task Run()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("ADD PRESET DATA TO GRAPH\n");
            Console.ResetColor();

            Console.WriteLine(
                $"Source: {_options.Source}\n" +
                $"Type: {_options.Type}\n" +
                $"Database: {_options.DatabaseName}\n" +
                $"Profile file: {_options.ProfilePath}\n" +
                $"Repository plugin tag: {_options.RepositoryPluginTag}\n" +
                $"Dry mode: {(_options.IsDry ? "yes" : "no")}\n");

            if (_options.Type == 'T')
            {
                Console.WriteLine(
                    "Thesaurus ID as root" + 
                    (_options.ThesaurusIdAsRoot ? "yes" : "no"));
                Console.WriteLine(
                    $"\nThesaurus ID prefix {_options.ThesaurusIdPrefix}\n");
            }

            IGraphRepository repository = GraphHelper.GetGraphRepository(
                _options);
            if (repository != null)
            {
                using Stream source = new FileStream(_options.Source, FileMode.Open,
                    FileAccess.Read, FileShare.Read);

                switch (_options.Type)
                {
                    case 'M':
                        ImportMappings(source, repository);
                        break;
                    case 'T':
                        ImportThesauri(source, repository);
                        break;
                    default:
                        ImportNodes(source, repository);
                        break;
                }
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Options for <see cref="AddGraphPresetsCommand"/>
    /// </summary>
    internal class AddGraphPresetsCommandOptions : GraphCommandOptions
    {
        public AddGraphPresetsCommandOptions(AppOptions options) : base(options)
        {
        }

        public string Source { get; set; }
        public char Type { get; set; }
        public bool IsDry { get; set; }
        public bool ThesaurusIdAsRoot { get; set; }
        public string ThesaurusIdPrefix { get; set; }
    }
}
