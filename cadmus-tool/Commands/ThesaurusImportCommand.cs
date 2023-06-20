using Cadmus.Cli.Services;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Cadmus.Import;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

public sealed class ThesaurusImportCommand :
    AsyncCommand<ThesaurusImportCommandSettings>
{
    private static ImportUpdateMode GetMode(char c)
    {
        // return ImportUpdateMode enum according to c R P S
        return char.ToUpperInvariant(c) switch
        {
            'R' => ImportUpdateMode.Replace,
            'P' => ImportUpdateMode.Patch,
            'S' => ImportUpdateMode.Synch,
            _ => throw new ArgumentException("Invalid mode", nameof(c)),
        };
    }

    public override Task<int> ExecuteAsync(
        CommandContext context, ThesaurusImportCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[red underline]IMPORT THESAURI[/]");
        AnsiConsole.MarkupLine($"Database: [cyan]{settings.DatabaseName}[/]");
        AnsiConsole.MarkupLine($"Database Type: [cyan]{settings.DatabaseType}[/]");
        AnsiConsole.MarkupLine($"Input: [cyan]{settings.InputFileMask}[/]");
        AnsiConsole.MarkupLine($"Mode: [cyan]{settings.Mode}[/]");
        AnsiConsole.MarkupLine(
            $"Dry run: [cyan]{(settings.IsDryRun ? "yes" : "no")}[/]");

        // repository
        AnsiConsole.MarkupLine("Creating repository...");
        string cs = string.Format(
            CliAppContext.Configuration.GetConnectionString("Mongo")!,
            settings.DatabaseName);
        ICadmusRepository repository = CliHelper.GetCadmusRepository(null, cs);

        // import
        int n = 0;
        foreach (string path in Directory.EnumerateFiles(
            Path.GetDirectoryName(settings.InputFileMask) ?? "",
            Path.GetFileName(settings.InputFileMask)!)
            .OrderBy(s => s))
        {
            // load thesaurus
            AnsiConsole.MarkupLine($"[yellow]{++n:000}[/] [green]{path}[/]");

            using JsonThesaurusReader reader = new(File.ReadAllText(path));
            Thesaurus? source;
            while ((source = reader.Next()) != null)
            {
                // fetch from repository
                Thesaurus? target = repository.GetThesaurus(source.Id);

                // import
                Thesaurus result = ThesaurusHelper.CopyThesaurus(source, target,
                    GetMode(settings.Mode));

                // save
                if (!settings.IsDryRun) repository.AddThesaurus(result);
            }
        }
        return Task.FromResult(0);
    }
}

public class ThesaurusImportCommandSettings : CommandSettings
{
    [CommandArgument(0, "<InputFileMask>")]
    [Description("The input files mask")]
    public string? InputFileMask { get; set; }

    [CommandArgument(1, "<DatabaseName>")]
    [Description("The database name")]
    public string? DatabaseName { get; set; }

    [CommandOption("-t|--db-type <pgsql|mysql>")]
    [Description("The database type (pgsql or mysql)")]
    [DefaultValue("pgsql")]
    public string DatabaseType { get; set; }

    [CommandOption("-m|--mode <Mode:R|P|S>")]
    [Description("The import mode: R=replace, P=patch, S=synch")]
    [DefaultValue('R')]
    public char Mode { get; set; }

    [CommandOption("-d|--dry")]
    [Description("Dry run")]
    public bool IsDryRun { get; set; }

    public ThesaurusImportCommandSettings()
    {
        DatabaseType = "pgsql";
        Mode = 'R';
    }
}