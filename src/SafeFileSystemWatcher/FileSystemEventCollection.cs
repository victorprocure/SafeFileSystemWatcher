using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SafeFileSystemWatcher.Configurations;
using SafeFileSystemWatcher.Internals;

namespace SafeFileSystemWatcher
{
    /// <inheritdoc />
    /// <summary>
    /// Collection of any file system events currently happening in a given directory,
    /// Should be used on a background task as this will block while waiting for change events
    /// </summary>
    public sealed class FileSystemEventCollection : IEnumerable<FileSystemEventArgs>, IDisposable
    {
        internal readonly ManualResetEventSlim isInitializedEvent = new ManualResetEventSlim();
        private readonly CancellationToken _cancellationToken;
        private readonly FileSystemEventConfiguration _configuration;
        private readonly ILogger<FileSystemEventCollection> _logger;

        /// <summary>
        /// Initializes a new instance of <see cref="FileSystemEventCollection"/>
        /// </summary>
        /// <param name="configurationBuilder">Builder to use for configuration</param>
        /// <param name="configuration">Configuration to use</param>
        /// <param name="cancellationToken">Cancellation token to signal to watcher to stop</param>
        /// <param name="logger">Logger to use</param>
        public FileSystemEventCollection(IFileSystemEventConfigurationBuilder configurationBuilder,
            FileSystemEventConfiguration configuration, CancellationToken cancellationToken, ILogger<FileSystemEventCollection> logger = null)
        {
            if (cancellationToken == default)
                throw new ArgumentNullException(nameof(cancellationToken));
            if (configuration is null)
                throw new ArgumentNullException(nameof(configuration));

            _configuration = configurationBuilder.Build(configuration);
            _cancellationToken = cancellationToken;

            if (logger is null)
                logger = NullLogger<FileSystemEventCollection>.Instance;

            _logger = logger;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemEventCollection"/>
        /// </summary>
        /// <param name="configuration">Configuration values for collection</param>
        /// <param name="cancellationToken">Cancellation token to signal to watcher to stop</param>
        /// <param name="logger">Logger to use</param>
        public FileSystemEventCollection(FileSystemEventConfiguration configuration, CancellationToken cancellationToken,
            ILogger<FileSystemEventCollection> logger = null)
            : this(new DefaultFileSystemEventConfigurationBuilder(), configuration, cancellationToken, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemEventCollection" />
        /// </summary>
        /// <param name="cancellationToken">Cancellation token to signal to watcher to stop</param>
        /// <param name="directory">Directory to monitor for events</param>
        /// <param name="filePattern">File pattern to monitor within directory</param>
        /// <param name="logger">Logger to use</param>
        public FileSystemEventCollection(CancellationToken cancellationToken, string directory, string filePattern = null,
            ILogger<FileSystemEventCollection> logger = null)
            : this(new FileSystemEventConfiguration(directory, filePattern), cancellationToken, logger)

        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
            isInitializedEvent?.Dispose();
            GC.SuppressFinalize(this);
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
                using (var queue = new FileSystemEventQueue(_configuration.DuplicateEventDelayWindow.TotalMilliseconds, _logger))
                {
                    Initialize(queue, watcher);

                    if (!_cancellationToken.IsCancellationRequested)
                    {
                        while (queue.TryDequeue(out var fileSystemEventArgs, _cancellationToken))
                            yield return fileSystemEventArgs;
                    }
                }
            }

            _logger.CancellationRequested();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void Initialize(FileSystemEventQueue queue, FileSystemWatcher watcher)
        {
            var initializationTasks = new[]
            {
                Task.Run(() => InitializeWatcher(queue, watcher)),
                Task.Run(() => QueueInitialFiles(queue))
            };

            Task.WaitAll(initializationTasks);
            isInitializedEvent.Set();
        }

        private void InitializeWatcher(FileSystemEventQueue queue, FileSystemWatcher watcher)
        {
            _logger.Initializing<FileSystemEventCollection>();
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
            _logger.QueuingInitialFiles();
            foreach (var file in Directory.GetFiles(_configuration.DirectoryToMonitor, _configuration.DirectoryFileFilter, SearchOption.TopDirectoryOnly))
            {
                queue.Enqueue(new FileSystemEventArgs(WatcherChangeTypes.All, _configuration.DirectoryToMonitor, Path.GetFileName(file)));
            }
        }
    }
}