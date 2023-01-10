namespace Cadmus.Cli.Services;

/// <summary>
/// CLI context service.
/// </summary>
public sealed class CadmusCliContextService
{
    public CadmusCliContextServiceConfig Configuration { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CadmusCliContextService"/>
    /// class.
    /// </summary>
    /// <param name="config">The configuration.</param>
    public CadmusCliContextService(CadmusCliContextServiceConfig config)
    {
        Configuration = config;
    }
}

/// <summary>
/// Configuration for <see cref="CadmusCliContextService"/>.
/// </summary>
public class CadmusCliContextServiceConfig
{
    /// <summary>
    /// Gets or sets the connection string to the data database.
    /// </summary>
    public string? DataConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the connection string to the index database.
    /// </summary>
    public string? IndexConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the local directory to use when loading resources
    /// from the local file system.
    /// </summary>
    public string? LocalDirectory { get; set; }
}
