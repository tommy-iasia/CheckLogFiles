using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckLogWorker
{
    public class HarddiskRemain : HarddiskRunner
    {
        protected override async Task RunAsync(DriveInfo drive, Logger logger)
        {
            await logger.InfoAsync($"{drive.AvailableFreeSpace / 1024 / 1024 / 1024}GB/{drive.AvailableFreeSpace}B is free in {Drive}");

            var criticalValue = GetSpaceValue(Critical);
            if (drive.AvailableFreeSpace <= criticalValue)
            {
                await logger.InfoAsync($"{drive.AvailableFreeSpace} has less than {criticalValue}");
                await logger.ErrorAsync($"Drive {drive.Name} is lower than {Critical}");
            }
            else
            {
                var warningValue = GetSpaceValue(Warning);
                if (drive.AvailableFreeSpace < warningValue)
                {
                    await logger.InfoAsync($"{drive.AvailableFreeSpace} has less than {warningValue}");
                    await logger.WarnAsync($"Drive {drive.Name} is lower than {Warning}");
                }
            }
        }

        public string Warning { get; set; }
        public string Critical { get; set; }

        public static long GetSpaceValue(string text)
        {
            var match = Regex.Match(text, @"(?<value>\d+)(?<unit>[KkMmGgTt])?B?");

            if (!match.Success)
            {
                throw new ArgumentException(null, nameof(text));
            }

            var valueText = match.Groups["value"].Value;
            var valueValue = long.Parse(valueText);

            var unitGroup = match.Groups["unit"];
            var unitValue = unitGroup.Success ?
                unitGroup.Value switch
                {
                    "K" => 1024,
                    "k" => 1000,
                    "M" => 1024 * 1024,
                    "m" => 1000 * 1000,
                    "G" => 1024 * 1024 * 1024,
                    "g" => 1000 * 1000 * 1000,
                    "T" => 1024L * 1024 * 1024 * 1024,
                    "t" => 1000L * 1000 * 1000 * 1000,
                    _ => throw new InvalidOperationException()
                } : 1;

            return valueValue * unitValue;
        }
    }
}
