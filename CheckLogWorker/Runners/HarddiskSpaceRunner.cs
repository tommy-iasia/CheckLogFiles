using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public abstract class HarddiskRunner : IRunner
    {
        public virtual async Task<bool> PrepareAsync(Logger logger)
        {
            if (string.IsNullOrWhiteSpace(Drive))
            {
                await logger.ErrorAsync($"Configure {nameof(Drive)} is empty");
                return false;
            }

            return true;
        }

        public string Drive { get; set; }
        public async Task RunAsync(Logger logger)
        {
            var drives = DriveInfo.GetDrives();

            var drive = drives.FirstOrDefault(t => t.Name == Drive);
            if (drive == null)
            {
                await logger.ErrorAsync($"Cannot find drive {Drive}");

                var names = drives.Select(t => t.Name);
                await logger.InfoAsync($"Only {string.Join(", ", names)} are available.");
            }

            await RunAsync(drive, logger);
        }
        protected abstract Task RunAsync(DriveInfo drive, Logger logger);
    }
}
