using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CheckLogUtility.Logging
{
    public class Logger
    {
        public IEnumerable<LogLine> Lines => lines;
        private readonly List<LogLine> lines = new();

        public string FileName { get; private set; }
        public async Task SetFileAsync(string fileName)
        {
            FileName = fileName;

            if (lines.Any())
            {
                var jsons = lines.Select(t => $"{JsonSerializer.Serialize(t)},\r\n");
                var text = string.Join(string.Empty, jsons);
                await File.AppendAllTextAsync(fileName, text);
            }
        }

        private async Task AddAsync(LogLevel level, string text)
        {
            var line = new LogLine
            {
                Time = DateTime.Now,
                Level = level,
                Text = text
            };
            lines.Add(line);
            
            Console.WriteLine(line);

            if (FileName != null)
            {
                var json = JsonSerializer.Serialize(line);
                await File.AppendAllTextAsync(FileName, $"{json},\r\n");
            }
        }

        public async Task InfoAsync(string line) => await AddAsync(LogLevel.Info, line);

        public async Task WarnAsync(string line) => await AddAsync(LogLevel.Warn, line);

        public async Task ErrorAsync(Exception exception) => await ErrorAsync(exception.ToString());
        public async Task ErrorAsync(string line) => await AddAsync(LogLevel.Error, line);

        public static LogLine[] Read(string text)
        {
            var json = "[" + text.TrimEnd(',', '\r', '\n') + "]";

            return JsonSerializer.Deserialize<LogLine[]>(json);
        }
    }
}
