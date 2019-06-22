using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SafeFileSystemWatcher
{
    public sealed class FileSystemEventCollection : IEnumerable<FileSystemEventArgs>
    {
        private readonly string _directory;
        private readonly string _filePattern;
        private readonly CancellationToken _cancellationToken;

        public FileSystemEventCollection(CancellationToken cancellationToken, string directory, string filePattern = null)
        {
            if (cancellationToken == default)
                throw new ArgumentNullException(nameof(cancellationToken));
            if (string.IsNullOrEmpty(directory))
                throw new ArgumentNullException(nameof(directory));
            if (!Directory.Exists(directory))
                throw new ArgumentException($"Directory: {directory} does not exist", nameof(directory));

            _directory = directory;
            _filePattern = filePattern ?? "*";
            _cancellationToken = cancellationToken;
        }

        public IEnumerator<FileSystemEventArgs> GetEnumerator()
        {
            if (!_cancellationToken.IsCancellationRequested)
            {
                using (var watcher = new FileSystemWatcher(_directory, _filePattern))
                using (var queue = new FileSystemEventQueue())
                {
                    InitializeWatcher(queue, watcher);
                    QueueInitialFiles(queue);

                    if (!_cancellationToken.IsCancellationRequested)
                    {
                        while (queue.TryDequeue(out var fileSystemEventArgs, _cancellationToken))
                            yield return fileSystemEventArgs;
                    }
                }
            }

            Trace.WriteLine("Cancellation has been requested");
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static void InitializeWatcher(FileSystemEventQueue queue, FileSystemWatcher watcher)
        {
            watcher.NotifyFilter = NotifyFilters.FileName
                        | NotifyFilters.CreationTime
                        | NotifyFilters.LastWrite;

            watcher.Created += (s, e) => queue.Enqueue(e);
            watcher.Changed += (s, e) => queue.Enqueue(e);
            watcher.Deleted += (s, e) => queue.Enqueue(e);
            watcher.Renamed += (s, e) => queue.Enqueue(e);

            watcher.EnableRaisingEvents = true;
        }

        private void QueueInitialFiles(FileSystemEventQueue queue)
        {
            foreach (var file in Directory.GetFiles(_directory, _filePattern, SearchOption.TopDirectoryOnly))
            {
                queue.Enqueue(new FileSystemEventArgs(WatcherChangeTypes.All, _directory, file));
            }
        }
    }
}