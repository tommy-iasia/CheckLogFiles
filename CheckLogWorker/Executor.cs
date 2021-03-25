using CheckLogUtility.Json;
using CheckLogUtility.Linq;
using CheckLogUtility.Logging;
using CheckLogUtility.Randomize;
using CheckLogWorker.Runners;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLogWorker
{
    public class Executor
    {
        public string Identifier { get; set; }
        public string Runner { get; set; }

        public string Server { get; set; }
        public bool SkipSendInfo { get; set; }

        public static async Task RunAsync(string[] arguments, CancellationToken cancellationToken)
        {
            var logger = new Logger();

            if (!arguments.Any())
            {
                await logger.ErrorAsync("No configure is provided.");
                return;
            }

            var jsons = await arguments.SelectAsync(async t => await File.ReadAllTextAsync(t)).ToArrayAsync();
            var json = JsonSerializerUtility.Combine(jsons);

            var program = JsonSerializer.Deserialize<Executor>(json);
            if (string.IsNullOrWhiteSpace(program.Runner))
            {
                await logger.ErrorAsync("Runner is not provided.");
                return;
            }

            if (string.IsNullOrWhiteSpace(program.Identifier))
            {
                await logger.ErrorAsync("Identifier is not provided.");
                return;
            }

            var logName = new LogName
            {
                Identifier = program.Identifier,
                Runner = program.Runner,
                Time = DateTime.Now,
                Random = RandomUtility.Next(9999)
            };

            await logger.InfoAsync($"Id: {logName}");

            await logger.SetFileAsync($"{nameof(CheckLogWorker)}\\{logName}.log");

            await logger.InfoAsync($"Configure: {json}");

            IRunner runner = program.Runner switch
            {
                nameof(HarddiskRemainLowRunner) => JsonSerializer.Deserialize<HarddiskRemainLowRunner>(json),
                nameof(HarddiskOverGrowthRunner) => JsonSerializer.Deserialize<HarddiskOverGrowthRunner>(json),
                nameof(KpiQueueRunner) => JsonSerializer.Deserialize<KpiQueueRunner>(json),
                nameof(LargeRetransmissionRequestRunner) => JsonSerializer.Deserialize<LargeRetransmissionRequestRunner>(json),
                nameof(NetWarnOverflowRunner) => JsonSerializer.Deserialize<NetWarnOverflowRunner>(json),
                nameof(NetErrorOverflowRunner) => JsonSerializer.Deserialize<NetErrorOverflowRunner>(json),
                nameof(OversizeDailyDirectoryRunner) => JsonSerializer.Deserialize<OversizeDailyDirectoryRunner>(json),
                nameof(RetransmissionRejectedRunner) => JsonSerializer.Deserialize<RetransmissionRejectedRunner>(json),
                _ => throw new InvalidOperationException($"Cannot not match runner with name {program.Runner}")
            };

            await logger.InfoAsync("Prepare");

            if (!await runner.PrepareAsync(logger))
            {
                await logger.ErrorAsync("Failed at prepare stage");
                return;
            }

            await logger.InfoAsync("Start");

            try
            {
                await runner.RunAsync(logger, cancellationToken);
            }
            catch (Exception e)
            {
                await logger.ErrorAsync(e);
            }

            if (logger.Lines.Any(t => t.Level > LogLevel.Info)
                || !program.SkipSendInfo)
            {
                await logger.InfoAsync("Send");
                try
                {
                    await SendAsync(logName, program.Server, logger, cancellationToken);
                }
                catch (Exception e)
                {
                    await logger.ErrorAsync(e);
                }
            }
            else
            {
                await logger.InfoAsync("Only info is logged, skip sending");
            }

            await logger.InfoAsync("End");
        }

        private static async Task SendAsync(LogName name, string server, Logger logger, CancellationToken cancellationToken)
        {
            var uri = $"{server.TrimEnd('/')}/Log/Create";

            var text = await File.ReadAllTextAsync(logger.FilePath, cancellationToken);

            var submit = new LogSubmit
            {
                Name = name,
                Time = DateTime.Now,
                Text = text
            };

            using var client = new HttpClient();
            var message = await client.PostAsJsonAsync(uri, submit, cancellationToken);

            if (message.StatusCode != HttpStatusCode.OK)
            {
                await logger.ErrorAsync($"Fail to upload to {uri} with status code {(int)message.StatusCode} {message.StatusCode}");
            }
        }
    }
}
