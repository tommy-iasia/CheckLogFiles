using CheckLogWorker.Enumerable;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public class LargeRetransmissionRequestRunner : DailyLogFileRunner
    {
        public override async Task<bool> PrepareAsync(Logger logger)
        {
            if (!await base.PrepareAsync(logger))
            {
                return false;
            }

            if (WarnCount <= 0)
            {
                await logger.ErrorAsync($"Configure {nameof(WarnCount)} should not be less than zero");
                return false;
            }

            await logger.InfoAsync($"Warning will be raised when more than {WarnCount} sequences");

            if (ErrorCount <= 0)
            {
                await logger.ErrorAsync($"Configure {nameof(ErrorCount)} should not be less than zero");
                return false;
            }

            await logger.InfoAsync($"Error will be raised when more than {ErrorCount} sequences");

            return true;
        }
        public int WarnCount { get; set; }
        public int ErrorCount { get; set; }

        protected override async Task RunAsync(string filePath, IAsyncEnumerable<string> lines, Logger logger)
        {
            var regex = new Regex(@"RetransmissionManager\] \[\[(?<channel>\d+)\]SEND Retansmission From=(?<from>\d+) To=(?<to>\d+)\]");

            var requests = await lines.SelectAsync(t => regex.Match(t))
                .WhereAsync(t => t.Success)
                .SelectAsync(t => (
                    channel: int.TryParse(t.Groups["channel"].Value, out var channel) ? channel : -1,
                    from: int.TryParse(t.Groups["from"].Value, out var from) ? from : -1,
                    to: int.TryParse(t.Groups["to"].Value, out var to) ? to : -1))
                .WhereAsync(t => t.channel >= 0 && t.from >= 0 && t.to >= 0)
                .ToArrayAsync();

            await logger.InfoAsync($"{requests.Length} re-transmissions are requested");

            if (!requests.Any())
            {
                return;
            }

            foreach (var (channel, from, to) in requests)
            {
                var count = to - from + 1;
                await logger.InfoAsync($"Re-transmission {count} sequences from {from} to {to} in channel {channel}");

                if (count >= ErrorCount)
                {
                    await logger.ErrorAsync($"Re-tranmission {count} sequences in channel {channel} is alerted");
                }
                else if (count >= WarnCount)
                {
                    await logger.WarnAsync($"Re-tranmission {count} sequences in channel {channel} is alerted");
                }
            }
        }
    }
}
