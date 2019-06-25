# Thread Safe File System Watcher

[![Build status](https://ci.appveyor.com/api/projects/status/u75h99fbktsdhq49?svg=true)](https://ci.appveyor.com/project/victorprocure/safefilesystemwatcher)  [![CodeFactor](https://www.codefactor.io/repository/github/victorprocure/safefilesystemwatcher/badge)](https://www.codefactor.io/repository/github/victorprocure/safefilesystemwatcher)

This is a thread safe implementation of the file system watcher as an enumerable collection of fired events.

## Example

### Using Watcher

```cs
internal static class Program
    {
        private static void Main()
        {
            using (var cancel = new CancellationTokenSource())
            using (var watcher = new Watcher(InternalChanger, config, cts.Token))
            {
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
            using (var watcher = new FileSystemEventCollection(cancel.Token, "c:\\temp"))
            {
                var watcherEnumerator = watcher.GetEnumerator();

                Task.Run(() =>
                  {
                      while (watcherEnumerator.MoveNext())
                      {
                          var item = watcherEnumerator.Current;

                          Console.WriteLine("{0} {1} {2}", item.FullPath, item.ChangeType, item.Name);
                      }
                  });

                Console.ReadLine();
                cancel.Cancel();
            }
        }
    }
```