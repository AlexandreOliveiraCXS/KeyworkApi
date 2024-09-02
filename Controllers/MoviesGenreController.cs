
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoviesAPI.Data;
using MoviesAPI.Models;

namespace MovieAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoviesGenreController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MoviesGenreController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("moviesToGenre")]
        public async Task<IActionResult> Get([FromQuery] NameGenreRequest request)
        {
            var moviesGenres = await (from movie in _context.Movies
                                      join movieGenre in _context.MoviesGenre on movie.Id equals movieGenre.IdMovies
                                      join genre in _context.Genres on movieGenre.IdGenre equals genre.Id
                                      where genre.Name == request.Name
                                      select new
                                      { movie, genre, }).ToListAsync();

            if (moviesGenres == null)
                return NotFound("Movie already exists.");

            var GenreArray = new Dictionary<string, returnGenre>();

            foreach (var item in moviesGenres)
            {
                var movieReturn = new returnGenre();

                movieReturn.Id = item.genre.Id;
                movieReturn.Name = item.genre.Name;


                if (GenreArray.ContainsKey(movieReturn.Name))
                {
                    movieReturn = GenreArray[movieReturn.Name];
                    movieReturn.Movies.Add(item.movie.Title);

                    GenreArray[movieReturn.Name] = movieReturn;
                }
                else
                {
                    movieReturn.Movies.Add(item.movie.Title);

                    GenreArray.Add(movieReturn.Name, movieReturn);
                }
            }

            return Ok(GenreArray.Values.ToList());
        }

        [HttpGet]
        [Route("movie")]
        public async Task<IActionResult> Get([FromQuery] NameMovieRequest request)
        {
            var moviesGenres = await (from movie in _context.Movies
                                      join movieGenre in _context.MoviesGenre on movie.Id equals movieGenre.IdMovies
                                      join genre in _context.Genres on movieGenre.IdGenre equals genre.Id
                                      where movie.Title == request.Title
                                      select new
                                      { movie, genre, }).ToListAsync();

            if (moviesGenres == null)
                return NotFound("Movie already exists.");

            var GenreArray = new Dictionary<string, returnMovie>();

            foreach (var item in moviesGenres)
            {
                var movieReturn = new returnMovie();

                movieReturn.Id = item.movie.Id;
                movieReturn.Title = item.movie.Title;
                movieReturn.Year = item.movie.Year;
                movieReturn.Month = item.movie.Month;

                if (GenreArray.ContainsKey(movieReturn.Title))
                {
                    movieReturn = GenreArray[movieReturn.Title];
                    movieReturn.Genres.Add(item.genre.Name);

                    GenreArray[movieReturn.Title] = movieReturn;
                }
                else
                {
                    movieReturn.Genres.Add(item.genre.Name);

                    GenreArray.Add(movieReturn.Title, movieReturn);
                }
            }

            return Ok(GenreArray.Values.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] MoviesGenreRequest request)
        {
            if (request == null)
                return BadRequest("MoviesGenre is null.");

            var movieFind = await _context.Movies
                .Where(m => m.Title.Equals(request.Title))
                .FirstOrDefaultAsync();

            if (movieFind == null)
                return NotFound("Movie Not Found.");

            var genreFind = await _context.Genres
                .Where(m => m.Name.Equals(request.Genre))
                .FirstOrDefaultAsync();

            if (genreFind == null)
                return NotFound("Genre Not Found.");

            var newObject = new MoviesGenre();
            newObject.Id = Guid.NewGuid(); // Gera um novo GUID para o Id
            newObject.IdMovies = movieFind.Id;
            newObject.IdGenre = genreFind.Id;
            newObject.CreationTime = DateTime.UtcNow;
            newObject.UpdateTime = DateTime.UtcNow;

            _context.MoviesGenre.Add(newObject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Post), new { id = newObject.Id }, newObject);


        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] MoviesGenreRequest request)
        {

            var movieFind = await _context.Movies
                .Where(m => m.Title.Equals(request.Title))
                .FirstOrDefaultAsync();

            if (movieFind == null)
                return NotFound("Movie not found.");

            var genreFind = await _context.Genres
                .Where(m => m.Name.Equals(request.Genre))
                .FirstOrDefaultAsync();

            if (genreFind == null)
                return NotFound("Genre not found.");

            var objDelete = await _context.MoviesGenre
                .Where(ms => ms.IdMovies == movieFind.Id && ms.IdGenre == genreFind.Id)
                .FirstOrDefaultAsync();

            if (objDelete == null)
                return NotFound("Movie-Genre relationship not found.");

            _context.MoviesGenre.Remove(objDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }


        public class NameMovieRequest
        {
            public string Title { get; set; }
        }

        public class NameGenreRequest
        {
            public string Name { get; set; }
        }

        public class MoviesGenreRequest
        {
            public string Title { get; set; }
            public string Genre { get; set; }
        }

        private class returnMovie : Movie
        {
            public List<string> Genres { get; set; }
            public returnMovie()
            {
                Genres = new List<string>();
            }
        }

        private class returnGenre : Genre
        {
            public List<string> Movies { get; set; }
            public returnGenre()
            {
                Movies = new List<string>();
            }
        }
    }
}
