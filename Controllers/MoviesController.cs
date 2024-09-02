
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
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MoviesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var movies = await _context.Movies.ToListAsync();

            var totalItems = movies.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var paginationMetadata = new
            {
                totalItems,
                pageSize,
                currentPage = pageNumber,
                totalPages,
                hasNextPage = pageNumber < totalPages,
                hasPreviousPage = pageNumber > 1
            };

            Response.Headers.Add("X-Pagination", Newtonsoft.Json.JsonConvert.SerializeObject(paginationMetadata));

            return Ok(movies);
        }

        [HttpGet]
        [Route("title")]
        public async Task<IActionResult> Get([FromQuery] TitleMovieRequest request)
        {

            //var movieFind = await _context.Movies
            //   .Where(m => m.Title.Equals(request.Title))
            //   .Join(_context.MoviesGenre,
            //         movie => movie.Id,
            //         movieGenre => movieGenre.IdMovies,
            //         (movie, movieGenre) => new { movie, movieGenre })
            //   .Join(_context.Genres,
            //         beforeJoin => beforeJoin.movieGenre.IdGenre,
            //         genre => genre.Id,
            //         (beforeJoin, genre) => new { beforeJoin.movie, genre }
            //         ).FirstOrDefaultAsync();


            var moviesGenres = await (from movie in _context.Movies
                                      join movieGenre in _context.MoviesGenre on movie.Id equals movieGenre.IdMovies
                                      join genre in _context.Genres on movieGenre.IdGenre equals genre.Id
                                      join moviesStreamings in _context.MoviesStreamings on movie.Id equals moviesStreamings.IdMovies
                                      join streaming in _context.Streamings on moviesStreamings.IdStreamings equals streaming.Id
                                      where movie.Title == request.Title
                                      select new
                                      { movie, genre, streaming }).ToListAsync();

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

                    if (!movieReturn.Genres.Contains(item.genre.Name))
                        movieReturn.Genres.Add(item.genre.Name);
                    if (!movieReturn.Streamings.Contains(item.streaming.Name))
                        movieReturn.Streamings.Add(item.streaming.Name);

                    GenreArray[movieReturn.Title] = movieReturn;
                }
                else
                {
                    movieReturn.Genres.Add(item.genre.Name);
                    movieReturn.Streamings.Add(item.streaming.Name);

                    GenreArray.Add(movieReturn.Title, movieReturn);
                }
            }

            return Ok(GenreArray.Values.ToList());
        }

        [HttpGet]
        [Route("year")]
        public async Task<IActionResult> Get([FromQuery] YearMovieRequest request)
        {
            var moviesGenres = await (from movie in _context.Movies
                                      join movieGenre in _context.MoviesGenre on movie.Id equals movieGenre.IdMovies
                                      join genre in _context.Genres on movieGenre.IdGenre equals genre.Id
                                      where movie.Year == request.Year
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

            return Ok(new { countMovieFound = GenreArray.Count(), movies = GenreArray.Values.ToList() });
        }

        [HttpGet]
        [Route("month")]
        public async Task<IActionResult> Get([FromQuery] MonthMovieRequest request)
        {
            var moviesGenres = await (from movie in _context.Movies
                                      join movieGenre in _context.MoviesGenre on movie.Id equals movieGenre.IdMovies
                                      join genre in _context.Genres on movieGenre.IdGenre equals genre.Id
                                      where movie.Month == request.Month
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

        private class returnMovie : Movie
        {
            public List<string> Genres { get; set; }
            public returnMovie()
            {
                Genres = new List<string>();
                Streamings = new List<string>();
            }

            public List<string> Streamings { get; set; }

        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Movie newMovie)
        {
            if (newMovie == null)
                return BadRequest("Movie is null.");

            if (!ValidateMovie(newMovie))
                return BadRequest("Movie not avalible.");

            newMovie.Id = Guid.NewGuid(); // Gera um novo GUID para o Id
            newMovie.CreationTime = DateTime.UtcNow;
            newMovie.UpdateTime = DateTime.UtcNow;

            _context.Movies.Add(newMovie);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Post), new { id = newMovie.Id }, newMovie);
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] TitleMovieRequest request)
        {

            var movie = await _context.Movies
                .Where(m => m.Title.Equals(request.Title))
                .FirstOrDefaultAsync();

            if (movie == null)
            {
                return NotFound("Movie not found.");
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
        [Route("update")]
        public async Task<IActionResult> Update([FromBody] Movie movieRequest)
        {
            if (movieRequest == null)
                return BadRequest("Movie is null.");

            if (!ValidateMovie(movieRequest))
                return BadRequest("Movie not avalible.");

            var movie = await _context.Movies
                .Where(m => m.Title.Equals(movieRequest.Title))
                .FirstOrDefaultAsync();

            if (movie == null)
                return NotFound("Movie not found.");

            movie.Month = movieRequest.Month;
            movie.Year = movieRequest.Year;

            await _context.SaveChangesAsync();

            return Ok(movie);
        }

        public bool ValidateMovie(Movie movieRequest)
        {

            if (movieRequest.Month < 0 || movieRequest.Month > 12)
            {
                return false;
            }

            if (movieRequest.Year < 1900 || movieRequest.Year > DateTime.Now.Year)
            {
                return false;
            }

            return true;
        }

        public class TitleMovieRequest
        {
            public string Title { get; set; }
        }

        public class YearMovieRequest
        {
            public int Year { get; set; }
        }

        public class MonthMovieRequest
        {
            public int Month { get; set; }
        }
    }
}
