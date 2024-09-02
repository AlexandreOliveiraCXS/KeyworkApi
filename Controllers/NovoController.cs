using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MovieAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NovoController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello, World!");
        }
        [HttpPost]
        public IActionResult Post()
        {
            return Ok("Hello, World!");
        }
    }
}