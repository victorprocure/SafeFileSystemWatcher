using System;

namespace SafeFileSystemWatcher.Configurations
{
    /// <summary>
    /// Builds a complete configuration for <see cref="FileSystemEventCollection"/>
    /// </summary>
    internal class DefaultFileSystemEventConfigurationBuilder : IFileSystemEventConfigurationBuilder
    {
        private readonly IFileSystemEventConfigurationValidator _validator;

        /// <summary>
        /// Initializes a new <see cref="IFileSystemEventConfigurationBuilder"/>
        /// </summary>
        public DefaultFileSystemEventConfigurationBuilder()
            : this(new DefaultFileSystemEventConfigurationValidator())
        {
        }

        /// <summary>
        /// Initializes a new <see cref="IFileSystemEventConfigurationBuilder"/>
        /// </summary>
        /// <param name="validator">Validator to use when building configuration</param>
        public DefaultFileSystemEventConfigurationBuilder(IFileSystemEventConfigurationValidator validator)
            => _validator = validator;

        /// <summary>
        /// Build configuration for <see cref="FileSystemEventCollection" />
        /// </summary>
        /// <param name="configuration">Configuration to build against</param>
        /// <returns>Completed configuration</returns>
        public FileSystemEventConfiguration Build(FileSystemEventConfiguration configuration)
        {
            var newConfig = new FileSystemEventConfiguration();
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