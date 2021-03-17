using System.ComponentModel.DataAnnotations;

namespace CheckLogServer.Models
{
    public class Account
    {
        public string Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        public string[] Sessions { get; set; }
    }
}
