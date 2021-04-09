using Microsoft.EntityFrameworkCore;
using System;

namespace CheckLogServer.Models
{
    [Index(nameof(Identifier), IsUnique = true)]
    public class Node
    {
        public int Id { get; set; }
        public string Identifier { get; set; }

        public DateTime LogTime { get; set; }

        public LogRow LevelLog { get; set; }
        public DateTime LevelTime { get; set; }

        public bool Disabled { get; set; }
    }
}
