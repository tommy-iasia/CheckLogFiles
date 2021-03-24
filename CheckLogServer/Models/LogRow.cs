using CheckLogUtility.Logging;
using System;

namespace CheckLogServer.Models
{
    public class LogRow
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public DateTime Time { get; set; }
        public LogLevel Level { get; set; }

        public string FileName { get; set; }
        public bool Deleted { get; set; }

        public string Client { get; set; }
    }
}
