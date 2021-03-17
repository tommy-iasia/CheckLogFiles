using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckLogWorker
{
    public class HarddiskGrowth : HarddiskRunner
    {
        protected override async Task RunAsync(DriveInfo drive, Logger logger)
        {
            var oldRecord = await FindRecordAsync();

            var newRecord = drive.ToRecord(DateTime.Now);
            await SaveRecordAsync(newRecord);

            if (oldRecord != null)
            {
                await logger.InfoAsync($"From {oldRecord}");
                await logger.InfoAsync($"To {newRecord}");

                var (rateSize, rateTime) = GetRateValue();
                await logger.InfoAsync($"Grown {rateSize}B/{rateTime}s");

                var realSize = oldRecord.AvailableFreeSpace - newRecord.AvailableFreeSpace;
                var realTime = (long) (newRecord.Time - oldRecord.Time).TotalSeconds;

                if (realSize * rateTime > rateSize * realTime)
                {
                    await logger.WarnAsync($"Drive {Drive} has grown {rateSize}B in {rateTime}s more than {Rate} allowed.");
                }
            }
            else
            {
                await logger.InfoAsync("No old record.");
                await logger.InfoAsync($"New {newRecord}");
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

        public string Rate { get; set; }
        private (long size, long time) GetRateValue()
        {
            var match = Regex.Match(Rate.ToLower(),
                @"(?<size>[^/]+)/(?<value>\d+)?(?<unit>s|second|m|min|minute|h|hr|hour|d|day)");

            if (!match.Success)
            {
                throw new InvalidOperationException();
            }

            var sizeText = match.Groups["size"].Value;
            var sizeValue = HarddiskRemain.GetSpaceValue(sizeText);

            var valueGroup = match.Groups["value"];
            var valueValue = valueGroup.Success ? long.Parse(match.Groups["value"].Value) : 1;

            var unitGroup = match.Groups["unit"];
            var unitValue = unitGroup.Success ?
                unitGroup.Value.StartsWith("d") ? 24 * 60 * 60
                : unitGroup.Value.StartsWith("h") ? 60 * 60
                : unitGroup.Value.StartsWith("m") ? 60
                : 1 : 1;

            return (sizeValue, valueValue * unitValue);
        }
    }
}
