using CheckLogWorker.Enumerable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public class KpiQueueRunner : DailyLogFileRunner
    {
        public override async Task<bool> PrepareAsync(Logger logger)
        {
            if (!await base.PrepareAsync(logger))
            {
                return false;
            }

            if (IgnoreQueuePatterns != null)
            {
                foreach (var pattern in IgnoreQueuePatterns)
                {
                    if (string.IsNullOrWhiteSpace(pattern))
                    {
                        await logger.ErrorAsync($"Configure {nameof(IgnoreQueuePatterns)} should not contains empty item");
                        return false;
                    }

                    try
                    {
                        _ = new Regex(pattern);
                    }
                    catch (Exception)
                    {
                        await logger.ErrorAsync($"Configure {nameof(IgnoreQueuePatterns)} pattern \"{IgnoreQueuePatterns}\" is invalid");
                        return false;
                    }
                }
            }

            if (WarnProportion <= 0)
            {
                await logger.ErrorAsync($"Configure {nameof(WarnProportion)} should not be less than zero");
                return false;
            }

            if (WarnCount <= 0)
            {
                await logger.ErrorAsync($"Configure {nameof(WarnCount)} should not be less than zero");
                return false;
            }

            if (ErrorProportion <= 0)
            {
                await logger.ErrorAsync($"Configure {nameof(ErrorProportion)} should not be less than zero");
                return false;
            }

            if (ErrorCount <= 0)
            {
                await logger.ErrorAsync($"Configure {nameof(ErrorCount)} should not be less than zero");
                return false;
            }

            return true;
        }

        protected override async Task RunAsync(string filePath, IAsyncEnumerable<string> lines, Logger logger)
        {
            var records = await GetQueuesRecordsAsync(lines);
            if (!records.Any())
            {
                await logger.InfoAsync($"No queue is found");
            }

            var queues = records.Select(t => t.Key);
            await logger.InfoAsync($"{records.Length} queues are found: {string.Join(", ", queues)}");

            var pairs = GetQueuePairs(records).ToArray();
            await logger.InfoAsync($"{pairs.Length} pairs are found: {string.Join(", ", queues)}");

            foreach (var (queue, add, process) in pairs)
            {
                await LogQueuePairAsync(queue, add, process, logger);
            }
        }

        private async Task<IGrouping<string, KpiQueueRecord>[]> GetQueuesRecordsAsync(IAsyncEnumerable<string> lines)
        {
            var allRecords = await GetQueueRecordsAsync(lines).ToArrayAsync();
            var queuesRecords = allRecords.GroupBy(t => t.Queue);

            var ignoreQueueRegexes = GetIgnoreQueueRegexes();
            return queuesRecords.Where(t => !ignoreQueueRegexes.Any(s => s.IsMatch(t.Key))).ToArray();
        }
        private static async IAsyncEnumerable<KpiQueueRecord> GetQueueRecordsAsync(IAsyncEnumerable<string> lines)
        {
            var lineRegex = new Regex(@"\[(?<time>[\d/: .]+)] \[KPI\] \[\w+\s+(?<queue>[\w-.]+)\s+\d+\s+(?<count>\d+)");
            var queueRegex = new Regex(@"^(?<name>[\w-.]+)\.(?<action>\w+)$");

            await foreach (var line in lines)
            {
                var lineMatch = lineRegex.Match(line);
                if (lineMatch.Success)
                {
                    var timeText = lineMatch.Groups["time"].Value;
                    if (DateTime.TryParse(timeText, out var timeValue))
                    {
                        var queueText = lineMatch.Groups["queue"].Value;
                        var queueMatch = queueRegex.Match(queueText);
                        if (queueMatch.Success)
                        {
                            var queue = queueMatch.Groups["name"].Value;
                            var action = queueMatch.Groups["action"].Value;

                            var countText = lineMatch.Groups["count"].Value;
                            if (long.TryParse(countText, out var countValue))
                            {
                                yield return new KpiQueueRecord
                                {
                                    Time = timeValue,
                                    Queue = queue,
                                    Action = action,
                                    Count = countValue
                                };
                            }
                        }
                    }
                }
            }
        }

        public string[] IgnoreQueuePatterns { get; set; }
        private IEnumerable<Regex> GetIgnoreQueueRegexes() => IgnoreQueuePatterns?.Select(t => new Regex(t)) ?? Array.Empty<Regex>();

        private static IEnumerable<(string queue, KpiQueueRecord add, KpiQueueRecord process)> GetQueuePairs(IGrouping<string, KpiQueueRecord>[] queuesRecords)
        {
            foreach (var queueRecords in queuesRecords)
            {
                var lastAdd = queueRecords.LastOrDefault(t => t.Action == "addQueue");
                if (lastAdd != null)
                {
                    var lastProcess = queueRecords
                        .Where(t => t.Action == "process")
                        .Where(t => Math.Abs((t.Time - lastAdd.Time).TotalSeconds) <= 1)
                        .LastOrDefault();
                    if (lastProcess != null)
                    {
                        yield return (queueRecords.Key, lastAdd, lastProcess);
                    }
                }
            }
        }

        public double WarnProportion { get; set; }
        public long WarnCount { get; set; }
        public double ErrorProportion { get; set; }
        public long ErrorCount { get; set; }
        private async Task LogQueuePairAsync(string queue, KpiQueueRecord add, KpiQueueRecord process, Logger logger)
        {
            if (add.Count > 0)
            {
                var count = process.Count - add.Count;
                if (count > 0)
                {
                    var proportion = (double)count / add.Count;

                    var message = $"Queue {queue} has {count}c/{add.Count}c or {Math.Round(proportion * 100)}% unprocessed";

                    if (proportion >= ErrorProportion
                        && count >= ErrorCount)
                    {
                        await logger.ErrorAsync(message);
                    }
                    else if (proportion >= WarnProportion
                        && count >= WarnCount)
                    {
                        await logger.WarnAsync(message);
                    }
                    else
                    {
                        await logger.InfoAsync(message);
                    }
                }
                else
                {
                    await logger.InfoAsync($"Queue {queue} has processed all {add.Count} items");
                }
            }
            else
            {
                await logger.InfoAsync($"Queue {queue} has no item yet");
            }
        }
    }
}
