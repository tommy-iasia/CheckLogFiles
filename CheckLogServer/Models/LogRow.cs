using CheckLogWorker;
using System;

namespace CheckLogServer.Models
{
    public class LogRow
    {
        public LogName Name { get; set; }

        public DateTime Time { get; set; }
        public LogLevel Level { get; set; }

        public string FileName { get; set; }
        public bool Deleted { get; set; }
    }
}
