using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public class HarddiskOverGrowthRunner : HarddiskRunner
    {
        public override async Task<bool> PrepareAsync(Logger logger)
        {
            if (!await base.PrepareAsync(logger))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(WarnRate))
            {
                await logger.ErrorAsync($"Configure {nameof(WarnRate)} is empty");
                return false;
            }
            if (!TryParseRate(WarnRate, out var warnRate))
            {
                await logger.ErrorAsync($"Configure {nameof(WarnRate)} should be in rate format");
                return false;
            }

            await logger.InfoAsync($"Warn will be raised at {warnRate.size}B/{warnRate.time.TotalSeconds}s");

            if (string.IsNullOrWhiteSpace(ErrorRate))
            {
                await logger.ErrorAsync($"Configure {nameof(ErrorRate)} is empty");
                return false;
            }
            if (!TryParseRate(ErrorRate, out var errorRate))
            {
                await logger.ErrorAsync($"Configure {nameof(ErrorRate)} should be in rate format");
                return false;
            }

            await logger.InfoAsync($"Error will be raised at {errorRate.size:#,##0}B/{errorRate.time.TotalSeconds:#,##0}s");

            return true;
        }

        protected override async Task RunAsync(DriveInfo drive, Logger logger)
        {
            var oldRecord = await FindRecordAsync();

            var newRecord = drive.ToRecord(DateTime.Now);
            await SaveRecordAsync(newRecord);

            if (oldRecord == null)
            {
                await logger.InfoAsync("No old record.");
                await logger.InfoAsync($"New {newRecord}");
                return;
            }

            await logger.InfoAsync($"From {oldRecord}");
            await logger.InfoAsync($"To {newRecord}");

            var realSize = oldRecord.AvailableFreeSpace - newRecord.AvailableFreeSpace;
            var realTime = newRecord.Time - oldRecord.Time;
            await logger.InfoAsync($"Growth is {realSize:#,##0}B/{(long)realTime.TotalSeconds:#,##0}s");

            if (realTime < TimeSpan.FromSeconds(1))
            {
                await logger.InfoAsync($"Last invoke was at {oldRecord.Time}");
                await logger.InfoAsync($"This invoke is at {newRecord.Time}");

                await logger.WarnAsync($"Invoked too frequently is meaningless");
                await logger.WarnAsync($"Skip checking");
                return;
            }

            _ = TryParseRate(ErrorRate, out var errorRate);
            if (realSize * errorRate.time.TotalMilliseconds > errorRate.size * realTime.TotalMilliseconds)
            {
                await logger.WarnAsync($"Drive {Drive} has "
                    + $"grown {UnitText.ToSize(realSize)}B "
                    + $"within {UnitText.ToSpan(realTime)}"
                    + $"more than allowed.");
            }
            else
            {
                _ = TryParseRate(WarnRate, out var warnRate);
                if (realSize * warnRate.time.TotalMilliseconds > warnRate.size * realTime.TotalMilliseconds)
                {
                    await logger.WarnAsync($"Drive {Drive} has "
                        + $"grown {UnitText.ToSize(realSize)}B "
                        + $"within {UnitText.ToSpan(realTime)}"
                        + $"more than allowed.");
                }
                else
                {
                    await logger.InfoAsync($"Growth rate is normal");
                }
            }
        }

        protected async Task<HarddiskSpaceRecord> FindRecordAsync()
        {
            try
            {
                var records = await HarddiskSpaceRecord.LoadAsync();
                return records.FirstOrDefault(t => t.Name == Drive);
            }
            catch (Exception)
            {
                return null;
            }
        }
        protected static async Task SaveRecordAsync(HarddiskSpaceRecord record)
        {
            HarddiskSpaceRecord[] oldRecords;
            try
            {
                oldRecords = await HarddiskSpaceRecord.LoadAsync();
            }
            catch (Exception)
            {
                oldRecords = Array.Empty<HarddiskSpaceRecord>();
            }

            var newRecords = oldRecords
                .Where(t => t.Name != record.Name)
                .Append(record)
                .OrderBy(t => t.Name)
                .ToArray();

            await HarddiskSpaceRecord.SaveAsync(newRecords);
        }

        public string WarnRate { get; set; }
        public string ErrorRate { get; set; }
        private static bool TryParseRate(string text, out (long size, TimeSpan time) rate)
        {
            var match = Regex.Match(text, @"(?<size>[\w ]+)/(?<time>[\w ]+)");

            if (!match.Success)
            {
                rate = default;
                return false;
            }

            var sizeText = match.Groups["size"].Value;
            if (!UnitText.TryParseSize(sizeText, out var sizeValue))
            {
                rate = default;
                return false;
            }

            var timeText = match.Groups["time"].Value;
            if (!UnitText.TryParseSpan(timeText, out var timeValue))
            {
                rate = default;
                return false;
            }

            rate = (sizeValue, timeValue);
            return true;
        }
    }
}
