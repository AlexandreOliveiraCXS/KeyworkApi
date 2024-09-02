using System;
using System.ComponentModel.DataAnnotations;

namespace MoviesAPI.Models
{
    public class MoviesStreamings
    {
        [Key]
        public Guid Id { get; set; }
        public Guid IdStreamings { get; set; }

        public Guid IdMovies { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime UpdateTime { get; set; }

    }
}
