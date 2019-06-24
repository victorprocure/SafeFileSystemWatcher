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
        public async Task GivenFileCreatedShouldEnumeratorEvent()
        {
            var changeList = new List<string>();
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(TimeSpan.FromSeconds(5));
                await RunWatcherAsync(f => changeList.Add(f.FullPath), cts.Token).ConfigureAwait(false);
                var tempFile = CreateTempFile();
                await Task.Delay(2000).ConfigureAwait(false);
                TryDeleteFile(tempFile);
                cts.Cancel();
                Assert.Contains(changeList, c => c == tempFile);
            }
        }

        [Fact]
        public async Task GivenFilePresentInDirectoryShouldEnumerateEvent()
        {
            var changeList = new List<string>();
            var tempFile = CreateTempFile();

            using (var cts = new CancellationTokenSource())
            {
                await RunWatcherAsync(f => changeList.Add(f.FullPath), cts.Token).ConfigureAwait(false);
                await Task.Delay(2000).ConfigureAwait(false);
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

        private Task RunWatcherAsync(Action<FileSystemEventArgs> processAction, CancellationToken cancellationToken)
        {
            var watcher = new FileSystemEventCollection(cancellationToken, Path.GetTempPath(), $"*.{_tempFileExtension}").GetEnumerator();

            Task.Run(() =>
            {
                while (watcher.MoveNext())
                {
                    processAction(watcher.Current);
                }
            });

            return Task.CompletedTask;
        }
    }
}