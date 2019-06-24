using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using SafeFileSystemWatcher;
using SafeFileSystemWatcher.Configurations;
using Xunit;

namespace Tests.SafeFileSystemWatcher
{
    public class WatcherTests
    {
        private static readonly string _tempFileExtension = Guid.NewGuid().ToString("N");

        [Fact]
        public void GivenFileSystemEventWatcherShouldExecuteCallback()
        {
            var changedFiles = new List<string>();
            var config = new FileSystemEventConfiguration(Path.GetTempPath(), $"*.{_tempFileExtension}");

            using (var cts = new CancellationTokenSource())
            using (var watcher = new Watcher(InternalChanger, config, cts.Token))
            {
                watcher.Watch();

                var tempFile = CreateTempFile();
                Thread.Sleep(2000);
                TryDeleteFile(tempFile);
                cts.Cancel();
                Assert.Contains(changedFiles, c => c == tempFile);
            }

            void InternalChanger(FileSystemEventArgs fse)
            {
                lock (changedFiles)
                {
                    changedFiles.Add(fse.FullPath);
                }
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
    }
}