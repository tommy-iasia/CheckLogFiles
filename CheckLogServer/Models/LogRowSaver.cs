namespace CheckLogServer.Models
{
    public class LogRowSaver : ArraySaver<LogRow>
    {
        public LogRowSaver() : base(@"Data\Logs.json") { }
    }
}
