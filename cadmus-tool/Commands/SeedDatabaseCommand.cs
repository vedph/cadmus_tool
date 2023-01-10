using Cadmus.Cli.Core;
using Cadmus.Cli.Services;
using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Mongo;
using Cadmus.Seed;
using Fusi.Cli.Commands;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

internal sealed class SeedDatabaseCommand : ICommand
{
    private readonly SeedDatabaseCommandOptions _options;

    public SeedDatabaseCommand(SeedDatabaseCommandOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public static void Configure(CommandLineApplication app,
        ICliAppContext context)
    {
        app.Description = "Create and seed a Cadmus MongoDB database " +
                              "from the specified profile and number of items.";
        app.HelpOption("-?|-h|--help");

        CommandArgument databaseArgument = app.Argument("[database]",
            "The database name");

        CommandArgument profileArgument = app.Argument("[profile]",
            "The seed profile JSON file path");

        CommandArgument repositoryTagArgument = app.Argument("[tag]",
            "The repository factory provider plugin tag.");

        CommandArgument seederTagArgument = app.Argument("[tag]",
            "The parts seeder factory provider plugin tag.");

        CommandOption countOption = app.Option("-c|--count",
            "Items count (default=100)",
            CommandOptionType.SingleValue);

        CommandOption dryOption = app.Option("-d|--dry", "Dry run",
            CommandOptionType.NoValue);

        CommandOption historyOption = app.Option("-h|--history",
            "Add history data",
            CommandOptionType.NoValue);

        app.OnExecute(() =>
        {
            context.Command = new SeedDatabaseCommand(
                new SeedDatabaseCommandOptions(context)
                {
                    DatabaseName = databaseArgument.Value,
                    ProfilePath = profileArgument.Value,
                    RepositoryPluginTag = repositoryTagArgument.Value,
                    SeederPluginTag = seederTagArgument.Value,
                    Count = countOption.HasValue() &&
                        int.TryParse(countOption.Value(), out int n) ? n : 100,
                    IsDryRun = dryOption.HasValue(),
                    HasHistory = historyOption.HasValue()
                });
            return 0;
        });
    }

    private static string LoadProfile(string path)
    {
        using StreamReader reader = File.OpenText(path);
        return reader.ReadToEnd();
    }

    public Task<int> Run()
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("SEED DATABASE\n");
        Console.ResetColor();

        Console.WriteLine($"Database: {_options.DatabaseName}\n" +
                          $"Profile file: {_options.ProfilePath}\n" +
                          $"Repository plugin tag: {_options.RepositoryPluginTag}\n" +
                          $"Seeders plugin tag: {_options.SeederPluginTag}\n" +
                          $"Count: {_options.Count}\n" +
                          $"Dry run: {_options.IsDryRun}\n" +
                          $"History: {_options.HasHistory}\n");
        Serilog.Log.Information("SEED DATABASE: " +
                     $"Database: {_options.DatabaseName}, " +
                     $"Profile file: {_options.ProfilePath}, " +
                     $"Repository plugin tag: {_options.RepositoryPluginTag}\n" +
                     $"Seeders plugin tag: {_options.SeederPluginTag}\n" +
                     $"Count: {_options.Count}, " +
                     $"Dry: {_options.IsDryRun}, " +
                     $"History: {_options.HasHistory}");

        // profile
        string profileContent = LoadProfile(_options.ProfilePath!);

        if (!_options.IsDryRun)
        {
            // create database if not exists
            string connection = string.Format(CultureInfo.InvariantCulture,
                _options.Configuration!.GetConnectionString("Mongo")!,
                _options.DatabaseName);

            IDatabaseManager manager = new MongoDatabaseManager();
            IDataProfileSerializer serializer = new JsonDataProfileSerializer();
            DataProfile profile = serializer.Read(profileContent);

            if (!manager.DatabaseExists(connection))
            {
                Console.WriteLine("Creating database...");
                Serilog.Log.Information(
                    $"Creating database {_options.DatabaseName}...");

                manager.CreateDatabase(connection, profile);

                Console.WriteLine("Database created.");
                Serilog.Log.Information("Database created.");
            }
        }

        // repository
        Console.WriteLine("Creating repository...");
        Serilog.Log.Information("Creating repository...");

        var repositoryProvider = PluginFactoryProvider
            .GetFromTag<IRepositoryProvider>(
            _options.RepositoryPluginTag);
        if (repositoryProvider == null)
        {
            throw new FileNotFoundException(
                "The requested repository provider tag " +
                _options.RepositoryPluginTag +
                " was not found among plugins in " +
                PluginFactoryProvider.GetPluginsDir());
        }
        repositoryProvider.ConnectionString = string.Format(
            _options.Configuration!.GetConnectionString("Mongo")!,
            _options.DatabaseName);
        ICadmusRepository? repository = _options.IsDryRun
            ? null : repositoryProvider.CreateRepository();

        // seeder
        var seederProvider = PluginFactoryProvider
            .GetFromTag<IPartSeederFactoryProvider>(_options.SeederPluginTag);
        if (seederProvider == null)
        {
            throw new FileNotFoundException(
                "The requested part seeders provider tag " +
                _options.SeederPluginTag +
                " was not found among plugins in " +
                PluginFactoryProvider.GetPluginsDir());
        }

        Console.WriteLine("Seeding items");
        PartSeederFactory factory = seederProvider.GetFactory(profileContent);
        CadmusSeeder seeder = new(factory);
        foreach (IItem item in seeder.GetItems(_options.Count))
        {
            Console.WriteLine($"{item}: {item.Parts.Count} parts");
            if (!_options.IsDryRun)
            {
                repository?.AddItem(item,_options.HasHistory);
                foreach (IPart part in item.Parts)
                {
                    repository?.AddPart(part, _options.HasHistory);
                }
            }
        }
        Console.WriteLine("Completed.");

        return Task.FromResult(0);
    }
}

internal class SeedDatabaseCommandOptions : CommandOptions<CadmusCliAppContext>
{
    public string? DatabaseName { get; set; }
    public string? ProfilePath { get; set; }
    public int Count { get; set; }
    public bool IsDryRun { get; set; }
    public bool HasHistory { get; set; }
    public string? RepositoryPluginTag { get; set; }
    public string? SeederPluginTag { get; set; }

    public SeedDatabaseCommandOptions(ICliAppContext options)
        : base((CadmusCliAppContext)options)
    {
    }
}
