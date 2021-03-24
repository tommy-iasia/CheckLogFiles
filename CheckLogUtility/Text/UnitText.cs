using System;
using System.Text.RegularExpressions;

namespace CheckLogUtility.Text
{
    public static class UnitText
    {
        public static long ParseSize(string text) => TryParseSize(text, out var size) ? size : throw new ArgumentException(null, nameof(text));
        public static bool TryParseSize(string text, out long size)
        {
            var match = Regex.Match(text, @"(?<value>\d+)?\s*(?<unit>[KkMmGgTt])?");

            if (!match.Success)
            {
                size = default;
                return false;
            }

            var valueGroup = match.Groups["value"];
            var valueValue = valueGroup.Success ? long.Parse(valueGroup.Value) : 1;

            var unitGroup = match.Groups["unit"];
            var unitValue = unitGroup.Success ?
                unitGroup.Value switch
                {
                    "K" => 1024,
                    "k" => 1000,
                    "M" => 1024 * 1024,
                    "m" => 1000 * 1000,
                    "G" => 1024 * 1024 * 1024,
                    "g" => 1000 * 1000 * 1000,
                    "T" => 1024L * 1024 * 1024 * 1024,
                    "t" => 1000L * 1000 * 1000 * 1000,
                    _ => throw new InvalidOperationException()
                } : 1;

            size = valueValue * unitValue;
            return true;
        }

        public static string ToSize(long value)
        {
            return value < 5 * 1024 ? value.ToString()
                : value < 5 * 1024 * 1024 ? $"{value / 1024}k"
                : value < 5L * 1024 * 1024* 1024 ? $"{value / 1024 / 1024}M"
                : value < 5L * 1024 * 1024* 1024 * 1024 ? $"{value / 1024 / 1024 / 1024}G"
                : $"{value / 1024 / 1024 / 1024}T";
        }

        public static long ParseCount(string text) => ParseSize(text);
        public static string ToCount(long value)
        {
            return value < 5_000 ? value.ToString()
                : value < 5_000_000 ? $"{value / 1000}k"
                : value < 5_000_000_000 ? $"{value / 1_000_000}M"
                : value < 5_000_000_000_000 ? $"{value / 1_000_000_000}G"
                : $"{value / 1_000_000_000}T";
        }

        public static TimeSpan ParseSpan(string text) => TryParseSpan(text, out var span) ? span : throw new ArgumentException(null, nameof(text));
        public static bool TryParseSpan(string text, out TimeSpan span)
        {
            var match = Regex.Match(text, @"(?<value>[\d.]+)?\s*(?<unit>millisecond|ms|second|s|minute|min|m|hour|hr|h|day|d)");

            if (!match.Success)
            {
                span = default;
                return false;
            }

            var valueGroup = match.Groups["value"];
            var valueValue = valueGroup.Success ? double.Parse(match.Groups["value"].Value) : 1;

            var unitText = match.Groups["unit"].Value;
            
            var outputValue = unitText switch
            {
                "millisecond" or "ms" => TimeSpan.FromMilliseconds(valueValue),
                "second" or "s" => TimeSpan.FromSeconds(valueValue),
                "minute" or "min" or "m" => TimeSpan.FromMinutes(valueValue),
                "hour" or "hr" or "h" => TimeSpan.FromHours(valueValue),
                "day" or "d" => TimeSpan.FromDays(valueValue),
                _ => (TimeSpan?) null,
            };

            if (outputValue == null)
            {
                span = default;
                return false;
            }

            span = outputValue.Value;
            return true;
        }

        public static string ToSpan(TimeSpan value)
        {
            return value.TotalMilliseconds < 5 * 1000 ? $"{(int)value.TotalMilliseconds}ms"
                : value.TotalSeconds < 5 * 60 ? $"{(int)value.TotalSeconds}s"
                : value.TotalMinutes < 5 * 60 ? $"{(int)value.TotalMinutes}min"
                : value.TotalHours < 5 * 24 ? $"{(int)value.TotalHours}h"
                : $"{value.TotalDays}d";
        }
        public static string ToSpan(long value)
        {
            return value < 5 * 1000 ? $"{value}ms"
                : value < 5 * 60 * 1000 ? $"{value / 1000}s"
                : value < 5 * 60 * 60 * 1000 ? $"{value / 60 / 1000}min"
                : value < 5 * 24 * 60 * 60 * 1000 ? $"{value / 60 / 60 / 1000}h"
                : $"{value / 24 / 60 / 60 / 1000}d";
        }
    }
}
