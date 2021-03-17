using System;
using System.ComponentModel.DataAnnotations;

namespace CheckLogWorker
{
    public class LogSubmit
    {
        [Required]
        public LogName Name { get; set; }

        [Required]
        public DateTime Time { get; set; }

        [Required]
        public string Text { get; set; }
    }
}
