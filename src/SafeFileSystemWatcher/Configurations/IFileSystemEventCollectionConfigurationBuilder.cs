namespace SafeFileSystemWatcher.Configurations
{
    /// <summary>
    /// Builds a complete configuration for <see cref="FileSystemEventCollection"/>
    /// </summary>
    public interface IFileSystemEventCollectionConfigurationBuilder
    {
        /// <summary>
        /// Build configuration for <see cref="FileSystemEventCollection"/>
        /// </summary>
        /// <param name="configuration">Configuration to build against</param>
        /// <returns>Completed configuration</returns>
        FileSystemEventCollectionConfiguration Build(FileSystemEventCollectionConfiguration configuration);
    }
}