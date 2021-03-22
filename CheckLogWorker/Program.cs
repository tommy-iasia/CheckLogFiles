using CheckLogWorker.Runners;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace CheckLogWorker
{
    public class Program
    {
        public string Identifier { get; set; }
        public string Runner { get; set; }

        public string Server { get; set; }
        public bool SkipSendInfo { get; set; }

        static async Task Main(string[] args)
        {
            if (!args.Any())
            {
                Console.Error.WriteLine("No configure is provided.");
                return;
            }

            var json = await GetConfigureJsonAsync(args);

            var program = JsonSerializer.Deserialize<Program>(json);
            if (string.IsNullOrWhiteSpace(program.Runner))
            {
                Console.Error.WriteLine("Runner is not provided.");
                return;
            }

            if (string.IsNullOrWhiteSpace(program.Identifier))
            {
                Console.Error.WriteLine("Identifier is not provided.");
                return;
            }

            var random = new Random();
            var logName = new LogName
            {
                Identifier = program.Identifier,
                Runner = program.Runner,
                Time = DateTime.Now,
                Random = random.Next(9999)
            };
            
            var logger = new Logger($"{logName}.log");
            await logger.InfoAsync($"Id: {logName}");

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
                await runner.RunAsync(logger);
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
                    await SendAsync(logName, program.Server, logger);
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
        private static async Task<string> GetConfigureJsonAsync(string[] filePaths)
        {
            var outputDictionary = new Dictionary<string, object>();

            foreach (var filePath in filePaths)
            {
                var json = await File.ReadAllTextAsync(filePath);
                var jsonDictionary = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                foreach (var (key, value) in jsonDictionary)
                {
                    outputDictionary[key] = value;
                }
            }

            return JsonSerializer.Serialize(outputDictionary);
        }

        private static async Task SendAsync(LogName name, string server, Logger logger)
        {
            var uri = $"{server.TrimEnd('/')}/Log/Create";

            var text = await File.ReadAllTextAsync(logger.FileName);

            var submit = new LogSubmit
            {
                Name = name,
                Time = DateTime.Now,
                Text = text
            };

            using var client = new HttpClient();
            var message = await client.PostAsJsonAsync(uri, submit);

            if (message.StatusCode != HttpStatusCode.OK)
            {
                await logger.ErrorAsync($"Fail to upload to {uri} with status code {(int)message.StatusCode} {message.StatusCode}");
            }
        }
    }
}
