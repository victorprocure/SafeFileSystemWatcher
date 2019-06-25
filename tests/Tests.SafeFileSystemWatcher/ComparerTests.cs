using System.IO;
using SafeFileSystemWatcher.Collections;
using Xunit;

namespace Tests.SafeFileSystemWatcher
{
    public class ComparerTests
    {
        private readonly FileSystemEventArgsComparer _fileSystemEventArgsComparer;

        public ComparerTests() => _fileSystemEventArgsComparer = new FileSystemEventArgsComparer();

        [Fact]
        public void GivenEventArgsHasMatchingValuesComparerShouldReturnTrue()
        {
            var event1 = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetTempPath(), "Temp");
            var event2 = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetTempPath(), "Temp");

            Assert.True(_fileSystemEventArgsComparer.Equals(event1, event2));
        }

        [Fact]
        public void GivenEqualEventsHashCodeShouldBeEqual()
        {
            var event1 = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetTempPath(), "Temp");
            var event2 = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetTempPath(), "Temp");

            var event1HashCode = _fileSystemEventArgsComparer.GetHashCode(event1);
            var event2HashCode = _fileSystemEventArgsComparer.GetHashCode(event2);

            Assert.Equal(event1HashCode, event2HashCode);
        }

        [Fact]
        public void GivenEqualRenamedEventArgsShouldReturnTrue()
        {
            var renameEvent1 = new RenamedEventArgs(WatcherChangeTypes.Changed, Path.GetTempPath(), "Temp", "Temp1");
            var renameEvent2 = new RenamedEventArgs(WatcherChangeTypes.Changed, Path.GetTempPath(), "Temp", "Temp1");

            Assert.True(_fileSystemEventArgsComparer.Equals(renameEvent1, renameEvent2));
        }

        [Fact]
        public void GivenEqualRenameEventsHashCodesShouldBeEqual()
        {
            var renameEvent1 = new RenamedEventArgs(WatcherChangeTypes.Changed, Path.GetTempPath(), "Temp", "Temp1");
            var renameEvent2 = new RenamedEventArgs(WatcherChangeTypes.Changed, Path.GetTempPath(), "Temp", "Temp1");

            var event1HashCode = _fileSystemEventArgsComparer.GetHashCode(renameEvent1);
            var event2HashCode = _fileSystemEventArgsComparer.GetHashCode(renameEvent2);

            Assert.Equal(event1HashCode, event2HashCode);
        }

        [Fact]
        public void GivenEventArgsNotMatchingValuesComparerShouldReturnFalse()
        {
            var event1 = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetTempPath(), "Temp");
            var event2 = new FileSystemEventArgs(WatcherChangeTypes.Changed, Path.GetTempPath(), "Temp");

            Assert.False(_fileSystemEventArgsComparer.Equals(event1, event2));
        }

        [Fact]
        public void GivenUnequalEventsHashCodeShouldNotBeEqual()
        {
            var event1 = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetTempPath(), "Temp");
            var event2 = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetTempPath(), "Temp2");

            var event1HashCode = _fileSystemEventArgsComparer.GetHashCode(event1);
            var event2HashCode = _fileSystemEventArgsComparer.GetHashCode(event2);

            Assert.NotEqual(event1HashCode, event2HashCode);
        }

        [Fact]
        public void GivenUnequalRenamedEventArgsShouldReturnFalse()
        {
            var renameEvent1 = new RenamedEventArgs(WatcherChangeTypes.Changed, Path.GetTempPath(), "Temp", "Tem32");
            var renameEvent2 = new RenamedEventArgs(WatcherChangeTypes.Changed, Path.GetTempPath(), "Temp", "Temp1");

            Assert.False(_fileSystemEventArgsComparer.Equals(renameEvent1, renameEvent2));
        }

        [Fact]
        public void GivenUnequalRenameEventsHashCodesShouldNotBeEqual()
        {
            var renameEvent1 = new RenamedEventArgs(WatcherChangeTypes.Changed, Path.GetTempPath(), "Temp", "Temp123");
            var renameEvent2 = new RenamedEventArgs(WatcherChangeTypes.Changed, Path.GetTempPath(), "Temp", "Temp1");

            var event1HashCode = _fileSystemEventArgsComparer.GetHashCode(renameEvent1);
            var event2HashCode = _fileSystemEventArgsComparer.GetHashCode(renameEvent2);

            Assert.NotEqual(event1HashCode, event2HashCode);
        }
    }
}