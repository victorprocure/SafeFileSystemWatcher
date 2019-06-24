using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace SafeFileSystemWatcher.Internals
{
    internal sealed class FileSystemEventQueue : IDisposable
    {
        private readonly ConcurrentDictionary<FileSystemEventArgs, Timer> _dedupeQueue;
        private readonly ConcurrentQueue<FileSystemEventArgs> _queue;
        private readonly SemaphoreSlim _enqueueSemaphore;
        private readonly double _duplicateDelayWindow;

        internal FileSystemEventQueue(double duplicateDelayWindow)
        {
            _dedupeQueue = new ConcurrentDictionary<FileSystemEventArgs, Timer>();
            _queue = new ConcurrentQueue<FileSystemEventArgs>();
            _enqueueSemaphore = new SemaphoreSlim(0);
            _duplicateDelayWindow = duplicateDelayWindow;
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

        public bool TryDequeue(out FileSystemEventArgs fileEventArgs, CancellationToken cancellationToken)
        {
            fileEventArgs = null;
            if (cancellationToken.IsCancellationRequested)
            {
                Trace.WriteLine("Cancellation has been requested");
                return false;
            }

            try
            {
                _enqueueSemaphore.Wait(cancellationToken);
                return _queue.TryDequeue(out fileEventArgs);
            }
            catch (OperationCanceledException)
            {
                Trace.WriteLine("Cancellation has been requested");
                return false;
            }
        }

        public void Enqueue(FileSystemEventArgs fileSystemEventArgs)
        {
            if (!_dedupeQueue.TryGetValue(GetOriginatingKey(fileSystemEventArgs), out var timer))
            {
#pragma warning disable DF0022 // Marks undisposed objects assinged to a property, originated in an object creation.
                timer = new Timer { Interval = _duplicateDelayWindow, AutoReset = false };
#pragma warning restore DF0022 // Marks undisposed objects assinged to a property, originated in an object creation.

                timer.Elapsed += OnTimerElapsed;

                _dedupeQueue.TryAdd(fileSystemEventArgs, timer);
            }

            timer.Stop();
            timer.Start();
        }

        private FileSystemEventArgs GetOriginatingKey(FileSystemEventArgs fileSystemEventArgs)
            => _dedupeQueue.FirstOrDefault(d => d.Key.IsDuplicate(fileSystemEventArgs)).Key ?? fileSystemEventArgs;

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            var timer = sender as Timer;

            var kvp = _dedupeQueue.First(t => t.Value == timer);
            _queue.Enqueue(kvp.Key);
            Trace.WriteLine($"File name added to queue: {kvp.Key}");

            if (_dedupeQueue.TryRemove(kvp.Key, out var removedInfo))
                removedInfo?.Dispose();

            _enqueueSemaphore.Release();
        }
    }
}