using Cadmus.Cli.Core;
using Cadmus.Cli.Services;
using Cadmus.Core;
using Cadmus.Core.Storage;
using Cadmus.Index;
using Cadmus.Index.Config;
using Fusi.Cli.Commands;
using Fusi.Tools;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using ShellProgressBar;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

internal sealed class IndexDatabaseCommand : ICommand
{
    private readonly IndexDatabaseCommandOptions _options;

    public IndexDatabaseCommand(IndexDatabaseCommandOptions options)
    {
        _options = options;
    }

    public static void Configure(CommandLineApplication app,
        ICliAppContext context)
    {
        app.Description = "Index a Cadmus MongoDB database " +
                              "from the specified indexer profile.";
        app.HelpOption("-?|-h|--help");

        CommandArgument databaseArgument = app.Argument("[database]",
            "The database name");

        CommandArgument profileArgument = app.Argument("[profile]",
            "The indexer profile JSON file path");

        CommandArgument repositoryTagArgument = app.Argument("[tag]",
            "The repository factory provider plugin tag.");

        CommandOption clearOption = app.Option("-c|--clear",
            "Clear before indexing", CommandOptionType.NoValue);

        app.OnExecute(() =>
        {
        context.Command = new IndexDatabaseCommand(
            new IndexDatabaseCommandOptions(context)
            {
                DatabaseName = databaseArgument.Value,
                ProfilePath = profileArgument.Value,
                RepositoryPluginTag = repositoryTagArgument.Value,
                ClearDatabase = clearOption.HasValue()
            });
            return 0;
        });
    }

    private static string LoadProfile(string path)
    {
        using StreamReader reader = File.OpenText(path);
        return reader.ReadToEnd();
    }

    public async Task<int> Run()
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

        string cs = string.Format(_options.Configuration!
            .GetConnectionString("Index")!, _options.DatabaseName);
        IItemIndexFactoryProvider provider =
            new StandardItemIndexFactoryProvider(cs);
        ItemIndexFactory factory = provider.GetFactory(profileContent);
        IItemIndexWriter? writer = factory.GetItemIndexWriter();
        if (writer == null)
        {
            throw new InvalidOperationException(
                "Unable to instantiate item index writer");
        }

        // repository
        Console.WriteLine("Creating repository...");
        Serilog.Log.Information("Creating repository...");

        var repositoryProvider = string.IsNullOrEmpty(_options.RepositoryPluginTag)
            ? new StandardRepositoryProvider()
            : PluginFactoryProvider.GetFromTag<IRepositoryProvider>(
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
        ICadmusRepository repository = repositoryProvider.CreateRepository();

        var options = CliHelper.GetProgressBarOptions();
        using (var bar = new ProgressBar(100, "Indexing...", options))
        {
            ItemIndexer indexer = new(writer)
            {
                Logger = _options.Logger
            };
            if (_options.ClearDatabase) await indexer.Clear();

            indexer.Build(repository, new ItemFilter(),
                CancellationToken.None,
                new Progress<ProgressReport>(
                    r => bar.Tick(r.Percent, r.Message)));

            bar.Tick(100);
        }
        writer.Close();
        return 0;
    }
}

internal class IndexDatabaseCommandOptions : CommandOptions<CadmusCliAppContext>
{
    public string? DatabaseName { get; set; }
    public string? ProfilePath { get; set; }
    public string? RepositoryPluginTag { get; set; }
    public bool ClearDatabase { get; set; }

    public IndexDatabaseCommandOptions(ICliAppContext options)
        : base((CadmusCliAppContext)options)
    {
    }
}
