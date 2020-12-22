using System.ComponentModel.DataAnnotations;

namespace HobbyHub.Models
{
    public class LoginCredentials
    {
        [Required]
        public string LoginUsername { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}