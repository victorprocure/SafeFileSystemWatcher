using System;
using System.IO;
using System.Threading;
using SafeFileSystemWatcher.Configurations;

namespace SafeFileSystemWatcher
{
    /// <summary>
    /// Background threadsafe watcher for <see cref="FileSystemEventArgs"/>
    /// </summary>
    public class Watcher
    {
        private readonly Action<FileSystemEventArgs> _callback;
        private readonly CancellationToken _cancellationToken;
        private readonly FileSystemEventCollectionConfiguration _configuration;
        private Thread _internalThread;

        /// <summary>
        /// Initializes a new instance of the <see cref="Watcher"/> class.
        /// </summary>
        /// <param name="callback">Callback to execute on new file system event</param>
        /// <param name="configuration">Initial configuration object</param>
        /// <param name="cancellationToken">Cancellation token to signal to stop watching</param>
        public Watcher(Action<FileSystemEventArgs> callback, FileSystemEventCollectionConfiguration configuration,
            CancellationToken cancellationToken)
            : this(callback, new DefaultFileSystemEventCollectionConfigurationBuilder(), configuration, cancellationToken)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Watcher"/> class.
        /// </summary>
        /// <param name="callback">Callback to execute on new file system event</param>
        /// <param name="configurationBuilder">Configuration builder to use</param>
        /// <param name="configuration">Initial configuration object</param>
        /// <param name="cancellationToken">Cancellation token to signal to stop watching</param>
        public Watcher(Action<FileSystemEventArgs> callback, IFileSystemEventCollectionConfigurationBuilder configurationBuilder,
            FileSystemEventCollectionConfiguration configuration, CancellationToken cancellationToken)
        {
            _callback = callback;
            _cancellationToken = cancellationToken;
            _configuration = configurationBuilder.Build(configuration);

            Initialize();
        }

        /// <summary>
        /// Begin monitoring directory for file changes
        /// </summary>
        public void Watch()
            => _internalThread.Start();

        private void Initialize()
            => _internalThread = new Thread(StartCollectionWatcher);

        private void StartCollectionWatcher()
        {
            var collection = new FileSystemEventCollection(_configuration, _cancellationToken);
            var collectionEnumerator = collection.GetEnumerator();
            while (collectionEnumerator.MoveNext())
            {
                _callback(collectionEnumerator.Current);
            }
        }
    }
}