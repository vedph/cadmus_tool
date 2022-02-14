using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CadmusTool.Commands
{
    internal class CommandOptions
    {
        private readonly AppOptions _appOptions;

        public IConfiguration Configuration => _appOptions.Configuration;
        public ILogger Logger => _appOptions.Logger;

        public CommandOptions(AppOptions options)
        {
            _appOptions = options;
        }
    }
}
