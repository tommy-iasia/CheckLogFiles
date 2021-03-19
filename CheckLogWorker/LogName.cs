using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CheckLogWorker
{
    public class LogName
    {
        public string Identifier { get; set; }
        public string Runner { get; set; }

        public DateTime Time { get; set; }
        public int Random { get; set; }

        public static LogName Parse(string text)
        {
            var match = Regex.Match(text, @"-(?<runner>[^-])-(?<time>\d+)-(?<random>\d+)$");
            if (!match.Success)
            {
                throw new ArgumentException(null, nameof(text));
            }

            var identifier = text.Substring(0, text.Length - match.Length);
            var runner = match.Groups["runner"].Value;

            var timeText = match.Groups["time"].Value;
            var timeValue = DateTime.ParseExact(timeText, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

            var randomText = match.Groups["random"].Value;
            var randomValue = int.Parse(randomText);

            return new LogName
            {
                Identifier = identifier,
                Runner = runner,
                Time = timeValue,
                Random = randomValue
            };
        }
        public override string ToString()
        {
            return $"{Identifier}-{Runner}-{Time:yyyyMMddHHmmss}-{Random:0000}";
        }
    }
}
