using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public abstract class DailyFileRunner : IRunner
    {
        public virtual async Task<bool> PrepareAsync(Logger logger)
        {
            if (string.IsNullOrWhiteSpace(FilePattern))
            {
                await logger.ErrorAsync($"Configure {nameof(FilePattern)} should not be empty");
                return false;
            }

            return true;
        }

        public async Task RunAsync(Logger logger)
        {
            var filePath = TryTimeReplace(FilePattern, DateTime.Now);

            await logger.InfoAsync($"File path {filePath} for today");

            await RunAsync(filePath, logger);
        }
        protected abstract Task RunAsync(string filePath, Logger logger);

        public string FilePattern { get; set; }
        public static string TryTimeReplace(string pattern, DateTime time)
        {
            return Regex.Replace(pattern, @"<(?<format>)>", t =>
            {
                var format = t.Groups["format"].Value;
                return time.ToString(format);
            });
        }
    }
}
