using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Log4Extension
{
    public interface ILog4LoggerSettings
    {
        IChangeToken ChangeToken { get; }

        bool IncludeScopes { get; }

        string Log4netConfigPath { get; }

        bool TryGetSwitch(string name, out LogLevel level);

        ILog4LoggerSettings Reload();
    }
}
