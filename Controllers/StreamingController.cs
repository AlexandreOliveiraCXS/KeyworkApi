
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
    public class StreamingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StreamingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var Genre = await _context.Streamings.ToListAsync();

            return Ok(Genre);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Streaming request)
        {
            if (request == null)
                return BadRequest("Streaming is null.");

            var userFind = await _context.Streamings
                .Where(m => m.Name.Equals(request.Name))
                .FirstOrDefaultAsync();

            if (userFind != null)
                return NotFound("Streaming already exists.");

            request.Id = Guid.NewGuid(); // Gera um novo GUID para o Id
            request.CreationTime = DateTime.UtcNow;
            request.UpdateTime = DateTime.UtcNow;

            _context.Streamings.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Post), new { id = request.Id }, request);
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] NameStreamingRequest request)
        {

            var StreamingFind = await _context.Streamings
                .Where(m => m.Name.Equals(request.Name))
                .FirstOrDefaultAsync();

            if (StreamingFind == null)
                return NotFound("Streaming not found.");

            var moviesStreamings = await _context.MoviesStreamings
            .Where(ms => ms.IdStreamings == StreamingFind.Id)
            .ToListAsync();

            _context.MoviesStreamings.RemoveRange(moviesStreamings);

            _context.Streamings.Remove(StreamingFind);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        public class NameStreamingRequest
        {
            public string Name { get; set; }
        }
    }
}
