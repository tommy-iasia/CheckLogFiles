using System.Collections.Generic;
using System.Linq;

namespace CheckLogUtility.Logging
{
    public static class LogLineExtensions
    {
        public static LogLevel GetLevel(this IEnumerable<LogLine> lines) => lines.Select(t => t.Level).OrderByDescending(t => t).FirstOrDefault();
    }
}
