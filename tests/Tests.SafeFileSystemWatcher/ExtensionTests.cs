using System.IO;
using SafeFileSystemWatcher.Internals;
using Xunit;

namespace Tests.SafeFileSystemWatcher
{
    public class ExtensionTests
    {
        [Fact]
        public void GivenEqualEventArgsIsDuplicateShouldBeTrue()
        {
            var event1 = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetTempPath(), "Temp");
            var event2 = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetTempPath(), "Temp");

            Assert.True(event1.IsDuplicate(event2));
        }

        [Fact]
        public void GivenUnequalEventArgsIsDuplicateShouldBeFalse()
        {
            var event1 = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetTempPath(), "Temp");
            var event2 = new FileSystemEventArgs(WatcherChangeTypes.Created, Path.GetTempPath(), "Temp123");

            Assert.False(event1.IsDuplicate(event2));
        }
    }
}