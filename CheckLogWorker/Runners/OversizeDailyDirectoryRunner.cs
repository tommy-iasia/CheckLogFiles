using CheckLogUtility.Linq;
using CheckLogUtility.Logging;
using CheckLogUtility.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public class OversizeDailyDirectoryRunner : IRunner
    {
        public async Task<bool> PrepareAsync(Logger logger)
        {
            if (string.IsNullOrWhiteSpace(PathPattern))
            {
                await logger.ErrorAsync($"Configure {nameof(PathPattern)} should not be empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(WarnSize))
            {
                await logger.ErrorAsync($"Configure {nameof(WarnSize)} is empty");
                return false;
            }
            if (!UnitText.TryParseSize(WarnSize, out var warnSize))
            {
                await logger.ErrorAsync($"Configure {nameof(WarnSize)} should be in size format");
                return false;
            }

            await logger.InfoAsync($"Warn will be raised when less than {warnSize}B");

            if (string.IsNullOrWhiteSpace(ErrorSize))
            {
                await logger.ErrorAsync($"Configure {nameof(ErrorSize)} is empty");
                return false;
            }
            if (!UnitText.TryParseSize(ErrorSize, out var errorSize))
            {
                await logger.ErrorAsync($"Configure {nameof(ErrorSize)} should be in size format");
                return false;
            }

            await logger.InfoAsync($"Error will be raised when less than {errorSize}B");

            return true;
        }

        public string PathPattern { get; set; }
        public string WarnSize { get; set; }
        public string ErrorSize { get; set; }
        public async Task RunAsync(Logger logger, CancellationToken cancellationToken)
        {
            var directoryPath = DailyFileRunner.TryTimeReplace(PathPattern, DateTime.Now);
            await logger.InfoAsync($"Folder path {directoryPath} for today");

            var fileInfos = await GetFileInfosAsync(directoryPath, logger, cancellationToken).ToArrayAsync();
            var directorySize = fileInfos.Sum(t => t.Length);
            await logger.InfoAsync($"Folder is of {directorySize}B");

            var warnSize = UnitText.ParseSize(WarnSize);
            var errorSize = UnitText.ParseSize(ErrorSize);

            if (directorySize >= errorSize)
            {
                await logger.ErrorAsync($"Folder of {UnitText.ToSize(directorySize)}B is larger than {ErrorSize} allowed");
            }
            else if (directorySize >= warnSize)
            {
                await logger.WarnAsync($"Folder of {UnitText.ToSize(directorySize)}B is larger than {WarnSize} allowed");
            }
            else
            {
                await logger.InfoAsync($"Folder of {UnitText.ToSize(directorySize)}B is normal");
                return;
            }

            var largeFiles = fileInfos.OrderByDescending(t => t.Length).Take(3);
            foreach (var file in largeFiles)
            {
                await logger.InfoAsync($"{file} is of {file.Length}B/{UnitText.ToSize(file.Length)}B");
            }
        }
        private static async IAsyncEnumerable<FileInfo> GetFileInfosAsync(string directoryPath, Logger logger, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (!Directory.Exists(directoryPath))
            {
                await logger.InfoAsync($"Folder does not exists");
                yield break;
            }

            const int fileCountLimit = 1001;
            var count = 0;

            var files = Directory.EnumerateFiles(directoryPath);
            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (++count >= fileCountLimit)
                {
                    await logger.ErrorAsync($"Cannot check folder with more than {fileCountLimit - 1} files.");
                    yield break;
                }

                yield return new FileInfo(file);
            }
        }
    }
}
