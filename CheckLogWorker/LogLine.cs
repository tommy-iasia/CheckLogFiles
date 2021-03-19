using System;

namespace CheckLogWorker
{
    public class LogLine
    {
        public DateTime Time { get; set; }
        public LogLevel Level { get; set; }
        public string Text { get; set; }


        public override string ToString() => $"[{Time:yyMMdd HH:mm:ss}][{Level}] {Text}";
    }
}
