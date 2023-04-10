using Cadmus.Index.MySql;
using Cadmus.Index.Sql;
using Fusi.Tools.Data;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

internal sealed class BuildIndexSqlCommand :
    AsyncCommand<BuildIndexSqlCommandSettings>
{
    private readonly PagingOptions _options;
    private readonly SqlQueryBuilderBase _builder;

    public BuildIndexSqlCommand()
    {
        _options = new PagingOptions();
        _builder = new MySqlQueryBuilder();
    }

    public override Task<int> ExecuteAsync(CommandContext context,
        BuildIndexSqlCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[green underline]BUILD INDEX SQL[/]");

        if (!string.IsNullOrEmpty(settings.Query))
        {
            AnsiConsole.MarkupLine($"Query: [cyan]{settings.Query}[/]");
            var sql = _builder.BuildForItem(settings.Query, _options);
            AnsiConsole.MarkupLine($"[yellow]{sql.Item1}[/]");
            return Task.FromResult(0);
        }

        while (true)
        {
            AnsiConsole.MarkupLine("Enter query: ");
            string? query = Console.ReadLine();
            if (string.IsNullOrEmpty(query) || query == "quit") break;
            Console.WriteLine();

            var sql = _builder.BuildForItem(query, _options);
            AnsiConsole.MarkupLine($"[yellow]{sql.Item1}[/]");
        }

        return Task.FromResult(0);
    }
}

public class BuildIndexSqlCommandSettings : CommandSettings
{
    [CommandOption("-q|--query <QUERY>")]
    [Description("The query text")]
    public string? Query { get; set; }
}
