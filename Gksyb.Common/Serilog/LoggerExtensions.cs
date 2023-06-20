namespace Microsoft.Extensions.Logging
{
    public static class AssemblyLoadContextExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2254:模板应为静态表达式", Justification = "<挂起>")]
        public static void Log(this ILogger logger, LogLevel logLevel, LogPath logPath, string message, params object[] args)
        {
            if (args.Length == 0)
            {
                logger.Log(logLevel, $"{{{nameof(LogPath)}}} {{message}}", logPath, message);
                return;
            }
            var objs = new object[args.Length + 1];
            objs[0] = logPath;
            if (args.Length > 0) args.CopyTo(objs, 1);
            logger.Log(logLevel, message: $"{{{nameof(LogPath)}}} {message}", objs);
        }

        public static void LogCritical(this ILogger logger, LogPath logPath, string message, params object[] args)
        {
            logger.Log(LogLevel.Critical, logPath, message, args);
        }

        public static void LogDebug(this ILogger logger, LogPath logPath, string message, params object[] args)
        {
            logger.Log(LogLevel.Debug, logPath, message, args);
        }

        public static void LogError(this ILogger logger, LogPath logPath, string message, params object[] args)
        {
            logger.Log(LogLevel.Error, logPath, message, args);
        }

        public static void LogInformation(this ILogger logger, LogPath logPath, string message, params object[] args)
        {
            logger.Log(LogLevel.Information, logPath, message, args);
        }

        public static void LogTrace(this ILogger logger, LogPath logPath, string message, params object[] args)
        {
            logger.Log(LogLevel.Trace, logPath, message, args);
        }

        public static void LogWarning(this ILogger logger, LogPath logPath, string message, params object[] args)
        {
            logger.Log(LogLevel.Warning, logPath, message, args);
        }
    }

    /// <summary>
    /// 日志路径
    /// </summary>
    public class LogPath : IFormattable
    {
        /// <summary>
        /// 默认日志位置
        /// </summary>
        public static LogPath Default { get; } = new("");

        public LogPath(string path)
        {
            Path = string.IsNullOrWhiteSpace(path) ? "Application" : path;
        }

        public string Path { get; private set; }

        public override string ToString()
        {
            return ToString("", null);
        }

        public string ToString(string format)
        {
            return ToString(format, null);
        }

        public string ToString(string format, IFormatProvider provider)
        {
            if (format == "N") return "";
            return Path;
        }
    }
}