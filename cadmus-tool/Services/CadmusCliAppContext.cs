using Fusi.Cli.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;

namespace Cadmus.Cli.Services;

/// <summary>
/// CLI app context.
/// </summary>
/// <seealso cref="CliAppContext" />
public class CadmusCliAppContext : CliAppContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CadmusCliAppContext"/>
    /// class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <param name="logger">The logger.</param>
    public CadmusCliAppContext(IConfiguration? config, ILogger? logger)
        : base(config, logger)
    {
    }

    /// <summary>
    /// Gets the context service.
    /// </summary>
    /// <param name="dbName">The database name.</param>
    /// <exception cref="ArgumentNullException">dbName</exception>
    public virtual CadmusCliContextService GetContextService(string dbName)
    {
        if (dbName is null) throw new ArgumentNullException(nameof(dbName));

        return new CadmusCliContextService(
            new CadmusCliContextServiceConfig
            {
                DataConnectionString = string.Format(CultureInfo.InvariantCulture,
                    Configuration!.GetConnectionString("Mongo")!, dbName),
                IndexConnectionString = string.Format(CultureInfo.InvariantCulture,
                    Configuration!.GetConnectionString("Index")!, dbName),
                LocalDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                    "Assets")
            });
    }
}
