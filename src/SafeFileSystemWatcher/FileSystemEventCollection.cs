using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using SafeFileSystemWatcher.Configurations;
using SafeFileSystemWatcher.Internals;

namespace SafeFileSystemWatcher
{
    /// <inheritdoc />
    /// <summary>
    /// Collection of any file system events currently happening in a given directory,
    /// Should be used on a background task as this will block while waiting for change events
    /// </summary>
    public sealed class FileSystemEventCollection : IEnumerable<FileSystemEventArgs>
    {
        private readonly CancellationToken _cancellationToken;
        private readonly FileSystemEventCollectionConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of <see cref="FileSystemEventCollection"/>
        /// </summary>
        /// <param name="configurationBuilder">Builder to use for configuration</param>
        /// <param name="configuration">Configuration to use</param>
        /// <param name="cancellationToken">Cancellation token to signal to watcher to stop</param>
        public FileSystemEventCollection(IFileSystemEventCollectionConfigurationBuilder configurationBuilder,
            FileSystemEventCollectionConfiguration configuration, CancellationToken cancellationToken)
        {
            if (cancellationToken == default)
                throw new ArgumentNullException(nameof(cancellationToken));
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            _configuration = configurationBuilder.Build(configuration);
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemEventCollection"/>
        /// </summary>
        /// <param name="configuration">Configuration values for collection</param>
        /// <param name="cancellationToken">Cancellation token to signal to watcher to stop</param>
        public FileSystemEventCollection(FileSystemEventCollectionConfiguration configuration, CancellationToken cancellationToken)
            : this(new DefaultFileSystemEventCollectionConfigurationBuilder(), configuration, cancellationToken)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemEventCollection" />
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to signal to watcher to stop</param>
        /// <param name="directory">Directory to monitor for events</param>
        /// <param name="filePattern">File pattern to monitor within directory</param>
        public FileSystemEventCollection(CancellationToken cancellationToken, string directory, string filePattern = null)
            : this(new FileSystemEventCollectionConfiguration(directory, filePattern), cancellationToken)

        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Iterates over the collection of <see cref="FileSystemEventArgs" /> awaiting any new ones.
        /// This is long running and will block while waiting for the next file system event
        /// </summary>
        /// <remarks>
        /// On initial creation of collection will create an event for all files currently in monitored directory.
        /// </remarks>
        /// <returns>Non duplicate <see cref="FileSystemEventArgs"/></returns>
        public IEnumerator<FileSystemEventArgs> GetEnumerator()
        {
            if (!_cancellationToken.IsCancellationRequested)
            {
                using (var watcher = new FileSystemWatcher(_configuration.DirectoryToMonitor, _configuration.DirectoryFileFilter))
                using (var queue = new FileSystemEventQueue(_configuration.DuplicateEventDelayWindow.TotalMilliseconds))
                {
                    QueueInitialFiles(queue);
                    InitializeWatcher(queue, watcher);

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
            foreach (var file in Directory.GetFiles(_configuration.DirectoryToMonitor, _configuration.DirectoryFileFilter, SearchOption.TopDirectoryOnly))
            {
                queue.Enqueue(new FileSystemEventArgs(WatcherChangeTypes.All, _configuration.DirectoryToMonitor, Path.GetFileName(file)));
            }
        }
    }
}