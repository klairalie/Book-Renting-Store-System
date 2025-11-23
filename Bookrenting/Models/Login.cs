using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookRenting.Models
{
    [Table("login")] 
    public class Login
    {
        [Key]
        public int login_id { get; set; } // primary key for login table

        public int registered_id { get; set; } // FK from register_users

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        public string UserName { get; set; } = "";

        [Required]
        public string Password { get; set; } = "";
    }
}
