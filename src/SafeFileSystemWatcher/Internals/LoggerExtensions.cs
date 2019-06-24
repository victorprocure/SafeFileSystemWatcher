using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace SafeFileSystemWatcher.Internals
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> _callbackOverride = LoggerMessage.Define(
            logLevel: LogLevel.Debug,
            eventId: new EventId(2, nameof(CallbackOverride)),
            formatString: "Callback is being overridden");

        private static readonly Action<ILogger, Exception> _cancellationRequested = LoggerMessage.Define(
            logLevel: LogLevel.Debug,
            eventId: new EventId(3, nameof(CancellationRequested)),
            formatString: "Cancellation requested");

        private static readonly Action<ILogger, FileSystemEventArgs, Exception> _duplicateTimerRestart = LoggerMessage.Define<FileSystemEventArgs>(
            logLevel: LogLevel.Trace,
            eventId: new EventId(6, nameof(DuplicateTimerRestart)),
            formatString: "Timer reset for: @{fileEventArgs}");

        private static readonly Action<ILogger, FileSystemEventArgs, Exception> _enqueue = LoggerMessage.Define<FileSystemEventArgs>(
                    logLevel: LogLevel.Debug,
            eventId: new EventId(5, nameof(Enqueue)),
            formatString: "File queued: @{fileEventArgs}");

        private static readonly Action<ILogger, string, string, Exception> _initializing = LoggerMessage.Define<string, string>(
                                    logLevel: LogLevel.Debug,
            eventId: new EventId(1, nameof(Initializing)),
            formatString: "Initializing {type} {extraInformation}");

        private static readonly Action<ILogger, FileSystemEventArgs, Exception> _originalTimerAdded = LoggerMessage.Define<FileSystemEventArgs>(
            logLevel: LogLevel.Trace,
            eventId: new EventId(6, nameof(OriginalTimerAdded)),
            formatString: "Added timer for new item: @{fileEventArgs}");

        private static readonly Action<ILogger, Exception> _queuingInitialFiles = LoggerMessage.Define(
                    logLevel: LogLevel.Debug,
            eventId: new EventId(4, nameof(QueuingInitialFiles)),
            formatString: "Queuing initial files");

        public static void CallbackOverride(this ILogger logger)
            => _callbackOverride(logger, null);

        public static void CancellationRequested(this ILogger logger)
            => _cancellationRequested(logger, null);

        public static void DuplicateTimerRestart(this ILogger logger, FileSystemEventArgs fileSystemEventArgs)
                            => _duplicateTimerRestart(logger, fileSystemEventArgs, null);

        public static void Enqueue(this ILogger logger, FileSystemEventArgs fileSystemEventArgs)
                            => _enqueue(logger, fileSystemEventArgs, null);

        public static void Initializing<T>(this ILogger logger, string extraInformation = null)
            => _initializing(logger, typeof(T).Name, extraInformation, null);

        public static void OriginalTimerAdded(this ILogger logger, FileSystemEventArgs fileSystemEventArgs)
                            => _originalTimerAdded(logger, fileSystemEventArgs, null);

        public static void QueuingInitialFiles(this ILogger logger)
            => _queuingInitialFiles(logger, null);
    }
}