using ShellProgressBar;
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
    }
}
