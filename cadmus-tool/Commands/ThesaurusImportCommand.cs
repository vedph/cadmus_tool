using Cadmus.Cli.Services;
using Cadmus.Core.Config;
using Cadmus.Core.Storage;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

public sealed class ThesaurusImportCommand :
    AsyncCommand<ThesaurusImportCommandSettings>
{
    private static Thesaurus CopyThesaurus(Thesaurus source, Thesaurus? target,
        char mode)
    {
        Thesaurus result = new();

        // replace: just copy
        if (mode == 'R')
        {
            foreach (ThesaurusEntry se in source.Entries) result.AddEntry(se);
            return result;
        }

        // patch/synch thesaurus target with source:
        // - add source entries missing in target,
        // - update source entries existing in target.
        if (target != null)
        {
            foreach (ThesaurusEntry te in target.Entries) result.AddEntry(te);
        }

        foreach (ThesaurusEntry se in source.Entries)
        {
            ThesaurusEntry? te = result.Entries.FirstOrDefault(
                e => e.Id == se.Id);
            if (te == null) result.Entries.Add(se);
            else te.Value = se.Value;
        }

        // synch thesaurus target with source:
        // - remove target entries missing in source.
        if (mode == 'S')
        {
            foreach (ThesaurusEntry te in
                from ThesaurusEntry te in result.Entries
                where source.Entries.All(e => e.Id != te.Id)
                select te)
            {
                result.Entries.Remove(te);
            }
        }
        return result;
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
        JsonSerializerOptions options = new()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
        };

        // import
        int n = 0;
        foreach (string path in Directory.EnumerateFiles(
            Path.GetDirectoryName(settings.InputFileMask) ?? "",
            Path.GetFileName(settings.InputFileMask)!)
            .OrderBy(s => s))
        {
            // load thesaurus
            AnsiConsole.MarkupLine($"[yellow]{++n:000}[/] [green]{path}[/]");
            Thesaurus? source =
                Path.GetExtension(path).ToLowerInvariant() switch
                {
                    ".json" => JsonSerializer.Deserialize<Thesaurus>(
                        File.ReadAllText(path), options),
                    _ => throw new InvalidOperationException(
                        "Unsupported file type: " + path),
                } ?? throw new InvalidOperationException("Invalid thesaurus in "
                    + path);

            // fetch from repository
            Thesaurus? target = repository.GetThesaurus(source.Id);
            Thesaurus result = CopyThesaurus(source, target,
                char.ToUpperInvariant(settings.Mode));

            // save
            if (!settings.IsDryRun) repository.AddThesaurus(result);
        }
        return Task.FromResult(0);
    }
}

public class ThesaurusImportCommandSettings : CommandSettings
{
    [CommandArgument(0, "<DatabaseName>")]
    [Description("The database name")]
    public string? DatabaseName { get; set; }

    [CommandArgument(1, "<InputFileMask>")]
    [Description("The input files mask")]
    public string? InputFileMask { get; set; }

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