
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
    public class StreamingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StreamingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var Streamings = await _context.Streamings.ToListAsync();

            return Ok(Streamings);
        }

        [HttpGet]
        [Route("one")]
        public async Task<IActionResult> Get([FromQuery] NameStreamingsRequest request)
        {
            var streamingsFind = await _context.Streamings
               .Where(m => m.Name.Equals(request.Name))
               .FirstOrDefaultAsync();

            if (streamingsFind == null)
                return NotFound("Streaming already exists.");

            return Ok(streamingsFind);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Streaming newStreamings)
        {
            if (newStreamings == null)
                return BadRequest("Streamings is null.");

            var userFind = await _context.Users
                .Where(m => m.Name.Equals(newStreamings.Name))
                .FirstOrDefaultAsync();

            if (userFind != null)
                return NotFound("Streamings already exists.");

            newStreamings.Id = Guid.NewGuid(); // Gera um novo GUID para o Id
            newStreamings.CreationTime = DateTime.UtcNow;
            newStreamings.UpdateTime = DateTime.UtcNow;

            _context.Streamings.Add(newStreamings);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Post), new { id = newStreamings.Id }, newStreamings);
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] NameStreamingsRequest request)
        {

            var userFind = await _context.Users
                .Where(m => m.Name.Equals(request.Name))
                .FirstOrDefaultAsync();

            if (userFind == null)
                return NotFound("Streamings not found.");

            _context.Users.Remove(userFind);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        public class NameStreamingsRequest
        {
            public string Name { get; set; }
        }
    }
}
