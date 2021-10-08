using Cadmus.Cli.Core;
using Cadmus.Core.Storage;
using Cadmus.Index;
using Cadmus.Index.Config;
using CadmusTool.Services;
using Fusi.Tools;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using ShellProgressBar;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace CadmusTool.Commands
{
    public sealed class IndexDatabaseCommand : ICommand
    {
        private readonly IndexDatabaseCommandOptions _options;

        public IndexDatabaseCommand(IndexDatabaseCommandOptions options)
        {
            _options = options;
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Index a Cadmus MongoDB database " +
                                  "from the specified indexer profile.";
            command.HelpOption("-?|-h|--help");

            CommandArgument databaseArgument = command.Argument("[database]",
                "The database name");

            CommandArgument profileArgument = command.Argument("[profile]",
                "The indexer profile JSON file path");

            CommandArgument repositoryTagArgument = command.Argument("[tag]",
                "The repository factory provider plugin tag.");

            CommandOption clearOption = command.Option("-c|--clear",
                "Clear before indexing", CommandOptionType.NoValue);

            command.OnExecute(() =>
            {
            options.Command = new IndexDatabaseCommand(
                new IndexDatabaseCommandOptions
                {
                    AppOptions = options,
                    DatabaseName = databaseArgument.Value,
                    ProfilePath = profileArgument.Value,
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

        public async Task Run()
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

            string profileContent = LoadProfile(_options.ProfilePath);

            string cs = string.Format(_options.AppOptions.Configuration
                .GetConnectionString("Index"), _options.DatabaseName);
            IItemIndexFactoryProvider provider =
                new StandardItemIndexFactoryProvider(cs);
            ItemIndexFactory factory = provider.GetFactory(profileContent);
            IItemIndexWriter writer = factory.GetItemIndexWriter();

            var options = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new ProgressBarOptions
                {
                    ProgressCharacter = '.',
                    ProgressBarOnBottom = true,
                    DisplayTimeInRealTime = false,
                    EnableTaskBarProgress = true
                }
                : new ProgressBarOptions
                {
                    ProgressCharacter = '.',
                    ProgressBarOnBottom = true,
                    DisplayTimeInRealTime = false
                };

            // repository
            Console.WriteLine("Creating repository...");
            Serilog.Log.Information("Creating repository...");

            var repositoryProvider = PluginFactoryProvider
                .GetFromTag<ICliRepositoryFactoryProvider>(
                _options.RepositoryPluginTag);
            if (repositoryProvider == null)
            {
                throw new FileNotFoundException(
                    "The requested repository provider tag " +
                    _options.RepositoryPluginTag +
                    " was not found among plugins in " +
                    PluginFactoryProvider.GetPluginsDir());
            }
            repositoryProvider.ConnectionString = _options.AppOptions.Configuration
                .GetConnectionString("Mongo");
            ICadmusRepository repository = repositoryProvider.CreateRepository(
                _options.DatabaseName);

            using (var bar = new ProgressBar(100, "Indexing...", options))
            {
                ItemIndexer indexer = new ItemIndexer(writer);
                if (_options.ClearDatabase) await indexer.Clear();

                indexer.Build(repository, new ItemFilter(),
                    CancellationToken.None,
                    new Progress<ProgressReport>(
                        r => bar.Tick(r.Percent, r.Message)));
            }
            writer.Close();
        }
    }

    public class IndexDatabaseCommandOptions : CommandOptions
    {
        public string DatabaseName { get; set; }
        public string ProfilePath { get; set; }
        public string RepositoryPluginTag { get; set; }
        public bool ClearDatabase { get; set; }
    }
}

