using Microsoft.Extensions.Logging;

namespace Ethanol.Cli
{
    internal class ContextProvider : ConsoleAppBase
    {
        private readonly ILogger _logger;

        public ContextProvider(ILogger<ContextProvider> logger)
        {
            this._logger = logger;
        }
    }
}