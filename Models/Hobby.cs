using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HobbyHub.Models
{
    public class Hobby
    {
        [Key]
        public int HobbyId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property for UserHobby set
        public List<UserHobby> Enthusiasts { get; set; }
    }

}