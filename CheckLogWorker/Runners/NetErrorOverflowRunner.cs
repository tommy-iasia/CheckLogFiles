using CheckLogUtility.Logging;
using CheckLogUtility.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public class NetErrorOverflowRunner : NetOverflowRunner
    {
        public override async Task<bool> PrepareAsync(Logger logger)
        {
            if (!await base.PrepareAsync(logger))
            {
                return false;
            }

            const string errorLogName = "NetErrorLog.txt";
            if (!FilePattern.EndsWith(errorLogName))
            {
                await logger.ErrorAsync($"Configure {nameof(FilePattern)} should end with {errorLogName}");
                return false;
            }

            return true;
        }

        protected override Regex LineRegex => new(@"\[(?<time>[\d/ :.]+)\] \[NetClient\] \[appendWriteBuffer \| (?<client>[\d.:]+) \| java.nio.BufferOverflowException\]");

        protected override async Task RunAsync(IEnumerable<NetOverflowRecord> overflows, string _, Logger logger, CancellationToken cancellationToken)
        {
            var ignoreTime = UnitText.ParseSpan(IgnoreSpan);

            foreach (var overflow in overflows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var message = $"{overflow.Client} overflowed {overflow.Count}c from {overflow.Start:HH:mm:ss} to {overflow.End:HH:mm:ss}";

                if (overflow.Count > IgnoreCount
                    && overflow.End - overflow.Start > ignoreTime)
                {
                    await logger.ErrorAsync(message);
                }
                else
                {
                    await logger.InfoAsync(message);
                }
            }
        }
    }
}
