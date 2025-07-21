using Microsoft.AspNetCore.Mvc;
using SnippetsAPI.Data;
using SnippetsAPI.Models;

namespace SnippetsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SnippetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SnippetsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetSnippets()
        {
            var snippets = _context.Snippets.ToList();
            return Ok(snippets);
        }
    }
}
