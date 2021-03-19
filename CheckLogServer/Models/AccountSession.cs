using Microsoft.EntityFrameworkCore;
using System;

namespace CheckLogServer.Models
{
    [Index(nameof(Code), IsUnique = true)]
    public class AccountSession
    {
        public int Id { get; set; }
        public Account Account { get; set; }

        public string Code { get; set; }
        public DateTime StartTime { get; set; }
    }
}
