﻿using Cadmus.Index.MySql;
using Cadmus.Index.PgSql;
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

    public BuildIndexSqlCommand()
    {
        _options = new PagingOptions();
    }

    public override Task<int> ExecuteAsync(CommandContext context,
        BuildIndexSqlCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[green underline]BUILD INDEX SQL[/]");

        try
        {
            SqlQueryBuilderBase builder =
                settings.DatabaseType.Equals("mysql",
                    StringComparison.InvariantCultureIgnoreCase)
                ? new MySqlQueryBuilder(settings.IsLegacy)
                : new PgSqlQueryBuilder();

            if (!string.IsNullOrEmpty(settings.Query))
            {
                AnsiConsole.MarkupLine($"Query: [cyan]{settings.Query}[/]");
                var sql = builder.BuildForItem(settings.Query, _options);
                AnsiConsole.MarkupLine($"[yellow]{sql.Item1}[/]");
                return Task.FromResult(0);
            }

            while (true)
            {
                AnsiConsole.MarkupLine("Enter query: ");
                string? query = Console.ReadLine();
                if (string.IsNullOrEmpty(query) || query == "quit") break;
                Console.WriteLine();

                var sql = builder.BuildForItem(query, _options);
                AnsiConsole.MarkupLine($"[yellow]{sql.Item1}[/]");
            }

            return Task.FromResult(0);
        }
        catch (Exception ex)
        {
            CliHelper.DisplayException(ex);
            return Task.FromResult(2);
        }
    }
}

public class BuildIndexSqlCommandSettings : CommandSettings
{
    [CommandOption("-q|--query <QUERY>")]
    [Description("The query text")]
    public string? Query { get; set; }

    [CommandOption("-t|--db-type <DatabaseType>")]
    [Description("The database type (pgsql or mysql)")]
    [DefaultValue("pgsql")]
    public string DatabaseType { get; set; }

    [CommandOption("-l|--legacy")]
    [Description("Whether the query uses legacy field names (MySql only)")]
    public bool IsLegacy { get; set; }

    public BuildIndexSqlCommandSettings()
    {
        DatabaseType = "pgsql";
    }
}
