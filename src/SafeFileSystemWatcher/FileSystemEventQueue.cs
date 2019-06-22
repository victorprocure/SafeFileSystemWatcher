using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SafeFileSystemWatcher
{
    internal sealed class FileSystemEventQueue : IDisposable
    {
        private readonly ConcurrentDictionary<string, (FileSystemEventArgs eventArgs, Timer timer)> _dedupeQueue;
        private readonly ConcurrentQueue<FileSystemEventArgs> _queue;
        private readonly SemaphoreSlim _enqueueSemaphore;

        internal FileSystemEventQueue()
        {
            _dedupeQueue = new ConcurrentDictionary<string, (FileSystemEventArgs eventArgs, Timer timer)>();
            _queue = new ConcurrentQueue<FileSystemEventArgs>();
            _enqueueSemaphore = new SemaphoreSlim(0);
        }

        public void Dispose()
        {
            _enqueueSemaphore?.Dispose();
            if (_dedupeQueue.Count > 0)
            {
                foreach (var info in _dedupeQueue)
                {
                    info.Value.timer?.Dispose();
                }
            }

            GC.SuppressFinalize(this);
        }

        public bool TryDequeue(out FileSystemEventArgs fileName, CancellationToken cancellationToken)
        {
            fileName = null;
            if (cancellationToken.IsCancellationRequested)
            {
                Trace.WriteLine("Cancellation has been requested");
                return false;
            }

            try
            {
                _enqueueSemaphore.Wait(cancellationToken);
                return _queue.TryDequeue(out fileName);
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("Cancellation has been requested");
                return false;
            }
        }

        public void Enqueue(FileSystemEventArgs fileSystemEventArgs)
        {
            if (!_dedupeQueue.TryGetValue(fileSystemEventArgs.FullPath, out var eventInfo))
            {
                eventInfo.eventArgs = fileSystemEventArgs;

#pragma warning disable DF0022 // Marks undisposed objects assinged to a property, originated in an object creation.
                eventInfo.timer = new Timer { Interval = 200, AutoReset = false };
#pragma warning restore DF0022 // Marks undisposed objects assinged to a property, originated in an object creation.

                eventInfo.timer.Elapsed += OnTimerElapsed;

                _ = _dedupeQueue.TryAdd(fileSystemEventArgs.FullPath, eventInfo);
            }

            eventInfo.timer.Stop();
            eventInfo.timer.Start();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var timer = sender as Timer;

            var file = _dedupeQueue.First(t => t.Value.timer == timer);
            _queue.Enqueue(file.Value.eventArgs);
            Debug.WriteLine($"File name added to queue: {file.Key}");

            if (_dedupeQueue.TryRemove(file.Key, out var removedInfo))
                removedInfo.timer?.Dispose();

            _ = _enqueueSemaphore.Release();
        }
    }
}