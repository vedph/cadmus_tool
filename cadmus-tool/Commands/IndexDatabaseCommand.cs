using Cadmus.Cli.Core;
using Cadmus.Cli.Services;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.Index;
using Cadmus.Index.Config;
using Fusi.Tools;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

internal sealed class IndexDatabaseCommand :
    AsyncCommand<IndexDatabaseCommandSettings>
{
    private readonly IndexDatabaseCommandSettings _options;

    public IndexDatabaseCommand(IndexDatabaseCommandSettings options)
    {
        _options = options;
    }

    private static string LoadProfile(string path)
    {
        using StreamReader reader = File.OpenText(path);
        return reader.ReadToEnd();
    }

    public async override Task<int> ExecuteAsync(CommandContext context,
        IndexDatabaseCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[red underline]INDEX DATABASE[/]");
        AnsiConsole.MarkupLine($"Database: [cyan]{_options.DatabaseName}[/]");
        AnsiConsole.MarkupLine($"Profile file: [cyan]{_options.ProfilePath}[/]");
        if (!string.IsNullOrEmpty(_options.RepositoryPluginTag))
        {
            AnsiConsole.MarkupLine(
                $"Repository plugin tag: [cyan]{_options.RepositoryPluginTag}[/]");
        }
        AnsiConsole.MarkupLine($"Clear: [cyan]{_options.ClearDatabase}[/]\n");

        Serilog.Log.Information("INDEX DATABASE: " +
                     $"Database: {_options.DatabaseName}, " +
                     $"Profile file: {_options.ProfilePath}, " +
                     $"Repository plugin tag: {_options.RepositoryPluginTag}\n" +
                     $"Clear: {_options.ClearDatabase}");

        string profileContent = LoadProfile(_options.ProfilePath!);

        string cs = string.Format(CliAppContext.Configuration
            .GetConnectionString("Index")!, _options.DatabaseName);
        IItemIndexFactoryProvider provider =
            new StandardItemIndexFactoryProvider(cs);
        ItemIndexFactory factory = provider.GetFactory(profileContent);
        IItemIndexWriter? writer = factory.GetItemIndexWriter()
            ?? throw new InvalidOperationException(
                "Unable to instantiate item index writer");

        // repository
        Console.WriteLine("Creating repository...");

        ICadmusRepository repository = CliHelper.GetCadmusRepository(
            settings.RepositoryPluginTag,
            CliAppContext.Configuration.GetConnectionString("Mongo")!);

        await AnsiConsole.Progress().StartAsync(async ctx =>
            {
                ProgressTask task = ctx.AddTask("Indexing database");
                ItemIndexer indexer = new(writer)
                {
                    Logger = CliAppContext.Logger
                };
                if (_options.ClearDatabase) await indexer.Clear();

                indexer.Build(repository, new ItemFilter(),
                    CancellationToken.None,
                    new Progress<ProgressReport>(
                        r => task.Increment(r.Percent - task.Value)));

                task.Increment(100 - task.Value);
            });
        writer.Close();
        return 0;
    }
}

internal class IndexDatabaseCommandSettings : CommandSettings
{
    [CommandArgument(0, "<DatabaseName>")]
    [Description("The database name")]
    public string? DatabaseName { get; set; }

    [CommandArgument(1, "<JsonProfilePath>")]
    [Description("The indexer profile JSON file path")]
    public string? ProfilePath { get; set; }

    [CommandOption("-t|--tag <RepositoryPluginTag>")]
    [Description("The repository factory plugin tag")]
    public string? RepositoryPluginTag { get; set; }

    [CommandOption("-c|--clear")]
    [Description("Clear before indexing")]
    public bool ClearDatabase { get; set; }
}
