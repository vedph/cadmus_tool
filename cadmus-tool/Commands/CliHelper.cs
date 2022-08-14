using Cadmus.Cli.Core;
using Cadmus.Core.Storage;
using ShellProgressBar;
using System.IO;
using System;
using System.Runtime.InteropServices;

namespace CadmusTool.Commands
{
    internal static class CliHelper
    {
        public static ProgressBarOptions GetProgressBarOptions()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new ProgressBarOptions
                {
                    ProgressCharacter = '.',
                    ProgressBarOnBottom = true,
                    DisplayTimeInRealTime = false,
                    EnableTaskBarProgress = true
                }
                : new ProgressBarOptions
                {
                    ProgressCharacter = '.',
                    ProgressBarOnBottom = true,
                    DisplayTimeInRealTime = false
                };
        }

        public static ICadmusRepository GetCadmusRepository(string tag,
            string connStr, string dbName)
        {
            var repositoryProvider = PluginFactoryProvider
                .GetFromTag<ICliCadmusRepositoryProvider>(tag);
            if (repositoryProvider == null)
            {
                throw new FileNotFoundException(
                    "The requested repository provider tag " +
                    tag +
                    " was not found among plugins in " +
                    PluginFactoryProvider.GetPluginsDir());
            }
            repositoryProvider.ConnectionString = connStr;
            return repositoryProvider.CreateRepository(dbName);
        }
    }
}
