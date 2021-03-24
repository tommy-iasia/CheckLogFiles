using CheckLogUtility.Linq;
using CheckLogUtility.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public class RetransmissionRejectedRunner : DailyLogFileRunner
    {
        public override async Task<bool> PrepareAsync(Logger logger)
        {
            if (!await base.PrepareAsync(logger))
            {
                return false;
            }

            if (!FilePattern.EndsWith(LargeRetransmissionRequestRunner.FileName))
            {
                await logger.ErrorAsync($"Configure {nameof(FilePattern)} should end with \"{LargeRetransmissionRequestRunner.FileName}\"");
                return false;
            }

            return true;
        }

        protected override async Task RunAsync(string filePath, IAsyncEnumerable<string> lines, Logger logger, CancellationToken cancellationToken)
        {
            var regex = new Regex(@"RetransmissionTask\] \[Retransmission Response \(\d+\). Retrans Status=2 Channel=(?<channel>\d+) BeginSeq=(?<begin>\d+) EndSeq=(?<end>\d+)\]");

            var rejects = await lines.SelectAsync(t => regex.Match(t))
                .WhereAsync(t => t.Success)
                .SelectAsync(t => (
                    channel: int.TryParse(t.Groups["channel"].Value, out var channel) ? channel : -1,
                    begin: int.TryParse(t.Groups["begin"].Value, out var begin) ? begin : -1,
                    end: int.TryParse(t.Groups["end"].Value, out var end) ? end : -1))
                .WhereAsync(t => t.channel >= 0 && t.begin >= 0 && t.end >= 0)
                .WithCancellation(cancellationToken)
                .ToArrayAsync();

            await logger.InfoAsync($"{rejects.Length} re-transmissions are rejected");

            if (!rejects.Any())
            {
                return;
            }

            foreach (var (channel, begin, end) in rejects)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await logger.ErrorAsync($"Re-transmission in channel {channel} from {begin} to {end} is rejected");
            }
        }
    }
}
