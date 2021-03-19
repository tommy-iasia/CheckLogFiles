using System;

namespace CheckLogWorker.Runners
{
    public record NetOverflowRecord
    {
        public string Client { get; set; }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        
        public int Count { get; set; }
    }
}
