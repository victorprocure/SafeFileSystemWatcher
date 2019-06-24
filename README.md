# Thread Safe File System Watcher

This is a thread safe implementation of the file system watcher as an enumerable collection of fired events.

## Example

### Using Watcher

```cs
internal static class Program
    {
        private static void Main()
        {
            using (var cancel = new CancellationTokenSource())
            {
                var watcher = new Watcher(OnFileEvent, new FileSystemEventConfiguration("c:\\temp"), cancel.Token);
                watcher.Watch();    

                Console.ReadLine();
                cancel.Cancel();
            }
        }

        private static void OnFileEvent(FileSystemEventArgs fileSystemEvent)
            => Console.WriteLine("{0} {1} {2}", fileSystemEvent.FullPath, fileSystemEvent.ChangeType, fileSystemEvent.Name);
    }
```

### Using Collection Directly

```cs
internal static class Program
    {
        private static void Main()
        {
            using (var cancel = new CancellationTokenSource())
            {
                var watcher = new FileSystemEventCollection(cancel.Token, "c:\\temp").GetEnumerator();

                Task.Run(() =>
                  {
                      while (watcher.MoveNext())
                      {
                          var item = watcher.Current;

                          Console.WriteLine("{0} {1} {2}", item.FullPath, item.ChangeType, item.Name);
                      }
                  });

                Console.ReadLine();
                cancel.Cancel();
            }
        }
    }
```