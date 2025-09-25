using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace Broadcast.Classes
{
    public class ContextualLogger<T> : ILogger<T>
    {
        private readonly ContextualLogger _inner;

        public ContextualLogger(ILoggerFactory factory)
        {
            var baseLogger = factory.CreateLogger(typeof(T).FullName!);
            _inner = new ContextualLogger(baseLogger);
        }
        public IDisposable BeginScope<TState>(TState state)
        {
            return _inner.BeginScope(state) ;
        }

        public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _inner.Log(logLevel, eventId, state, exception, formatter);
        }
    }

    public class ContextualLogger : ILogger
    {
        private readonly ILogger _inner;

        public ContextualLogger(ILogger inner)
        {
            _inner = inner;
        }

        public IDisposable BeginScope<TState>(TState state) => _inner.BeginScope(state);
        public bool IsEnabled(LogLevel logLevel) => _inner.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var caller = GetCaller();
            var originalMessage = formatter(state, exception);
            var enrichedMessage = $"[{timestamp}] [{caller}] {originalMessage}";

            _inner.Log(logLevel, eventId, state, exception, (_, _) => enrichedMessage);
        }

        private static string GetCaller()
        {
            var stack = new StackTrace(skipFrames: 2, fNeedFileInfo: false); // Skip Log() and wrapper
            for (int i = 0; i < stack.FrameCount; i++)
            {
                var method = stack.GetFrame(i)?.GetMethod();
                var type = method?.DeclaringType;

                if (type == null) continue;

                // Skip logger-related frames
                if (type == typeof(ContextualLogger) || type.Name.StartsWith("Logger") || type.Name.Contains("ContextualLogger"))
                    continue;

                return $"{type.Name}.{method!.Name}";
            }

            return "UnknownCaller";
        }
    }
}
