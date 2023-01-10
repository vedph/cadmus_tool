using Cadmus.Graph;
using Cadmus.Graph.MySql;
using Cadmus.Index.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Cadmus.Cli.Commands;

internal static class GraphHelper
{
    public static string LoadText(string path)
    {
        if (path == null) throw new ArgumentNullException(nameof(path));

        using StreamReader reader = File.OpenText(path);
        return reader.ReadToEnd();
    }

    public static IList<NodeMapping> ParseMappings(string json)
    {
        List<NodeMapping> mappings = new();
        JsonSerializerOptions options = new()
        {
            AllowTrailingCommas = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new NodeMappingOutputJsonConverter());

        mappings.AddRange(JsonSerializer.Deserialize<IList<NodeMapping>>(
            json ?? "{}",
            options) ?? Array.Empty<NodeMapping>());

        return mappings;
    }

    public static IList<NodeMapping> LoadMappings(string path)
    {
        return ParseMappings(LoadText(path));
    }

    public static IGraphRepository GetGraphRepository(
        GraphCommandOptions options)
    {
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        string cs = string.Format(options.Configuration!
            .GetConnectionString("Index")!, options.DatabaseName);

        var repository = new MySqlGraphRepository();
        repository.Configure(new SqlOptions
        {
            ConnectionString = cs
        });
        return repository;
    }

    public static void UpdateGraphForDeletion(string id,
        GraphCommandOptions options)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        if (options == null)
            throw new ArgumentNullException(nameof(options));

        IGraphRepository graphRepository = GetGraphRepository(options);
        if (graphRepository == null) return;

        options.Logger?.LogInformation("Updating graph for deleted {Id}", id);
        graphRepository.DeleteGraphSet(id);
    }
}
