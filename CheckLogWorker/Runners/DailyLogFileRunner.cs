using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CheckLogWorker.Runners
{
    public abstract class DailyLogFileRunner : DailyFileRunner
    {
        public string Identifier { get; set; }

        protected override async Task RunAsync(string filePath, Logger logger)
        {
            if (!File.Exists(filePath))
            {
                await logger.InfoAsync($"Log file does not exist.");
                return;
            }

            var lines = NextLinesAsync(filePath);
            await RunAsync(filePath, lines, logger);
        }
        protected abstract Task RunAsync(string filePath, IAsyncEnumerable<string> lines, Logger logger);

        public override int GetHashCode()
        {
            return (FilePattern?.GetHashCode() ?? 0)
                ^ (Identifier?.GetHashCode() ?? 0);
        }

        protected IAsyncEnumerable<string> NextLinesAsync(string filePath)
        {
            var hash = GetHashCode();

            return NextLinesAsync(filePath, hash);
        }
        private static async IAsyncEnumerable<string> NextLinesAsync(string filePath, int hash)
        {
            const string positionsFile = nameof(DailyLogFileRunner) + ".Positions.json";
            
            var fromPositionsJson = await File.ReadAllTextAsync(positionsFile);
            var fromPositions = JsonSerializer.Deserialize<DailyLogFilePosition[]>(fromPositionsJson);

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

            using var fileStream = File.OpenRead(filePath);
            fileStream.Position = fromPosition.Position;

            var lines = ReadLinesAsync(fileStream);
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

            var toPositionsJson = JsonSerializer.Serialize(toPositions);
            await File.WriteAllTextAsync(positionsFile, toPositionsJson);
        }
        private static async IAsyncEnumerable<string> ReadLinesAsync(Stream stream)
        {
            using var reader = new StreamReader(stream, Encoding.UTF8);

            while (true)
            {
                var startPosition = stream.Position;

                var line = await reader.ReadLineAsync();

                if (reader.EndOfStream)
                {
                    stream.Position -= 1;
                    var lastByte = stream.ReadByte();
                    if (lastByte == '\n')
                    {
                        yield return line;
                    }
                    else
                    {
                        stream.Position = startPosition;
                    }

                    break;
                }
                else
                {
                    yield return line;
                }
            }
        }
    }
}
