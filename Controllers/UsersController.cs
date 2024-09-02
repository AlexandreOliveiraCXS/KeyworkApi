
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
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var users = await _context.Users.Skip((pageNumber - 1) * pageSize)
                                          .Take(pageSize)
                                          .ToListAsync();

            var totalItems = users.Count();
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

            return Ok(users);
        }

        [HttpGet]
        [Route("one")]
        public async Task<IActionResult> Get([FromQuery] NameUserRequest request)
        {
            var userFind = await _context.Users
               .Where(m => m.Name.Equals(request.Name))
               .FirstOrDefaultAsync();

            if (userFind == null)
                return NotFound("User already exists.");

            return Ok(userFind);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] User newUser)
        {
            if (newUser == null)
                return BadRequest("User is null.");

            var userFind = await _context.Users
                .Where(m => m.Name.Equals(newUser.Name))
                .FirstOrDefaultAsync();

            if (userFind != null)
                return NotFound("User already exists.");

            newUser.Id = Guid.NewGuid(); // Gera um novo GUID para o Id
            newUser.CreationTime = DateTime.UtcNow;
            newUser.UpdateTime = DateTime.UtcNow;

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Post), new { id = newUser.Id }, newUser);
        }

        [HttpPost]
        [Route("delete")]
        public async Task<IActionResult> Delete([FromBody] NameUserRequest request)
        {

            var userFind = await _context.Users
                .Where(m => m.Name.Equals(request.Name))
                .FirstOrDefaultAsync();

            if (userFind == null)
                return NotFound("User not found.");

            _context.Users.Remove(userFind);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        public class NameUserRequest
        {
            public string Name { get; set; }
        }
    }
}
