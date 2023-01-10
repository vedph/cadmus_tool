using Cadmus.Graph;
using Fusi.Cli.Commands;
using Fusi.Tools;
using Microsoft.Extensions.CommandLineUtils;
using ShellProgressBar;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

internal sealed class UpdateGraphClassesCommand : ICommand
{
    private readonly GraphCommandOptions _options;

    public UpdateGraphClassesCommand(GraphCommandOptions options)
    {
        _options = options;
    }

    public static void Configure(CommandLineApplication app,
        ICliAppContext context)
    {
        app.Description = "Update nodes classes in the " +
            "specified database index.";
        app.HelpOption("-?|-h|--help");

        CommandArgument databaseArgument = app.Argument("[database]",
            "The database name");

        CommandArgument profileArgument = app.Argument("[profile]",
            "The indexer profile JSON file path");

        app.OnExecute(() =>
        {
            context.Command = new UpdateGraphClassesCommand(
                new GraphCommandOptions(context)
                {
                    DatabaseName = databaseArgument.Value,
                    MappingsPath = profileArgument.Value
                });
            return 0;
        });
    }

    public async Task<int> Run()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("UPDATE CLASS NODES IN GRAPH\n");
        Console.ResetColor();

        Console.WriteLine(
            $"Database: {_options.DatabaseName}\n" +
            $"Profile file: {_options.MappingsPath}\n");

        IGraphRepository repository = GraphHelper.GetGraphRepository(
            _options);
        if (repository == null) return 2;

        ProgressBarOptions options = CliHelper.GetProgressBarOptions();
        using var bar = new ProgressBar(100, "Updating...", options);

        await repository.UpdateNodeClassesAsync(CancellationToken.None,
            new Progress<ProgressReport>(r =>
            {
                bar.Tick(r.Percent);
            }));

        Console.WriteLine("\nCompleted.");
        return 0;
    }
}
