using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SafeFileSystemWatcher;
using Xunit;

namespace Tests.SafeFileSystemWatcher
{
    public class CollectionTests
    {
        private static readonly string _tempFileExtension = Guid.NewGuid().ToString("N");

        [Fact]
        public void GivenFileCreatedShouldEnumeratorEvent()
        {
            var changeList = new List<string>();
            using (var cts = new CancellationTokenSource())
            {
                var watcher = new FileSystemEventCollection(cts.Token, Path.GetTempPath(), $"*.{_tempFileExtension}").GetEnumerator();
                Task.Run(() =>
                {
                    while (watcher.MoveNext())
                    {
                        changeList.Add(watcher.Current.FullPath);
                    }
                });

                var tempFile = CreateTempFile();
                Task.Delay(2000).GetAwaiter().GetResult();
                TryDeleteFile(tempFile);
                cts.Cancel();
                Assert.Contains(changeList, c => c == tempFile);
            }
        }

        [Fact]
        public void GivenFilePresentInDirectoryShouldEnumerateEvent()
        {
            var changeList = new List<string>();
            var tempFile = CreateTempFile();

            using (var cts = new CancellationTokenSource())
            {
                var watcher = new FileSystemEventCollection(cts.Token, Path.GetTempPath(), $"*.{_tempFileExtension}").GetEnumerator();
                Task.Run(() =>
                {
                    while (watcher.MoveNext())
                    {
                        changeList.Add(watcher.Current.FullPath);
                    }
                });

                Task.Delay(2000).GetAwaiter().GetResult();
                TryDeleteFile(tempFile);
                cts.Cancel();
                Assert.Contains(changeList, c => c == tempFile);
            }
        }

        private static void TryDeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // Don't throw
            }
        }

        private static string CreateTempFile()
        {
            var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.{_tempFileExtension}");
            using (File.Create(tempFilePath))
            {
                return tempFilePath;
            }
        }
    }
}