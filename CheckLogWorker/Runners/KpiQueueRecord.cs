using System;

namespace CheckLogWorker.Runners
{
    public record KpiQueueRecord
    {
        public DateTime Time { get; set; }

        public string Queue { get; set; }
        public string Action { get; set; }
        
        public long Count { get; set; }
    }
}
