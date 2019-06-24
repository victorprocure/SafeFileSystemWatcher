using System;

namespace SafeFileSystemWatcher.Configurations
{
    /// <summary>
    /// Configuration containment object for <see cref="FileSystemEventCollection"/>
    /// </summary>
    public class FileSystemEventCollectionConfiguration
    {
        /// <summary>
        /// Intializes a new <see cref="FileSystemEventCollectionConfiguration"/>
        /// </summary>
        public FileSystemEventCollectionConfiguration()
        {
        }

        /// <summary>
        /// Intializes a new <see cref="FileSystemEventCollectionConfiguration"/>
        /// </summary>
        /// <param name="directory">Directory to monitor</param>
        /// <param name="filePattern">File pattern to monitor within directory</param>
        public FileSystemEventCollectionConfiguration(string directory, string filePattern = null)
        {
            DirectoryToMonitor = directory;
            if (!string.IsNullOrEmpty(filePattern))
                DirectoryFileFilter = filePattern;
        }

        /// <summary>
        /// Gets or sets <see cref="DuplicateEventDelayWindow"/>, this
        /// value represents the time to wait before posting and event from <see cref="System.IO.FileSystemWatcher"/>
        /// in order to verify it is not a duplicate event
        /// </summary>
        public TimeSpan DuplicateEventDelayWindow { get; set; } = TimeSpan.FromMilliseconds(500);

        /// <summary>
        /// Gets or sets <see cref="DirectoryToMonitor"/>, the directory to monitor for file changes
        /// </summary>
        public string DirectoryToMonitor { get; set; }

        /// <summary>
        /// Gets or sets <see cref="DirectoryFileFilter"/>, the filter to use for monitoring file changes,
        /// default value is "*", for all files
        /// </summary>
        public string DirectoryFileFilter { get; set; } = "*";
    }
}