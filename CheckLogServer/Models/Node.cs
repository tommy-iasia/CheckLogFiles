using Microsoft.EntityFrameworkCore;
using System;

namespace CheckLogServer.Models
{
    [Index(nameof(Identitifer), IsUnique = true)]
    public class Node
    {
        public int Id { get; set; }
        public string Identitifer { get; set; }

        public DateTime LogTime { get; set; }

        public LogRow LevelLog { get; set; }
        public DateTime LevelTime { get; set; }

        public bool Disabled { get; set; }
    }
}
