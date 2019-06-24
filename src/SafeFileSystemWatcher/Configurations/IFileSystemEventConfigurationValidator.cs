namespace SafeFileSystemWatcher.Configurations
{
    /// <summary>
    /// Validation service for <see cref="FileSystemEventConfiguration"/>
    /// </summary>
    public interface IFileSystemEventConfigurationValidator
    {
        /// <summary>
        /// Try and validate the given configuration
        /// </summary>
        /// <param name="configuration">Configuration to validate</param>
        /// <returns>Returns <c>true</c> if valid, <c>false</c> if not</returns>
        bool TryValidate(FileSystemEventConfiguration configuration);

        /// <summary>
        /// Validate the given configuration
        /// </summary>
        /// <param name="configuration">Configuration to validate</param>
        /// <exception cref="System.InvalidOperationException">Thrown if the configuration is not valid</exception>
        void Validate(FileSystemEventConfiguration configuration);
    }
}