using System;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Models
{
    public class MoviesGenre
    {
        [Key]
        public Guid Id { get; set; }

        public Guid IdGenre { get; set; }

        public Guid IdMovies { get; set; }

        public DateTime CreationTime { get; set; } = DateTime.UtcNow;

        public DateTime UpdateTime { get; set; } = DateTime.UtcNow;

    }
}
