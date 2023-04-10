using Cadmus.Core.Config;
using Cadmus.Graph;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Cadmus.Cli.Commands;

/// <summary>
/// Import graph presets command.
/// </summary>
/// <seealso cref="ICommand" />
internal sealed class ImportGraphPresetsCommand :
    AsyncCommand<AddGraphPresetsCommandSettings>
{
    private static void ImportMappings(Stream source, IGraphRepository repository,
        AddGraphPresetsCommandSettings settings)
    {
        // source id : graph id
        Dictionary<int, int> ids = new();

        JsonGraphPresetReader reader = new();
        foreach (NodeMapping mapping in reader.LoadMappings(source))
        {
            Console.WriteLine(mapping);
            if (!settings.IsDry)
            {
                // adjust IDs
                int sourceId = mapping.Id;
                mapping.Id = 0;
                if (mapping.ParentId != 0)
                    mapping.ParentId = ids[mapping.ParentId];
                repository.AddMapping(mapping);
                ids[sourceId] = mapping.Id;
            }
        }
    }

    private static void ImportNodes(Stream source, IGraphRepository repository,
        AddGraphPresetsCommandSettings settings)
    {
        JsonGraphPresetReader reader = new();

        foreach (UriNode node in reader.ReadNodes(source))
        {
            if (!settings.IsDry)
            {
                node.Id = repository.AddUri(node.Uri!);
                Console.WriteLine(node);
                repository.AddNode(node);
            }
            else
            {
                Console.WriteLine(node);
            }
        }
    }

    private static void ImportTriples(Stream source, IGraphRepository repository,
        AddGraphPresetsCommandSettings settings)
    {
        JsonGraphPresetReader reader = new();
        foreach (Triple triple in reader.ReadTriples(source))
        {
            if (!settings.IsDry) repository.AddTriple(triple);
            else Console.WriteLine(triple);
        }
    }

    private static void ImportThesauri(Stream source, IGraphRepository repository,
        AddGraphPresetsCommandSettings settings)
    {
        string json = new StreamReader(source, Encoding.UTF8).ReadToEnd();
        Thesaurus[] thesauri = JsonSerializer.Deserialize<Thesaurus[]>(json,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true
            })!;
        foreach (Thesaurus thesaurus in thesauri)
        {
            if (!settings.IsDry)
            {
                repository.AddThesaurus(thesaurus,
                    settings.ThesaurusIdAsRoot,
                    settings.ThesaurusIdPrefix);
            }
            Console.WriteLine(thesaurus);
        }
    }

    public override Task<int> ExecuteAsync(CommandContext context,
        AddGraphPresetsCommandSettings settings)
    {
        AnsiConsole.MarkupLine("[red underline]IMPORT INTO GRAPH[/]");
        AnsiConsole.MarkupLine($"Source: [cyan]{settings.SourcePath}[/]");
        AnsiConsole.MarkupLine($"Mode: [cyan]{settings.Mode}[/]");
        AnsiConsole.MarkupLine($"Database: [cyan]{settings.DatabaseName}[/]");
        if (!string.IsNullOrEmpty(settings.RepositoryPluginTag))
        {
            AnsiConsole.MarkupLine(
                $"Repository plugin tag: [cyan]{settings.RepositoryPluginTag}[/]");
        }
        AnsiConsole.MarkupLine(
            $"Dry mode: [cyan]{(settings.IsDry ? "yes" : "no")}[/]");

        if (char.ToLowerInvariant(settings.Mode) == 'h')
        {
            AnsiConsole.MarkupLine("Thesaurus ID as root: [cyan]" +
                $"{(settings.ThesaurusIdAsRoot ? "yes" : "no")}[/]");
            AnsiConsole.MarkupLine($"Thesaurus ID prefix: " +
                $"[cyan]{settings.ThesaurusIdPrefix}[/]");
        }

        IGraphRepository repository = GraphHelper.GetGraphRepository(
            settings.DatabaseName!);
        if (repository != null)
        {
            using Stream source = new FileStream(settings.SourcePath!,
                FileMode.Open, FileAccess.Read, FileShare.Read);

            switch (char.ToLowerInvariant(settings.Mode))
            {
                case 'm':
                    ImportMappings(source, repository, settings);
                    break;
                case 't':
                    ImportTriples(source, repository, settings);
                    break;
                case 'h':
                    ImportThesauri(source, repository, settings);
                    break;
                default:
                    ImportNodes(source, repository, settings);
                    break;
            }
        }

        return Task.FromResult(0);
    }
}

/// <summary>
/// Options for <see cref="ImportGraphPresetsCommand"/>
/// </summary>
internal class AddGraphPresetsCommandSettings : CommandSettings
{
    [CommandArgument(0, "<SourcePath>")]
    [Description("The path to the source file")]
    public string? SourcePath { get; set; }

    [CommandArgument(1, "<DatabaseName>")]
    [Description("The database name")]
    public string? DatabaseName { get; set; }

    [CommandOption("-t|--tag <RepositoryPluginTag>")]
    [Description("The repository factory plugin tag")]
    public string? RepositoryPluginTag { get; set; }

    [CommandOption("-m|--mode <ImportMode>")]
    [DefaultValue('n')]
    [Description("Import mode: (n)odes (t)riples (m)appings t(h)esauri")]
    public char Mode { get; set; }

    [CommandOption("-d|--dry")]
    [DefaultValue(false)]
    [Description("Dry mode - don't write to database")]
    public bool IsDry { get; set; }

    [CommandOption("-r|--root")]
    [DefaultValue(false)]
    [Description("True to import the thesaurus' ID as the root class node")]
    public bool ThesaurusIdAsRoot { get; set; }

    [CommandOption("-p|--prefix <ThesaurusIdPrefix>")]
    [Description("The prefix to add to each thesaurus' class node")]
    public string? ThesaurusIdPrefix { get; set; }

    public AddGraphPresetsCommandSettings()
    {
        Mode = 'n';
    }
}
