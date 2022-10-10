using Cadmus.Cli.Core;
using Cadmus.Core.Storage;
using ShellProgressBar;
using System.IO;
using System.Runtime.InteropServices;
using Cadmus.Core;

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
            string connStr)
        {
            IRepositoryProvider provider = PluginFactoryProvider
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
}
