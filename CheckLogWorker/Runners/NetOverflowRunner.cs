using CheckLogUtility.Linq;
using CheckLogUtility.Logging;
using CheckLogUtility.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public abstract class NetOverflowRunner : DailyLogFileRunner
    {
        public override async Task<bool> PrepareAsync(Logger logger)
        {
            if (!await base.PrepareAsync(logger))
            {
                return false;
            }

            if (IgnoreCount <= 0)
            {
                await logger.ErrorAsync($"Configure {nameof(IgnoreCount)} should not be less than zero");
                return false;
            }

            if (string.IsNullOrWhiteSpace(IgnoreSpan))
            {
                await logger.ErrorAsync($"Configure {nameof(IgnoreSpan)} should not be empty");
                return false;
            }

            if (!UnitText.TryParseSpan(IgnoreSpan, out _))
            {
                await logger.ErrorAsync($"Configure {nameof(IgnoreSpan)} should not be an invalid time span");
                return false;
            }

            return await Task.FromResult(true);
        }

        protected abstract Regex LineRegex { get; }
        protected override async Task RunAsync(string filePath, IAsyncEnumerable<string> lines, Logger logger, CancellationToken cancellationToken)
        {
            var regex = LineRegex;

            var logs = await lines.SelectAsync(t => regex.Match(t))
                .WhereAsync(t => t.Success)
                .SelectAsync(t => (time: DateTime.TryParse(t.Groups["time"].Value, out var time) ? time : default, client: t.Groups["client"].Value))
                .WhereAsync(t => t.time != default)
                .WithCancellation(cancellationToken)
                .ToArrayAsync();

            var overflows = logs.GroupBy(t => t.client, t => t.time)
                .Select(t => new NetOverflowRecord
                {
                    Client = t.Key,
                    Start = t.Min(),
                    End = t.Max(),
                    Count = t.Count()
                })
                .ToArray();

            await logger.InfoAsync($"{overflows.Length} overflows are found.");

            if (!overflows.Any())
            {
                return;
            }

            await RunAsync(overflows, filePath, logger, cancellationToken);
        }
        protected abstract Task RunAsync(IEnumerable<NetOverflowRecord> overflows, string filePath, Logger logger, CancellationToken cancellationToken);

        public int IgnoreCount { get; set; }
        public string IgnoreSpan { get; set; }
    }
}
