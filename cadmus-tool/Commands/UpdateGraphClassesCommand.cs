using Cadmus.Graph;
using Fusi.Tools;
using Microsoft.Extensions.CommandLineUtils;
using ShellProgressBar;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CadmusTool.Commands
{
    internal sealed class UpdateGraphClassesCommand : ICommand
    {
        private readonly GraphCommandOptions _options;

        public UpdateGraphClassesCommand(GraphCommandOptions options)
        {
            _options = options;
        }

        public static void Configure(CommandLineApplication command,
            AppOptions options)
        {
            command.Description = "Update nodes classes in the " +
                "specified database index.";
            command.HelpOption("-?|-h|--help");

            CommandArgument databaseArgument = command.Argument("[database]",
                "The database name");

            CommandArgument profileArgument = command.Argument("[profile]",
                "The indexer profile JSON file path");

            command.OnExecute(() =>
            {
                options.Command = new UpdateGraphClassesCommand(
                    new GraphCommandOptions(options)
                    {
                        DatabaseName = databaseArgument.Value,
                        MappingsPath = profileArgument.Value
                    });
                return 0;
            });
        }
        public async Task Run()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("UPDATE CLASS NODES IN GRAPH\n");
            Console.ResetColor();

            Console.WriteLine(
                $"Database: {_options.DatabaseName}\n" +
                $"Profile file: {_options.MappingsPath}\n");

            IGraphRepository repository = GraphHelper.GetGraphRepository(
                _options);
            if (repository == null) return;

            ProgressBarOptions options = CliHelper.GetProgressBarOptions();
            using var bar = new ProgressBar(100, "Updating...", options);

            await repository.UpdateNodeClassesAsync(CancellationToken.None,
                new Progress<ProgressReport>(r =>
                {
                    bar.Tick(r.Percent);
                }));

            Console.WriteLine("\nCompleted.");
        }
    }
}
