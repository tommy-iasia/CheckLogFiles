using CheckLogWorker.Enumerable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public class RetransmissionRejectedRunner : DailyLogFileRunner
    {
        protected override async Task RunAsync(string filePath, IAsyncEnumerable<string> lines, Logger logger)
        {
            var regex = new Regex(@"RetransmissionTask\] \[Retransmission Response \(\d+\). Retrans Status=2 Channel=(?<channel>\d+) BeginSeq=(?<begin>\d+) EndSeq=(?<end>\d+)\]");

            var rejects = await lines.SelectAsync(t => regex.Match(t))
                .WhereAsync(t => t.Success)
                .SelectAsync(t => (
                    channel: int.TryParse(t.Groups["channel"].Value, out var channel) ? channel : -1,
                    begin: int.TryParse(t.Groups["begin"].Value, out var begin) ? begin : -1,
                    end: int.TryParse(t.Groups["end"].Value, out var end) ? end : -1))
                .WhereAsync(t => t.channel >= 0 && t.begin >= 0 && t.end >= 0)
                .ToArrayAsync();

            await logger.InfoAsync($"{rejects.Length} re-transmissions are rejected");

            if (!rejects.Any())
            {
                return;
            }

            foreach (var (channel, begin, end) in rejects)
            {
                await logger.ErrorAsync($"Re-transmission in {channel} are rejected from {begin} to {end}");
            }
        }
    }
}
