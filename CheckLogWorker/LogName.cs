using System;

namespace CheckLogWorker
{
    public record LogName
    {
        public string Identifier { get; set; }
        public string Runner { get; set; }

        public DateTime Time { get; set; }
        public int Random { get; set; }

        public override string ToString()
        {
            return $"{Identifier}-{Runner}-{Time:yyyyMMddHHmmss}-{Random:0000}";
        }
    }
}
