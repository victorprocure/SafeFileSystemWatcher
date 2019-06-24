using System;
using System.Diagnostics;
using System.IO;

namespace SafeFileSystemWatcher.Configurations
{
    /// <summary>
    /// Default validator for <see cref="FileSystemEventConfiguration"/>
    /// </summary>
    internal class DefaultFileSystemEventConfigurationValidator : IFileSystemEventConfigurationValidator
    {
        /// <summary>
        /// Try and validate the given configuration
        /// </summary>
        /// <param name="configuration">Configuration to validate</param>
        /// <returns>Returns <c>true</c> if valid, <c>false</c> if not</returns>
        public bool TryValidate(FileSystemEventConfiguration configuration)
        {
            try
            {
                Validate(configuration);
                return true;
            }
            catch (InvalidOperationException ex)
            {
                Debug.WriteLine($"Error validating configuration: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate the given configuration
        /// </summary>
        /// <param name="configuration">Configuration to validate</param>
        /// <exception cref="InvalidOperationException">Thrown if the configuration is not valid</exception>
        public void Validate(FileSystemEventConfiguration configuration)
        {
            if (configuration.DuplicateEventDelayWindow == default)
                throw new InvalidOperationException("Delay window configuration must be set");
            if (string.IsNullOrEmpty(configuration.DirectoryFileFilter))
                throw new InvalidOperationException("File filter to monitor must not be empty");
            if (string.IsNullOrEmpty(configuration.DirectoryToMonitor))
                throw new InvalidOperationException("File directory to monitor must not be empty");

            if (!Directory.Exists(configuration.DirectoryToMonitor))
                throw new InvalidOperationException("Directory to monitor does not exist");

            if (configuration.DuplicateEventDelayWindow <= TimeSpan.Zero)
                throw new InvalidOperationException("Event delay window must be greater than 0");
        }
    }
}