using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HobbyHub.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        [Required]
        [MinLength(2)]
        public string FirstName { get; set; }
        [Required]
        [MinLength(2)]
        public string LastName { get; set; }
        [Required]
        [MinLength(3)]
        [MaxLength(15)]
        public string Username { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password { get; set; }
        [NotMapped]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property for UserHobby set
        public List<UserHobby> Hobbies { get; set; } 
    }
}