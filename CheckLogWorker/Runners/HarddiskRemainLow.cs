using System.IO;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public class HarddiskRemainLowRunner : HarddiskRunner
    {
        public override async Task<bool> PrepareAsync(Logger logger)
        {
            if (!await base.PrepareAsync(logger))
            {
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

            await logger.InfoAsync($"Warning will be raised when less than {warnSize}B");

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

        public string WarnSize { get; set; }
        public string ErrorSize { get; set; }
        protected override async Task RunAsync(DriveInfo drive, Logger logger)
        {
            await logger.InfoAsync($"{drive.AvailableFreeSpace / 1024 / 1024 / 1024}GB/{drive.AvailableFreeSpace}B is free in {Drive}");

            var criticalValue = UnitText.ParseSize(ErrorSize);
            if (drive.AvailableFreeSpace <= criticalValue)
            {
                await logger.InfoAsync($"{drive.AvailableFreeSpace} has less than {criticalValue}");
                await logger.ErrorAsync($"Drive {drive.Name} is lower than {ErrorSize}");
            }
            else
            {
                var warningValue = UnitText.ParseSize(WarnSize);
                if (drive.AvailableFreeSpace < warningValue)
                {
                    await logger.InfoAsync($"{drive.AvailableFreeSpace} has less than {warningValue}");
                    await logger.WarnAsync($"Drive {drive.Name} is lower than {WarnSize}");
                }
            }
        }
    }
}
