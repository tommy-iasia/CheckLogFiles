using System;

namespace CheckLogServer.Models
{
    public class Update
    {
        public int Id { get; set; }

        public string Version { get; set; }
        public UpdateStatus Status { get; set; }

        public string Command { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime LastTime { get; set; }

        public string Remark { get; set; }
    }
}
