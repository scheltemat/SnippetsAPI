using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnippetsAPI.Data;
using SnippetsAPI.Models;
using SnippetsAPI.Services;

namespace SnippetsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SnippetsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEncryptionService _encryptionService;

        public SnippetsController(AppDbContext context, IEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        [HttpGet]
        public IActionResult GetSnippets()
        {
            var snippets = _context.Snippets
                .Include(s => s.User)
                .ToList();

            // Decrypt code for response and handle null checks
            foreach (var snippet in snippets)
            {
                if (!string.IsNullOrEmpty(snippet.EncryptedCode))
                {
                    snippet.Code = _encryptionService.Decrypt(snippet.EncryptedCode);
                }
            }

            return Ok(snippets);
        }

        [HttpGet("{id}")]
        public IActionResult GetSnippet(int id)
        {
            var snippet = _context.Snippets
                .Include(s => s.User)
                .FirstOrDefault(s => s.Id == id);

            if (snippet == null)
                return NotFound();

            // Decrypt code for response
            if (!string.IsNullOrEmpty(snippet.EncryptedCode))
            {
                snippet.Code = _encryptionService.Decrypt(snippet.EncryptedCode);
            }

            return Ok(snippet);
        }

        [HttpPost]
        public IActionResult CreateSnippet(Snippet snippet)
        {
            // Encrypt code before saving
            if (!string.IsNullOrEmpty(snippet.Code))
            {
                snippet.EncryptedCode = _encryptionService.Encrypt(snippet.Code);
            }
            snippet.CreatedAt = DateTime.UtcNow;
            snippet.UpdatedAt = DateTime.UtcNow;

            _context.Snippets.Add(snippet);
            _context.SaveChanges();

            // Return with decrypted code
            if (!string.IsNullOrEmpty(snippet.EncryptedCode))
            {
                snippet.Code = _encryptionService.Decrypt(snippet.EncryptedCode);
            }
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
