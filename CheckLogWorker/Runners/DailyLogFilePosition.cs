using System;

namespace CheckLogWorker.Runners
{
    public class DailyLogFilePosition
    {
        public string Path { get; set; }
        public string Hash { get; set; }

        public long Position { get; set; }
        public DateTime Time { get; set; }
    }
}
