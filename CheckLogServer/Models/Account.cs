using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CheckLogServer.Models
{
    public class Account
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public ICollection<AccountSession> Sessions { get; set; }
    }
}
