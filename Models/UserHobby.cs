using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HobbyHub.Models
{
    public class UserHobby
    {
        [Key]
        public int UserHobbyId { get; set; }
        public string Proficiency { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        [ForeignKey("Enthusiast")]
        public int EnthusiastId { get; set; }
        public User Enthusiast { get; set; }

        [ForeignKey("Hobby")]
        public int HobbyId { get; set; }
        public Hobby Hobby { get; set; }
    }
}