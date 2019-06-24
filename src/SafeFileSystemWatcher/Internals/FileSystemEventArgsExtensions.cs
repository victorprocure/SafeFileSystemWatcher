using System.IO;

namespace SafeFileSystemWatcher.Internals
{
    internal static class FileSystemEventArgsExtensions
    {
        private static readonly FileSystemEventArgsComparer _comparer = new FileSystemEventArgsComparer();

        public static bool IsDuplicate(this FileSystemEventArgs event1, FileSystemEventArgs event2)
            => _comparer.Equals(event1, event2);
    }
}