using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SafeFileSystemWatcher.Configurations;
using SafeFileSystemWatcher.Internals;

namespace SafeFileSystemWatcher
{
    /// <summary>
    /// Background threadsafe watcher for <see cref="FileSystemEventArgs"/>
    /// </summary>
    public sealed class Watcher : IDisposable
    {
        private readonly CancellationToken _cancellationToken;
        private readonly FileSystemEventConfiguration _configuration;
        private readonly ManualResetEventSlim _intializedEvent = new ManualResetEventSlim();
        private readonly ILogger<Watcher> _logger;
        private readonly object _syncRoot = new object();
        private Action<FileSystemEventArgs> _callback;
        private FileSystemEventCollection _collection;
        private Thread _internalThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="Watcher"/> class.
        /// </summary>
        /// <param name="callback">Callback to execute on new file system event</param>
        /// <param name="configuration">Initial configuration object</param>
        /// <param name="cancellationToken">Cancellation token to signal to stop watching</param>
        /// <param name="logger">Logger to use</param>
        public Watcher(Action<FileSystemEventArgs> callback, FileSystemEventConfiguration configuration,
            CancellationToken cancellationToken, ILogger<Watcher> logger = null)
            : this(callback, new DefaultFileSystemEventConfigurationBuilder(), configuration, cancellationToken, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Watcher"/> class.
        /// </summary>
        /// <param name="configuration">Initial configuration object</param>
        /// <param name="cancellationToken">Cancellation token to signal to stop watching</param>
        /// <param name="logger">Logger to use</param>
        public Watcher(FileSystemEventConfiguration configuration, CancellationToken cancellationToken, ILogger<Watcher> logger = null)
            : this(null, new DefaultFileSystemEventConfigurationBuilder(), configuration, cancellationToken, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Watcher"/> class.
        /// </summary>
        /// <param name="configurationBuilder">Configuration builder to use</param>
        /// <param name="configuration">Initial configuration object</param>
        /// <param name="cancellationToken">Cancellation token to signal to stop watching</param>
        /// <param name="logger">Logger to use</param>
        public Watcher(IFileSystemEventConfigurationBuilder configurationBuilder,
            FileSystemEventConfiguration configuration, CancellationToken cancellationToken, ILogger<Watcher> logger = null)
            : this(null, configurationBuilder, configuration, cancellationToken, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Watcher"/> class.
        /// </summary>
        /// <param name="callback">Callback to execute on new file system event</param>
        /// <param name="configurationBuilder">Configuration builder to use</param>
        /// <param name="configuration">Initial configuration object</param>
        /// <param name="cancellationToken">Cancellation token to signal to stop watching</param>
        /// <param name="logger">Logger to use</param>
        public Watcher(Action<FileSystemEventArgs> callback, IFileSystemEventConfigurationBuilder configurationBuilder,
            FileSystemEventConfiguration configuration, CancellationToken cancellationToken, ILogger<Watcher> logger = null)
        {
            if (cancellationToken == default)
                throw new ArgumentNullException(nameof(cancellationToken));

            _callback = callback;
            _cancellationToken = cancellationToken;
            _configuration = configurationBuilder.Build(configuration);

            if (logger is null)
                logger = NullLogger<Watcher>.Instance;
            _logger = logger;

            Initialize();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _intializedEvent?.Dispose();
            _collection?.Dispose();

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Begin monitoring directory for file changes
        /// </summary>
        /// <param name="callback">Callback to execute when file system event occurs</param>
        /// <exception cref="InvalidOperationException">Thrown if no callbacks available to execute</exception>
        public void Watch(Action<FileSystemEventArgs> callback = null)
        {
            if (_callback is null && callback is null)
                throw new InvalidOperationException("Unable to watch with no callback to execute");

            if (!(callback is null))
            {
                _logger.CallbackOverride();
                lock (_syncRoot)
                {
                    _callback = callback;
                }
            }

            _internalThread.Start();
            _intializedEvent.Wait();
        }

        private void Initialize()
        {
            _logger.Initializing<Watcher>(_callback is null ? "with no default callback" : "with default callback");
            lock (_syncRoot)
            {
                _internalThread = new Thread(StartCollectionWatcher);
            }
        }

        private void StartCollectionWatcher()
        {
            _collection = new FileSystemEventCollection(_configuration, _cancellationToken);

            Task.Run(() =>
            {
                _collection.isInitializedEvent.Wait();
                _intializedEvent.Set();
            });

            var collectionEnumerator = _collection.GetEnumerator();
            while (collectionEnumerator.MoveNext())
            {
                _callback(collectionEnumerator.Current);
            }
        }
    }
}