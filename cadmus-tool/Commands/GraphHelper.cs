using Cadmus.Cli.Services;
using Cadmus.Graph;
using Cadmus.Graph.Ef;
using Cadmus.Graph.Ef.MySql;
using Cadmus.Graph.Ef.PgSql;
using Fusi.DbManager.MySql;
using Fusi.DbManager.PgSql;
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

    public static void CreateGraphDatabase(string dbName, string dbType = "pgsql")
    {
        if (dbName is null) throw new ArgumentNullException(nameof(dbName));
        if (dbType is null) throw new ArgumentNullException(nameof(dbType));

        string cst = CliAppContext.Configuration.GetConnectionString(
            dbType == "mysql" ? "MyGraph" : "PgGraph")!;
        if (dbType == "mysql")
        {
            MySqlDbManager mgr = new(cst);
            mgr.CreateDatabase(dbName, EfMySqlGraphRepository.GetSchema(), null);
        }
        else
        {
            PgSqlDbManager mgr = new(cst);
            mgr.CreateDatabase(dbName, EfPgSqlGraphRepository.GetSchema(), null);
        }
    }

    public static IGraphRepository GetGraphRepository(string dbName,
        string dbType = "pgsql")
    {
        if (dbName is null) throw new ArgumentNullException(nameof(dbName));
        if (dbType is null) throw new ArgumentNullException(nameof(dbType));

        string cs = string.Format(CliAppContext.Configuration
            .GetConnectionString(dbType == "mysql" ? "MyGraph" : "PgGraph")!,
                                 dbName);

        EfGraphRepository repository = dbType == "mysql"
            ? new EfMySqlGraphRepository()
            : new EfPgSqlGraphRepository();
        repository.Configure(new EfGraphRepositoryOptions
        {
            ConnectionString = cs
        });
        return (IGraphRepository)repository;
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
