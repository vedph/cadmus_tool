using Cadmus.Cli.Core;
using Cadmus.Cli.Services;
using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Mongo;
using Cadmus.Seed;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

internal sealed class SeedDatabaseCommand :
    AsyncCommand<SeedDatabaseCommandSettings>
{
    private readonly SeedDatabaseCommandSettings _options;

    public SeedDatabaseCommand(SeedDatabaseCommandSettings options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    private static string LoadProfile(string path)
    {
        using StreamReader reader = File.OpenText(path);
        return reader.ReadToEnd();
    }

    public override Task<int> ExecuteAsync(CommandContext context,
        SeedDatabaseCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[green underline]BUILD INDEX SQL[/]");

        AnsiConsole.MarkupLine($"Database: [cyan]{_options.DatabaseName}[/]");
        AnsiConsole.MarkupLine($"Profile file: [cyan]{_options.ProfilePath}[/]");
        if (!string.IsNullOrEmpty(_options.RepositoryPluginTag))
        {
            AnsiConsole.MarkupLine(
                $"Repository plugin tag: [cyan]{_options.RepositoryPluginTag}[/]");
        }
        if (!string.IsNullOrEmpty(_options.SeederPluginTag))
        {
            AnsiConsole.MarkupLine(
                $"Seeders plugin tag: [cyan]{_options.SeederPluginTag}[/]");
        }
        AnsiConsole.MarkupLine($"Count: [cyan]{_options.Count}[/]");
        AnsiConsole.MarkupLine($"Dry run: [cyan]{_options.IsDryRun}[/]");
        AnsiConsole.MarkupLine($"History: [cyan]{_options.HasHistory}[/]");

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
                CliAppContext.Configuration.GetConnectionString("Mongo")!,
                _options.DatabaseName);

            IDatabaseManager manager = new MongoDatabaseManager();
            IDataProfileSerializer serializer = new JsonDataProfileSerializer();
            DataProfile profile = serializer.Read(profileContent);

            if (!manager.DatabaseExists(connection))
            {
                AnsiConsole.MarkupLine("Creating database...");
                Serilog.Log.Information(
                    $"Creating database {_options.DatabaseName}...");

                manager.CreateDatabase(connection, profile);

                AnsiConsole.MarkupLine("Database created.");
                Serilog.Log.Information("Database created.");
            }
        }

        // repository
        AnsiConsole.MarkupLine("Creating repository...");
        Serilog.Log.Information("Creating repository...");

        var repositoryProvider = PluginFactoryProvider
            .GetFromTag<IRepositoryProvider>(
            _options.RepositoryPluginTag) ??
            throw new FileNotFoundException(
                "The requested repository provider tag " +
                _options.RepositoryPluginTag +
                " was not found among plugins in " +
                PluginFactoryProvider.GetPluginsDir());

        repositoryProvider.ConnectionString = string.Format(
            CliAppContext.Configuration.GetConnectionString("Mongo")!,
            _options.DatabaseName);
        ICadmusRepository? repository = _options.IsDryRun
            ? null : repositoryProvider.CreateRepository();

        // seeder
        var seederProvider = PluginFactoryProvider
            .GetFromTag<IPartSeederFactoryProvider>(_options.SeederPluginTag)
            ?? throw new FileNotFoundException(
                "The requested part seeders provider tag " +
                _options.SeederPluginTag +
                " was not found among plugins in " +
                PluginFactoryProvider.GetPluginsDir());

        AnsiConsole.MarkupLine("Seeding items");
        PartSeederFactory factory = seederProvider.GetFactory(profileContent);
        CadmusSeeder seeder = new(factory);
        foreach (IItem item in seeder.GetItems(_options.Count))
        {
            AnsiConsole.MarkupLine($"{item}: {item.Parts.Count} parts");
            if (!_options.IsDryRun)
            {
                repository?.AddItem(item,_options.HasHistory);
                foreach (IPart part in item.Parts)
                {
                    repository?.AddPart(part, _options.HasHistory);
                }
            }
        }
        AnsiConsole.MarkupLine("Completed.");

        return Task.FromResult(0);
    }
}

internal class SeedDatabaseCommandSettings : CommandSettings
{
    [CommandArgument(0, "<DatabaseName>")]
    [Description("The database name")]
    public string? DatabaseName { get; set; }

    [CommandArgument(1, "<ProfilePath>")]
    [Description("The seed profile JSON file path")]
    public string? ProfilePath { get; set; }

    [CommandOption("-c|--count <Count>")]
    [DefaultValue(100)]
    public int Count { get; set; }

    [CommandOption("-d|--dry")]
    [Description("Dry run")]
    public bool IsDryRun { get; set; }

    [CommandOption("-h|--history")]
    [Description("Add history data")]
    public bool HasHistory { get; set; }

    [CommandOption("-t|--tag <RepositoryPluginTag>")]
    [Description("The repository factory plugin tag")]
    public string? RepositoryPluginTag { get; set; }

    [CommandOption("-s|--seed-tag <SeederPluginTag>")]
    [Description("The parts seeder factory plugin tag")]
    public string? SeederPluginTag { get; set; }

    public SeedDatabaseCommandSettings()
    {
        Count = 100;
    }
}
