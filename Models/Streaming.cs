using System;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Models
{
    public class Streaming
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        public DateTime CreationTime { get; set; } = DateTime.UtcNow;

        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
    }
}
