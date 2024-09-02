
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
    public class MoviesStreamingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MoviesStreamingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] NameStreamingRequest request)
        {
            var moviesGenres = await (from movie in _context.Movies
                                      join moviesStreamings in _context.MoviesStreamings on movie.Id equals moviesStreamings.IdMovies
                                      join streaming in _context.Streamings on moviesStreamings.IdStreamings equals streaming.Id
                                      where streaming.Name == request.Name
                                      select new
                                      { movie, streaming }).ToListAsync();

            if (moviesGenres == null)
                return NotFound("Movie already exists.");

            var GenreArray = new Dictionary<string, returnStreamings>();

            foreach (var item in moviesGenres)
            {
                var movieReturn = new returnStreamings();

                movieReturn.Id = item.streaming.Id;
                movieReturn.Name = item.streaming.Name;


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
                                      join moviesStreamings in _context.MoviesStreamings on movie.Id equals moviesStreamings.IdMovies
                                      join streaming in _context.Streamings on moviesStreamings.IdStreamings equals streaming.Id
                                      where movie.Title == request.Title
                                      select new
                                      { movie, streaming, }).ToListAsync();

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
                    movieReturn.Streamings.Add(item.streaming.Name);

                    GenreArray[movieReturn.Title] = movieReturn;
                }
                else
                {
                    movieReturn.Streamings.Add(item.streaming.Name);

                    GenreArray.Add(movieReturn.Title, movieReturn);
                }
            }

            return Ok(GenreArray.Values.ToList());
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] MoviesStreamingRequest request)
        {
            if (request == null)
                return BadRequest("MoviesStreaming is null.");

            var movieFind = await _context.Movies
                .Where(m => m.Title.Equals(request.Title))
                .FirstOrDefaultAsync();

            if (movieFind == null)
                return NotFound("Movie Not Found.");

            var streamingFind = await _context.Streamings
                .Where(m => m.Name.Equals(request.Streaming))
                .FirstOrDefaultAsync();

            if (streamingFind == null)
                return NotFound("Streaming Not Found.");


            var newObject = new MoviesStreamings();
            newObject.Id = Guid.NewGuid(); // Gera um novo GUID para o Id
            newObject.IdMovies = movieFind.Id;
            newObject.IdStreamings = streamingFind.Id;
            newObject.CreationTime = DateTime.UtcNow;
            newObject.UpdateTime = DateTime.UtcNow;

            _context.MoviesStreamings.Add(newObject);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Post), new { id = newObject.Id }, newObject);


        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] MoviesStreamingRequest request)
        {

            var movieFind = await _context.Movies
                .Where(m => m.Title.Equals(request.Title))
                .FirstOrDefaultAsync();

            if (movieFind == null)
                return NotFound("Movie not found.");

            var streamingFind = await _context.Streamings
                .Where(m => m.Name.Equals(request.Streaming))
                .FirstOrDefaultAsync();

            if (streamingFind == null)
                return NotFound("Streaming not found.");

            var objDelete = await _context.MoviesStreamings
                .Where(ms => ms.IdMovies == movieFind.Id && ms.IdStreamings == streamingFind.Id)
                .FirstOrDefaultAsync();

            if (objDelete == null)
                return NotFound("Movie-Streaming relationship not found.");

            _context.MoviesStreamings.Remove(objDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        public class NameMovieRequest
        {
            public string Title { get; set; }
        }

        public class NameStreamingRequest
        {
            public string Name { get; set; }
        }

        public class MoviesStreamingRequest
        {
            public string Title { get; set; }
            public string Streaming { get; set; }
        }

        private class returnMovie : Movie
        {
            public List<string> Streamings { get; set; }
            public double ManyStreamings
            {
                get
                {
                    return Streamings.Count();
                }
            }


            public returnMovie()
            {
                Streamings = new List<string>();
            }
        }

        private class returnStreamings : Streaming
        {
            public List<string> Movies { get; set; }
            public returnStreamings()
            {
                Movies = new List<string>();
            }
        }
    }
}
