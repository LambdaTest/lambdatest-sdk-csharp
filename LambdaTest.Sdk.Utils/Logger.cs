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

            var logLevelStr = Environment.GetEnvironmentVariable("LT_SDK_LOG_LEVEL");
            if (string.IsNullOrEmpty(logLevelStr))
            {
                logLevelStr = "info";
            }
            else
            {
                logLevelStr = logLevelStr.ToLower();
            }

            switch (logLevelStr)
            {
                case "debug":
                    return LogLevel.Debug;
                case "warning":
                    return LogLevel.Warning;
                case "error":
                    return LogLevel.Error;
                case "critical":
                    return LogLevel.Critical;
                default:
                    return LogLevel.Information;
            }
        }

        public static ILogger CreateLogger(string packageName)
        {
            Initialize();
            
            if (loggerFactory == null)
            {
                throw new InvalidOperationException("Logger factory failed to initialize");
            }
            
            return loggerFactory.CreateLogger(packageName);
        }
    }
}