namespace CheckLogServer.Models
{
    public class Email
    {
        public int Id { get; set; }

        public string From { get; set; }
        public string To { get; set; }

        public string SubjectTag { get; set; }
        public string ServerAddress { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }

        public string Host { get; set; }
        public int Port { get; set; }
        public bool Ssl { get; set; }
    }
}
