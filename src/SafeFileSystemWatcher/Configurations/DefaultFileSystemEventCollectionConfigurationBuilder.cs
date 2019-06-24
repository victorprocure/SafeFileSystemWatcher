using System;

namespace SafeFileSystemWatcher.Configurations
{
    /// <summary>
    /// Builds a complete configuration for <see cref="FileSystemEventCollection"/>
    /// </summary>
    internal class DefaultFileSystemEventCollectionConfigurationBuilder : IFileSystemEventCollectionConfigurationBuilder
    {
        private readonly IFileSystemEventCollectionConfigurationValidator _validator;

        /// <summary>
        /// Initializes a new <see cref="IFileSystemEventCollectionConfigurationBuilder"/>
        /// </summary>
        public DefaultFileSystemEventCollectionConfigurationBuilder()
            : this(new DefaultFileSystemEventCollectionConfigurationValidator())
        {
        }

        /// <summary>
        /// Initializes a new <see cref="IFileSystemEventCollectionConfigurationBuilder"/>
        /// </summary>
        /// <param name="validator">Validator to use when building configuration</param>
        public DefaultFileSystemEventCollectionConfigurationBuilder(IFileSystemEventCollectionConfigurationValidator validator)
            => _validator = validator;

        /// <summary>
        /// Build configuration for <see cref="FileSystemEventCollection" />
        /// </summary>
        /// <param name="configuration">Configuration to build against</param>
        /// <returns>Completed configuration</returns>
        public FileSystemEventCollectionConfiguration Build(FileSystemEventCollectionConfiguration configuration)
        {
            var newConfig = new FileSystemEventCollectionConfiguration();
            if (!string.IsNullOrEmpty(configuration.DirectoryFileFilter))
                newConfig.DirectoryFileFilter = configuration.DirectoryFileFilter;

            newConfig.DirectoryToMonitor = configuration.DirectoryToMonitor;

            if (configuration.DuplicateEventDelayWindow > TimeSpan.Zero)
                newConfig.DuplicateEventDelayWindow = configuration.DuplicateEventDelayWindow;

            _validator.Validate(newConfig);
            return newConfig;
        }
    }
}