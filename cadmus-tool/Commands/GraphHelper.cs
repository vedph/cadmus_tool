using Cadmus.Cli.Services;
using Cadmus.Graph;
using Cadmus.Graph.MySql;
using Cadmus.Index.Sql;
using Microsoft.Extensions.Configuration;
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

    public static IGraphRepository GetGraphRepository(string dbName)
    {
        if (dbName is null) throw new ArgumentNullException(nameof(dbName));

        string cs = string.Format(CliAppContext.Configuration
            .GetConnectionString("Index")!, dbName);

        var repository = new MySqlGraphRepository();
        repository.Configure(new SqlOptions
        {
            ConnectionString = cs
        });
        return repository;
    }

    public static void UpdateGraphForDeletion(string id, string dbName)
    {
        if (id == null) throw new ArgumentNullException(nameof(id));
        if (dbName is null) throw new ArgumentNullException(nameof(dbName));

        IGraphRepository graphRepository = GetGraphRepository(dbName);
        if (graphRepository == null) return;

        graphRepository.DeleteGraphSet(id);
    }
}
