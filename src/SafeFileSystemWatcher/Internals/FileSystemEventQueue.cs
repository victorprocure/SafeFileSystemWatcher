using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using Microsoft.Extensions.Logging;
using Timer = System.Timers.Timer;

namespace SafeFileSystemWatcher.Internals
{
    internal sealed class FileSystemEventQueue : IDisposable
    {
        private readonly ConcurrentDictionary<FileSystemEventArgs, Timer> _dedupeQueue;
        private readonly double _duplicateDelayWindow;
        private readonly SemaphoreSlim _enqueueSemaphore;
        private readonly ILogger _logger;
        private readonly ConcurrentQueue<FileSystemEventArgs> _queue;

        internal FileSystemEventQueue(double duplicateDelayWindow, ILogger logger)
        {
            _dedupeQueue = new ConcurrentDictionary<FileSystemEventArgs, Timer>();
            _queue = new ConcurrentQueue<FileSystemEventArgs>();
            _enqueueSemaphore = new SemaphoreSlim(0);
            _duplicateDelayWindow = duplicateDelayWindow;
            _logger = logger;
        }

        public void Dispose()
        {
            _enqueueSemaphore?.Dispose();
            if (_dedupeQueue.Count > 0)
            {
                foreach (var info in _dedupeQueue)
                {
                    info.Value?.Dispose();
                }
            }

            GC.SuppressFinalize(this);
        }

        public void Enqueue(FileSystemEventArgs fileSystemEventArgs)
        {
            if (!_dedupeQueue.TryGetValue(GetOriginatingKey(fileSystemEventArgs), out var timer))
            {
#pragma warning disable DF0022 // Marks undisposed objects assinged to a property, originated in an object creation.
                timer = new Timer { Interval = _duplicateDelayWindow, AutoReset = false };
#pragma warning restore DF0022 // Marks undisposed objects assinged to a property, originated in an object creation.

                timer.Elapsed += OnTimerElapsed;

                _logger.OriginalTimerAdded(fileSystemEventArgs);
                _dedupeQueue.TryAdd(fileSystemEventArgs, timer);
            }
            else
            {
                _logger.DuplicateTimerRestart(fileSystemEventArgs);
            }

            timer.Stop();
            timer.Start();
        }

        public bool TryDequeue(out FileSystemEventArgs fileEventArgs, CancellationToken cancellationToken)
        {
            fileEventArgs = null;
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.CancellationRequested();
                return false;
            }

            try
            {
                _enqueueSemaphore.Wait(cancellationToken);
                return _queue.TryDequeue(out fileEventArgs);
            }
            catch (OperationCanceledException)
            {
                _logger.CancellationRequested();
                return false;
            }
        }

        private FileSystemEventArgs GetOriginatingKey(FileSystemEventArgs fileSystemEventArgs)
            => _dedupeQueue.FirstOrDefault(d => d.Key.IsDuplicate(fileSystemEventArgs)).Key ?? fileSystemEventArgs;

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var timer = sender as Timer;

            var kvp = _dedupeQueue.First(t => t.Value == timer);
            _queue.Enqueue(kvp.Key);
            _logger.Enqueue(kvp.Key);

            if (_dedupeQueue.TryRemove(kvp.Key, out var removedInfo))
                removedInfo?.Dispose();

            _enqueueSemaphore.Release();
        }
    }
}