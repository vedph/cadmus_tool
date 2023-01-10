using Cadmus.Cli.Services;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Fusi.Cli.Commands;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Cadmus.Cli.Commands;

internal sealed class GetObjectCommand : ICommand
{
    private readonly GetObjectCommandOptions _options;

    public GetObjectCommand(GetObjectCommandOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public static void Configure(CommandLineApplication app,
        ICliAppContext context)
    {
        app.Description = "Get the raw code for the specified item/part. " +
        app.HelpOption("-?|-h|--help");

        CommandArgument databaseArgument = app.Argument("[DatabaseName]",
            "The database name");

        CommandArgument idArgument = app.Argument("[ID]",
            "The item/part ID.");

        CommandArgument repositoryTagArgument = app.Argument(
            "[RepoFactoryProviderTag]",
            "The repository factory provider plugin tag.");

        CommandArgument outDirArgument = app.Argument("[OutputDirectory]",
            "The output directory.");

        CommandOption isPartOption = app.Option("-p|--part",
            "The ID refers to a part rather than to an item.",
            CommandOptionType.NoValue);

        CommandOption isXmlOption = app.Option("-x|--xml",
            "Write also XML converted from JSON code.",
            CommandOptionType.NoValue);

        app.OnExecute(() =>
        {
            context.Command = new GetObjectCommand(
                new GetObjectCommandOptions(context)
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

    public Task<int> Run()
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
        string cs = string.Format(
          _options.Configuration!.GetConnectionString("Mongo")!,
          _options.DatabaseName);
        ICadmusRepository repository = CliHelper.GetCadmusRepository(
            _options.RepositoryPluginTag!, cs);

        // get object
        string? json;
        if (_options.IsPart)
        {
            json = repository.GetPartContent(_options.Id!);
            if (json == null)
            {
                Console.WriteLine("Part not found");
                return Task.FromResult(2);
            }
        }
        else
        {
            IItem? item = repository.GetItem(_options.Id!, false);
            if (item == null)
            {
                Console.WriteLine("Item not found");
                return Task.FromResult(2);
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

        return Task.FromResult(0);
    }
}

internal sealed class GetObjectCommandOptions :
    CommandOptions<CadmusCliAppContext>
{
    public string? DatabaseName { get; set; }
    public string? Id { get; set; }
    public string? RepositoryPluginTag { get; set; }
    public string? OutputDir { get; set; }
    public bool IsPart { get; set; }
    public bool IsXml { get; set; }

    public GetObjectCommandOptions(ICliAppContext options)
        : base((CadmusCliAppContext)options)
    {
    }
}
