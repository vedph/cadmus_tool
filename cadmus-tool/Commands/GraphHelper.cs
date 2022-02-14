using Cadmus.Core;
using Cadmus.Index.Graph;
using Cadmus.Index.MySql;
using Cadmus.Index.Sql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace CadmusTool.Commands
{
    internal static class GraphHelper
    {
        public static string LoadProfile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            using StreamReader reader = File.OpenText(path);
            return reader.ReadToEnd();
        }

        public static IGraphRepository GetGraphRepository(
            GraphCommandOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string cs = string.Format(options.Configuration
                .GetConnectionString("Index"), options.DatabaseName);

            var repository = new MySqlGraphRepository();
            repository.Configure(new SqlOptions
            {
                ConnectionString = cs
            });
            return repository;
        }

        public static void UpdateGraph(IItem item,
            GraphCommandOptions options)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            IGraphRepository graphRepository = GetGraphRepository(options);
            if (graphRepository == null) return;

            options.Logger.LogInformation("Mapping " + item);
            NodeMapper mapper = new(graphRepository)
            {
                Logger = options.Logger
            };
            GraphSet set = mapper.MapItem(item);

            options.Logger.LogInformation("Updating graph " + set);
            graphRepository.UpdateGraph(set);
        }

        public static void UpdateGraph(IItem item, IPart part,
            IList<Tuple<string, string>> pins, GraphCommandOptions options)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (part == null)
                throw new ArgumentNullException(nameof(part));
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            IGraphRepository graphRepository = GetGraphRepository(options);
            if (graphRepository == null) return;

            options.Logger.LogInformation("Mapping " + part);
            NodeMapper mapper = new(graphRepository)
            {
                Logger = options.Logger
            };
            GraphSet set = mapper.MapPins(item, part, pins);

            options.Logger.LogInformation("Updating graph " + set);
            graphRepository.UpdateGraph(set);
        }

        public static void UpdateGraphForDeletion(string id,
            GraphCommandOptions options)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            IGraphRepository graphRepository = GetGraphRepository(options);
            if (graphRepository == null) return;

            options.Logger.LogInformation("Updating graph for deleted " + id);
            graphRepository.DeleteGraphSet(id);
        }
    }
}
