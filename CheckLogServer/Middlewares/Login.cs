using CheckLogServer.Models;

namespace CheckLogServer.Middlewares
{
    public class Login
    {
        public string Session { get; set; }

        public bool Success => Account != null;
        public Account Account { get; set; }
    }
}
