using System;

namespace CheckLogRunner
{
    public record Slot
    {
        public Configure Configure { get; set; }
        
        public TimeExpression TimeExpression { get; set; }

        public DateTime NextTime { get; set; }
    }
}
