using CheckLogServer.Models;

namespace CheckLogServer.Middlewares
{
    public class Login
    {
        public bool Success => Account != null;
        public Account Account { get; set; }
        public AccountSession Session { get; set; }
    }
}
