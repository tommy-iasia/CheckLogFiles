using CheckLogUtility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public abstract class DailyLogFileRunner : DailyFileRunner
    {
        public string Identifier { get; set; }
        public string Runner { get; set; }

        protected override async Task RunAsync(string filePath, Logger logger, CancellationToken cancellationToken)
        {
            if (!File.Exists(filePath))
            {
                await logger.InfoAsync($"Log file does not exist.");
                return;
            }

            var lines = NextLinesAsync(filePath, logger);

            await RunAsync(filePath, lines, logger, cancellationToken);
        }
        protected abstract Task RunAsync(string filePath, IAsyncEnumerable<string> lines, Logger logger, CancellationToken cancellationToken);

        protected IAsyncEnumerable<string> NextLinesAsync(string filePath, Logger logger)
        {
            var hash = GetFileHash();

            return NextLinesAsync(filePath, hash, logger);
        }
        protected virtual string GetFileHash() => $"{FilePattern}, {DateTime.Today:yyMMdd}, {Identifier}, {Runner}";
        private static async IAsyncEnumerable<string> NextLinesAsync(string filePath, string hash, Logger logger)
        {
            const string positionsFile = nameof(DailyLogFileRunner) + ".Positions.json";
            
            DailyLogFilePosition[] fromPositions;
            if (File.Exists(positionsFile))
            {
                var fromPositionsJson = await File.ReadAllTextAsync(positionsFile);
                fromPositions = JsonSerializer.Deserialize<DailyLogFilePosition[]>(fromPositionsJson);
            }
            else
            {
                fromPositions = Array.Empty<DailyLogFilePosition>();
            }

            var fromPosition = fromPositions.FirstOrDefault(t => t.Path == filePath && t.Hash == hash);
            if (fromPosition == null)
            {
                fromPosition = new DailyLogFilePosition
                {
                    Path = filePath,
                    Hash = hash,
                    Position = 0,
                    Time = DateTime.MinValue
                };
            }

            using var fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            var positionLength = fileStream.Length - fromPosition.Position;
            const long maxLength = 50 * 1024 * 1024;
            if (positionLength <= maxLength)
            {
                var startPosition = Math.Min(Math.Max(fromPosition.Position, 0), fileStream.Length - 1);
                await logger.InfoAsync($"Start at {startPosition:#,##0}B within file of {fileStream.Length:#,##0}B");

                fileStream.Position = startPosition;
            }
            else
            {
                var lengthPosition = fileStream.Length - maxLength;
                await logger.InfoAsync($"Start at {lengthPosition:#,##0}B instead of {fromPosition.Position:#,##0}B as file of {fileStream.Length:#,##0}B is too large");

                fileStream.Position = fileStream.Length - maxLength;
            }

            var lines = ReadLinesAsync(fileStream, capacity: 1_000_000, logger);
            await foreach (var line in lines)
            {
                yield return line;
            }

            fromPosition.Position = fileStream.Position;
            fromPosition.Time = DateTime.Now;

            var toPositions = new[] { fromPosition }
                .Concat(fromPositions.Except(new[] { fromPosition }))
                .Take(500)
                .ToArray();

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var toPositionsJson = JsonSerializer.Serialize(toPositions, jsonOptions);

            await File.WriteAllTextAsync(positionsFile, toPositionsJson);
        }
        protected static async IAsyncEnumerable<string> ReadLinesAsync(Stream stream, int capacity, Logger logger)
        {
            var reader = new StreamReader(stream, Encoding.UTF8);

            var index = 0;
            while (index++ < capacity)
            {
                var startPosition = stream.Position;

                var line = await reader.ReadLineAsync();

                if (reader.EndOfStream)
                {
                    stream.Position -= 1;
                    var lastByte = stream.ReadByte();
                    if (lastByte == '\n')
                    {
                        if (line != null)
                        {
                            yield return line;
                        }
                    }
                    else
                    {
                        stream.Position = startPosition;
                    }

                    break;
                }
                else
                {
                    if (line != null)
                    {
                        yield return line;
                    }
                }
            }

            if (index >= capacity)
            {
                await logger.InfoAsync($"Read {index - 1:#,##0} lines only as file is too large");
            }
        }
    }
}
