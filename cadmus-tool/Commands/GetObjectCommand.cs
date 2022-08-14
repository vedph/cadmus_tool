using Cadmus.Core;
using Cadmus.Core.Storage;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace CadmusTool.Commands
{
    internal sealed class GetObjectCommand : ICommand
    {
        private readonly GetObjectCommandOptions _options;

        public GetObjectCommand(GetObjectCommandOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Get the raw code for the specified item/part. " +
            command.HelpOption("-?|-h|--help");

            CommandArgument databaseArgument = command.Argument("[DatabaseName]",
                "The database name");

            CommandArgument idArgument = command.Argument("[ID]",
                "The item/part ID.");

            CommandArgument repositoryTagArgument = command.Argument(
                "[RepoFactoryProviderTag]",
                "The repository factory provider plugin tag.");

            CommandArgument outDirArgument = command.Argument("[OutputDirectory]",
                "The output directory.");

            CommandOption isPartOption = command.Option("-p|--part",
                "The ID refers to a part rather than to an item.",
                CommandOptionType.NoValue);

            CommandOption isXmlOption = command.Option("-x|--xml",
                "Write also XML converted from JSON code.",
                CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
                options.Command = new GetObjectCommand(
                    new GetObjectCommandOptions(options)
                    {
                        DatabaseName = databaseArgument.Value,
                        Id = idArgument.Value,
                        RepositoryPluginTag = repositoryTagArgument.Value,
                        OutputDir = outDirArgument.Value,
                        IsPart = isPartOption.HasValue(),
                        IsXml = isXmlOption.HasValue()
                    });
                return 0;
            });
        }

        private static void WriteText(string path, string text)
        {
            using StreamWriter writer = new(
                new FileStream(path, FileMode.Create, FileAccess.Write,
                FileShare.Read), Encoding.UTF8);
            writer.WriteLine(text);
            writer.Flush();
        }

        public Task Run()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("GET OBJECT\n");
            Console.ResetColor();

            Console.WriteLine($"Database: {_options.DatabaseName}\n" +
                              $"ID: {_options.Id}\n" +
                              $"Repository plugin tag: {_options.RepositoryPluginTag}\n" +
                              $"Output: {_options.OutputDir}" +
                              $"Item/part: {(_options.IsPart ? "part" : "item")}\n" +
                              $"XML: {(_options.IsXml ? "yes" : "no")}\n");
            Serilog.Log.Information("GET OBJECT: " +
                              $"Database: {_options.DatabaseName}, " +
                              $"ID: {_options.Id}, " +
                              $"Repository plugin tag: {_options.RepositoryPluginTag}, " +
                              $"Output: {_options.OutputDir}" +
                              $"Item/part: {(_options.IsPart ? "part" : "item")}, " +
                              $"XML: {(_options.IsXml ? "yes" : "no")}");

            if (!Directory.Exists(_options.OutputDir))
                Directory.CreateDirectory(_options.OutputDir!);

            // repository
            Console.WriteLine("Creating repository...");
            ICadmusRepository repository = CliHelper.GetCadmusRepository(
                _options.RepositoryPluginTag!,
                _options.Configuration.GetConnectionString("Mongo"),
                _options.DatabaseName!);

            // get object
            string? json;
            if (_options.IsPart)
            {
                json = repository.GetPartContent(_options.Id);
                if (json == null)
                {
                    Console.WriteLine("Part not found");
                    return Task.CompletedTask;
                }
            }
            else
            {
                IItem? item = repository.GetItem(_options.Id, false);
                if (item == null)
                {
                    Console.WriteLine("Item not found");
                    return Task.CompletedTask;
                }
                json = JsonSerializer.Serialize(item, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
            }

            // write JSON
            string path = Path.Combine(_options.OutputDir ?? "",
                _options.Id + ".json");
            Console.WriteLine("  -> " + path);
            WriteText(path, json);

            // convert and write XML if requested
            if (_options.IsXml)
            {
                // convert to XML
                json = "{\"root\":" + json + "}";
                XmlDocument? doc = JsonConvert.DeserializeXmlNode(json);
                string xml = doc?.OuterXml ?? "";
                path = Path.Combine(_options.OutputDir ?? "",
                    _options.Id + ".xml");
                Console.WriteLine("  -> " + path);
                WriteText(path, xml);
            }

            return Task.CompletedTask;
        }
    }

    internal sealed class GetObjectCommandOptions
    {
        private readonly AppOptions _appOptions;

        public IConfiguration Configuration => _appOptions.Configuration;
        public ILogger Logger => _appOptions.Logger;

        public GetObjectCommandOptions(AppOptions options)
        {
            _appOptions = options;
        }

        public string? DatabaseName { get; set; }
        public string? Id { get; set; }
        public string? RepositoryPluginTag { get; set; }
        public string? OutputDir { get; set; }
        public bool IsPart { get; set; }
        public bool IsXml { get; set; }
    }
}
