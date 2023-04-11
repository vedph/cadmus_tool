using Cadmus.Cli.Core;
using Cadmus.Core.Storage;
using System.IO;
using Cadmus.Core;
using Cadmus.Cli.Services;
using Cadmus.Seed;

namespace Cadmus.Cli.Commands;

internal static class CliHelper
{
    public static ICadmusRepository GetCadmusRepository(string? tag,
        string connStr)
    {
        IRepositoryProvider? provider =
            (string.IsNullOrEmpty(tag)
            ? new StandardRepositoryProvider()
            : PluginFactoryProvider.GetFromTag<IRepositoryProvider>(tag))
            ?? throw new FileNotFoundException(
                "The requested repository provider tag " +
                tag +
                " was not found among plugins in " +
                PluginFactoryProvider.GetPluginsDir());

        provider.ConnectionString = connStr;
        return provider.CreateRepository();
    }

    public static IPartSeederFactoryProvider GetSeederFactoryProvider(string? tag)
    {
        return string.IsNullOrEmpty(tag)
            ? new StandardPartSeederFactoryProvider()
            : PluginFactoryProvider.GetFromTag<IPartSeederFactoryProvider>(tag)
                ?? throw new FileNotFoundException(
                    "The requested part seeders provider tag " +
                    tag +
                    " was not found among plugins in " +
                    PluginFactoryProvider.GetPluginsDir());
    }
}
