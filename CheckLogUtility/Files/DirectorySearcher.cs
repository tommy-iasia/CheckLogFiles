using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CheckLogUtility.Files
{
    public class DirectorySearcher
    {
        public string[] IgnoreNames { get; set; }

        public async IAsyncEnumerable<DirectoryInfo> EnumerateDirectoriesAsync(DirectoryInfo root, int width, int depth)
        {
            var directories = root.EnumerateDirectories();

            var count = 0;

            foreach (var directory in directories)
            {
                if (!(IgnoreNames?.Contains(directory.Name) ?? false))
                {
                    if (++count > width)
                    {
                        yield break;
                    }

                    yield return directory;

                    if (depth > 0)
                    {
                        var subDirectories = EnumerateDirectoriesAsync(directory, width, depth - 1);
                        await foreach (var subDirectory in subDirectories)
                        {
                            yield return subDirectory;
                        }
                    }
                }
            }
        }
    }
}
