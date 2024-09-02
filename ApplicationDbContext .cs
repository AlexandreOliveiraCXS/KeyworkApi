using Microsoft.EntityFrameworkCore;
using MoviesAPI.Models;

namespace MoviesAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Streaming> Streamings { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<MoviesStreamings> MoviesStreamings { get; set; }
        public DbSet<MoviesGenre> MoviesGenre { get; set; }
        public DbSet<Assessment> Assessment { get; set; }
    }
}
