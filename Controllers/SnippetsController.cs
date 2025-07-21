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

        [HttpGet("{id}")]
        public IActionResult GetSnippet(int id)
        {
            var snippet = _context.Snippets.Find(id);
            if (snippet == null)
            {
                return NotFound();
            }
            return Ok(snippet);
        }

        [HttpPost]
        public IActionResult CreateSnippet(Snippet snippet)
        {
            _context.Snippets.Add(snippet);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetSnippet), new { id = snippet.Id }, snippet);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSnippet(int id)
        {
            var snippet = _context.Snippets.Find(id);
            if (snippet == null)
            {
                return NotFound();
            }

            _context.Snippets.Remove(snippet);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
