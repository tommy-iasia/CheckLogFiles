using CheckLogUtility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CheckLogUpdater
{
    public class Continue
    {
        public static async Task RunAsync(IEnumerable<string> arguments, Logger logger)
        {
            var version = arguments.First();
            await logger.InfoAsync($"Apply update {version}...");

            var configure = await Configure.LoadAsync();
            if (configure.Version.CompareTo(version) >= 0)
            {
                throw new InvalidOperationException($"Cannot update from version {configure.Version} to {version}");
            }

            var bare = arguments.Contains("-bare");
            if (!bare)
            {
                await MoveAsync(version, logger);
            }

            configure.Version = version;
            await configure.SaveAsync();

            if (!bare)
            {
                await Prepare.RunAsync(logger);
            }
        }

        private static async Task MoveAsync(string version, Logger logger)
        {
            var directory = Prepare.GetDirectory(version);
            var files = Directory.GetFiles(directory, "*", SearchOption.AllDirectories);
            await logger.InfoAsync($"Move {files.Length} files from {directory}");

            foreach (var file in files)
            {
                var relativePath = file[(directory.Length + 1)..];

                await logger.InfoAsync($"Move {relativePath}");
                File.Move(file, relativePath, overwrite: true);
            }
        }
    }
}
