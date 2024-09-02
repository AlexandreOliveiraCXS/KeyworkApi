
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
    public class GenreController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GenreController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var Genre = await _context.Genres.ToListAsync();

            return Ok(Genre);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Genre newGenre)
        {
            if (newGenre == null)
                return BadRequest("Genre is null.");

            var userFind = await _context.Genres
                .Where(m => m.Name.Equals(newGenre.Name))
                .FirstOrDefaultAsync();

            if (userFind != null)
                return NotFound("Genre already exists.");

            newGenre.Id = Guid.NewGuid(); // Gera um novo GUID para o Id
            newGenre.CreationTime = DateTime.UtcNow;
            newGenre.UpdateTime = DateTime.UtcNow;

            _context.Genres.Add(newGenre);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Post), new { id = newGenre.Id }, newGenre);
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] NameGenreRequest request)
        {

            var GenreFind = await _context.Genres
                .Where(m => m.Name.Equals(request.Name))
                .FirstOrDefaultAsync();

            if (GenreFind == null)
                return NotFound("Genre not found.");

            var moviesGenre = await _context.MoviesGenre
           .Where(ms => ms.IdGenre == GenreFind.Id)
           .ToListAsync();

            _context.MoviesGenre.RemoveRange(moviesGenre);

            _context.Genres.Remove(GenreFind);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        public class NameGenreRequest
        {
            public string Name { get; set; }
        }
    }
}
