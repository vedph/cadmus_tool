using Cadmus.Cli.Core;
using Cadmus.Core.Storage;
using System.IO;
using Cadmus.Core;

namespace Cadmus.Cli.Commands;

internal static class CliHelper
{
    public static ICadmusRepository GetCadmusRepository(string tag,
        string connStr)
    {
        IRepositoryProvider? provider = PluginFactoryProvider
            .GetFromTag<IRepositoryProvider>(tag);
        if (provider == null)
        {
            throw new FileNotFoundException(
                "The requested repository provider tag " +
                tag +
                " was not found among plugins in " +
                PluginFactoryProvider.GetPluginsDir());
        }
        provider.ConnectionString = connStr;
        return provider.CreateRepository();
    }
}
