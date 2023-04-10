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
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("INDEX DATABASE\n");
        Console.ResetColor();

        Console.WriteLine($"Database: {_options.DatabaseName}\n" +
                          $"Profile file: {_options.ProfilePath}\n" +
                          $"Repository plugin tag: {_options.RepositoryPluginTag}\n" +
                          $"Clear: {_options.ClearDatabase}\n");
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
        Serilog.Log.Information("Creating repository...");

        var repositoryProvider = (string.IsNullOrEmpty(_options.RepositoryPluginTag)
            ? new StandardRepositoryProvider()
            : PluginFactoryProvider.GetFromTag<IRepositoryProvider>(
                _options.RepositoryPluginTag)) ??
                throw new FileNotFoundException(
                    "The requested repository provider tag " +
                    _options.RepositoryPluginTag +
                    " was not found among plugins in " +
                    PluginFactoryProvider.GetPluginsDir());

        repositoryProvider.ConnectionString = string.Format(
            CliAppContext.Configuration.GetConnectionString("Mongo")!,
            _options.DatabaseName);

        ICadmusRepository repository = repositoryProvider.CreateRepository();
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

    [CommandArgument(1, "<ProfilePath>")]
    [Description("The indexer profile JSON file path")]
    public string? ProfilePath { get; set; }

    [CommandOption("-t|--tag <RepositoryPluginTag>")]
    [Description("The repository factory plugin tag")]
    public string? RepositoryPluginTag { get; set; }

    [CommandOption("-c|--clear")]
    [Description("Clear before indexing")]
    public bool ClearDatabase { get; set; }
}
