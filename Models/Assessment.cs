using System;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Models
{
    public class Assessment
    {
        [Key]
        public Guid Id { get; set; }

        public Guid IdUser { get; set; }

        public Guid IdMovies { get; set; }

        [Required]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }

        public DateTime CreationTime { get; set; } = DateTime.UtcNow;

        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    }
}
