using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace CheckLogWorker
{
    public class Logger
    {
        public Logger(string fileName) => FileName = fileName;
        public readonly string FileName;

        public IEnumerable<LogLine> Lines => lines;
        private readonly List<LogLine> lines = new();

        private async Task WriteAsync(LogLevel level, string text)
        {
            var line = new LogLine
            {
                Time = DateTime.Now,
                Level = level,
                Text = text
            };
            lines.Add(line);
            
            Console.WriteLine(line);

            var json = JsonSerializer.Serialize(line);
            await File.AppendAllTextAsync(FileName, $"{json},\r\n");
        }

        public async Task InfoAsync(string line) => await WriteAsync(LogLevel.Info, line);

        public async Task WarnAsync(string line) => await WriteAsync(LogLevel.Warn, line);

        public async Task ErrorAsync(Exception exception) => await ErrorAsync(exception.ToString());
        public async Task ErrorAsync(string line) => await WriteAsync(LogLevel.Error, line);

        public static LogLine[] Read(string text)
        {
            var json = "[" + text.TrimEnd(',', '\r', '\n') + "]";

            return JsonSerializer.Deserialize<LogLine[]>(json);
        }
    }
}
