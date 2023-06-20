using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System.Collections.Concurrent;
using System.IO;

namespace Serilog.Sinks.MemoryQueue
{
    /// <summary>
    /// 内存队列
    /// </summary>
    public class MemoryQueueSink : ILogEventSink
    {
        private const int capacity = 5000;
        private readonly ITextFormatter _formatter;

        public MemoryQueueSink(ITextFormatter formatter)
        {
            _formatter = formatter ?? throw new ArgumentNullException(nameof(formatter));
        }

        public static readonly ConcurrentQueue<string> Logs = new();

        public void Emit(LogEvent logEvent)
        {
            if (Logs.Count >= capacity)
            {
                Logs.TryDequeue(out _);
            }
            using var buffer = new StringWriter();
            _formatter.Format(logEvent, buffer);
            var msg = buffer.ToString().Trim();
            Logs.Enqueue(msg);
        }
    }
}