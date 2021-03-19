using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CheckLogUpdater
{
    public class Continue
    {
        public static async Task RunAsync(IEnumerable<string> arguments)
        {
            var version = arguments.First();
            Console.WriteLine($"Apply update {version}...");

            var configure = await Configure.LoadAsync();
            if (configure.Version.CompareTo(version) >= 0)
            {
                throw new InvalidOperationException($"Cannot update from version {configure.Version} to {version}");
            }

            var bare = arguments.Contains("-bare");
            if (!bare)
            {
                Move(version);
            }

            configure.Version = version;
            await configure.SaveAsync();

            if (!bare)
            {
                await Prepare.RunAsync();
            }
        }

        private static void Move(string version)
        {
            var directory = Prepare.GetDirectory(version);
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            Console.WriteLine($"Move {files.Length} files from {directory}");

            foreach (var file in files)
            {
                var relativePath = file[(directory.Length + 1)..];

                Console.WriteLine($"Move {relativePath}");
                File.Move(file, relativePath, overwrite: true);
            }
        }
    }
}
