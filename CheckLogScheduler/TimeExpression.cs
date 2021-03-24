using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace CheckLogScheduler
{
    public class TimeExpression
    {
        public int[] Hours { get; set; }
        public int[] Minutes { get; set; }
        public int[] Seconds { get; set; }
        public DateTime Next(DateTime last)
        {
            if (Hours.Contains(last.Hour))
            {
                if (Minutes.Contains(last.Minute))
                {
                    using var nextSeconds = Seconds.Where(t => t > last.Second).GetEnumerator();
                    if (nextSeconds.MoveNext())
                    {
                        return new DateTime(
                            last.Year, last.Month, last.Day, last.Hour, last.Minute,
                            nextSeconds.Current);
                    }
                }

                using var nextMinutes = Minutes.Where(t => t > last.Minute).GetEnumerator();
                if (nextMinutes.MoveNext())
                {
                    return new DateTime(
                        last.Year, last.Month, last.Day, last.Hour,
                        nextMinutes.Current,
                        Seconds.First());
                }
            }

            using var nextHour = Hours.Where(t => t > last.Hour).GetEnumerator();
            if (nextHour.MoveNext())
            {
                return new DateTime(
                    last.Year, last.Month, last.Day,
                    nextHour.Current,
                    Minutes.First(),
                    Seconds.First());
            }

            var nextDay = last.AddDays(1);
            return new DateTime(
                nextDay.Year, nextDay.Month, nextDay.Day,
                    Hours.First(),
                    Minutes.First(),
                    Seconds.First());
        }

        public static TimeExpression Parse(string text)
        {
            var match = Regex.Match(text, @"^\s*(?<hour>[^:]+)\s*\:\s*(?<minute>[^:]+)\s*\:\s*(?<second>[^:]+)$");
            if (!match.Success)
            {
                throw new ArgumentException(null, nameof(text));
            }

            var hourText = match.Groups["hour"].Value;
            var hours = GetValues(hourText, 24);

            var minuteText = match.Groups["minute"].Value;
            var minutes = GetValues(minuteText, 60);

            var secondText = match.Groups["second"].Value;
            var seconds = GetValues(secondText, 60);

            return new TimeExpression
            {
                Hours = hours,
                Minutes = minutes,
                Seconds = seconds
            };
        }
        private static int[] GetValues(string pattern, int range)
        {
            var from = Enumerable.Range(0, range);
            
            var to = Filter(pattern, from)
                .Where(t => t >= 0 && t < range)
                .OrderBy(t => t)
                .ToArray();

            if (!to.Any())
            {
                throw new ArgumentException(null, nameof(pattern));
            }

            return to;
        }
        private static IEnumerable<int> Filter(string text, IEnumerable<int> input)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return input;
            }
            else
            {
                var parts = Regex.Split(text, @"\s*,\s*");
                if (parts.Length > 1)
                {
                    return parts.SelectMany(t => Filter(t, input)).Distinct();
                }
                else
                {
                    var match = Regex.Match(text, @"^\s*([x*?]*|(?<range>\s*(?<from>\d+)?\s*-\s*(?<to>\d+)?)|(?<number>\s*(?<value>\d+)))?(?<modulus>\s*\%\s*(?<operand>\d+)\s*(\+\s*(?<addend>\d+))?)?\s*$");
                    if (!match.Success)
                    {
                        throw new ArgumentException(null, nameof(text));
                    }

                    var output = input;

                    if (match.Groups["number"].Success)
                    {
                        var value = int.Parse(match.Groups["value"].Value);
                        output = output.Where(t => t == value);
                    }

                    if (match.Groups["modulus"].Success)
                    {
                        var operand = int.Parse(match.Groups["operand"].Value);
                        
                        var addendGroup = match.Groups["addend"];
                        if (addendGroup.Success)
                        {
                            var addend = int.Parse(addendGroup.Value);
                            output = output.Where(t => (t - addend) % operand == 0);
                        }
                        else
                        {
                            output = output.Where(t => t % operand == 0);
                        }
                    }

                    if (match.Groups["range"].Success)
                    {
                        var fromGroup = match.Groups["from"];
                        if (fromGroup.Success)
                        {
                            var from = int.Parse(fromGroup.Value);
                            output = output.Where(t => t >= from);
                        }

                        var toGroup = match.Groups["to"];
                        if (toGroup.Success)
                        {
                            var to = int.Parse(toGroup.Value);
                            output = output.Where(t => t <= to);
                        }
                    }

                    return output;
                }
            }
        }
    }
}
