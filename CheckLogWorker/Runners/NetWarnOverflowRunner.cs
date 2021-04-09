using CheckLogUtility.Linq;
using CheckLogUtility.Logging;
using CheckLogUtility.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public class NetWarnOverflowRunner : NetOverflowRunner
    {
        public override async Task<bool> PrepareAsync(Logger logger)
        {
            if (!await base.PrepareAsync(logger))
            {
                return false;
            }

            if (!FilePattern.EndsWith(WarnLogName))
            {
                await logger.ErrorAsync($"Configure {nameof(FilePattern)} should end with {WarnLogName}");
                return false;
            }

            return true;
        }

        protected override Regex LineRegex => new(@"\[(?<time>[\d/ :.]+)\] \[NetClient\] \[appendWriteBuffer \| Write Buffer Overflow \| buffLen: \d+ \| writeBuff: java\.nio\.HeapByteBuffer\[pos=\d+ lim=\d+ cap=\d+\] \| (?<client>[\d.:]+)\]");
        protected override async Task RunAsync(IEnumerable<NetOverflowRecord> overflows, string filePath, Logger logger, CancellationToken cancellationToken)
        {
            var serverPorts = await GetServerPortsAsync(filePath, logger);

            var ignoreTime = UnitText.ParseSpan(IgnoreSpan);

            foreach (var overflow in overflows)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var content = $"{overflow.Count}c from {overflow.Start:HH:mm:ss} to {overflow.End:HH:mm:ss}";

                if (overflow.Count > IgnoreCount
                    && overflow.End - overflow.Start > ignoreTime)
                {
                    if (serverPorts.TryGetValue(overflow.Client, out var port))
                    {
                        if (ErrorPorts?.Contains(port) ?? false)
                        {
                            await logger.ErrorAsync($"{overflow.Client} at critical port {port} overflowed {content}");
                        }
                        else
                        {
                            await logger.WarnAsync($"{overflow.Client} overflowed {content}");
                        }
                    }
                    else
                    {
                        await logger.ErrorAsync($"{overflow.Client} overflowed {content} but cannot be found in {InfoLogName}");
                    }
                }
                else
                {
                    await logger.InfoAsync($"{overflow.Client} overflowed {content}");
                }
            }
        }

        private const string WarnLogName = "NetWarnLog.txt";
        private const string InfoLogName = "NetInfoLog.txt";
        private static async Task<Dictionary<string, int>> GetServerPortsAsync(string warnPath, Logger logger)
        {
            var filePath = warnPath.Replace(WarnLogName, InfoLogName);

            using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var lines = ReadLinesAsync(stream, capacity: 100_000, logger);

            var regex = new Regex(@"\[validate \| server : [\d.]+:(?<port>\d+) \| client : (?<client>[\d.:]+) \|");

            var portClients = await lines.SelectAsync(t => regex.Match(t))
                .SelectAsync(t => (port: int.TryParse(t.Groups["port"].Value, out var port) ? port : 0, client: t.Groups["client"].Value))
                .WhereAsync(t => t.port > 0)
                .ToListAsync();

            return portClients.ToLookup(t => t.client, t => t.port)
                .ToDictionary(t => t.Key, t => t.Last());
        }

        public int[] ErrorPorts { get; set; }
    }
}
