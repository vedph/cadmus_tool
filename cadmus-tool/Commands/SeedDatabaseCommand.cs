using Cadmus.Cli.Services;
using Cadmus.Core;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Mongo;
using Cadmus.Seed;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

internal sealed class SeedDatabaseCommand :
    AsyncCommand<SeedDatabaseCommandSettings>
{
    private static string LoadProfile(string path)
    {
        using StreamReader reader = File.OpenText(path);
        return reader.ReadToEnd();
    }

    public override Task<int> ExecuteAsync(CommandContext context,
        SeedDatabaseCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[red underline]SEED DATABASE[/]");
        AnsiConsole.MarkupLine($"Database: [cyan]{settings.DatabaseName}[/]");
        AnsiConsole.MarkupLine($"Profile file: [cyan]{settings.ProfilePath}[/]");
        if (!string.IsNullOrEmpty(settings.RepositoryPluginTag))
        {
            AnsiConsole.MarkupLine(
                $"Repository plugin tag: [cyan]{settings.RepositoryPluginTag}[/]");
        }
        if (!string.IsNullOrEmpty(settings.SeederPluginTag))
        {
            AnsiConsole.MarkupLine(
                $"Seeders plugin tag: [cyan]{settings.SeederPluginTag}[/]");
        }
        AnsiConsole.MarkupLine($"Count: [cyan]{settings.Count}[/]");
        AnsiConsole.MarkupLine($"Dry run: [cyan]{settings.IsDryRun}[/]");
        AnsiConsole.MarkupLine($"History: [cyan]{settings.HasHistory}[/]");

        Serilog.Log.Information("SEED DATABASE: " +
                     $"Database: {settings.DatabaseName}, " +
                     $"Profile file: {settings.ProfilePath}, " +
                     $"Repository plugin tag: {settings.RepositoryPluginTag}\n" +
                     $"Seeders plugin tag: {settings.SeederPluginTag}\n" +
                     $"Count: {settings.Count}, " +
                     $"Dry: {settings.IsDryRun}, " +
                     $"History: {settings.HasHistory}");

        AnsiConsole.Status().Start("Initializing...", ctx =>
        {
            // profile
            ctx.Status("Loading profile...");
            string profileContent = LoadProfile(settings.ProfilePath!);

            // database
            if (!settings.IsDryRun)
            {
                ctx.Status("Preparing database...");
                ctx.Spinner(Spinner.Known.Star);

                string connection = string.Format(CultureInfo.InvariantCulture,
                    CliAppContext.Configuration.GetConnectionString("Mongo")!,
                    settings.DatabaseName);

                IDatabaseManager manager = new MongoDatabaseManager();
                IDataProfileSerializer serializer = new JsonDataProfileSerializer();
                DataProfile profile = serializer.Read(profileContent);

                if (!manager.DatabaseExists(connection))
                {
                    manager.CreateDatabase(connection, profile);
                    Serilog.Log.Information("Database created.");
                }
            }

            // repository
            ctx.Status("Creating repository...");
            ICadmusRepository repository = CliHelper.GetCadmusRepository(
                settings.RepositoryPluginTag,
                CliAppContext.Configuration.GetConnectionString("Mongo")!);

            // seeder
            ctx.Status("Creating seeders factory...");
            IPartSeederFactoryProvider seederProvider =
                CliHelper.GetSeederFactoryProvider(settings.SeederPluginTag);

            ctx.Status("Seeding items");
            ctx.Spinner(Spinner.Known.Star);
            PartSeederFactory factory = seederProvider.GetFactory(profileContent);
            CadmusSeeder seeder = new(factory);

            foreach (IItem item in seeder.GetItems(settings.Count))
            {
                ctx.Status($"{item}: {item.Parts.Count} parts");
                if (!settings.IsDryRun)
                {
                    repository?.AddItem(item,settings.HasHistory);
                    foreach (IPart part in item.Parts)
                    {
                        repository?.AddPart(part, settings.HasHistory);
                    }
                }
            }
            ctx.Status("Completed.");
        });

        return Task.FromResult(0);
    }
}

internal class SeedDatabaseCommandSettings : CommandSettings
{
    [CommandArgument(0, "<DatabaseName>")]
    [Description("The database name")]
    public string? DatabaseName { get; set; }

    [CommandArgument(1, "<JsonProfilePath>")]
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

    [CommandOption("-g|--tag <RepositoryPluginTag>")]
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
