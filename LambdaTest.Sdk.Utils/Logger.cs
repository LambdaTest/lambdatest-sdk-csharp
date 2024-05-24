using System;
using Microsoft.Extensions.Logging;

namespace LambdaTest.Sdk.Utils
{
    public static class Logger
    {
        private static ILoggerFactory loggerFactory;
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (!isInitialized)
            {
                loggerFactory = LoggerFactory.Create(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(GetLogLevel());
                });
                isInitialized = true;
            }
        }

        private static LogLevel GetLogLevel()
        {
            var debug = Environment.GetEnvironmentVariable("LT_SDK_DEBUG");
            if (debug != null && debug.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return LogLevel.Debug;
            }

            var logLevelStr = Environment.GetEnvironmentVariable("LT_SDK_LOG_LEVEL")?.ToLower() ?? "info";
            return logLevelStr switch
            {
                "debug" => LogLevel.Debug,
                "warning" => LogLevel.Warning,
                "error" => LogLevel.Error,
                "critical" => LogLevel.Critical,
                _ => LogLevel.Information,
            };
        }

        public static ILogger CreateLogger(string packageName)
        {
            Initialize();
            return loggerFactory.CreateLogger(packageName);
        }
    }
}
