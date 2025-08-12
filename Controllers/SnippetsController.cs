using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnippetsAPI.Data;
using SnippetsAPI.Models;
using SnippetsAPI.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SnippetsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // This protects ALL endpoints in this controller
    public class SnippetsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IEncryptionService _encryptionService;

        public SnippetsController(AppDbContext context, IEncryptionService encryptionService)
        {
            _context = context;
            _encryptionService = encryptionService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        // DTO for snippet responses
        public class SnippetDto
        {
            public int Id { get; set; }
            public string Language { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        [HttpGet]
        public IActionResult GetSnippets()
        {
            var currentUserId = GetCurrentUserId();

            var snippets = _context.Snippets
                .Where(s => s.UserId == currentUserId)
                .ToList();

            var snippetDtos = snippets.Select(snippet => new SnippetDto
            {
                Id = snippet.Id,
                Language = snippet.Language,
                Code = !string.IsNullOrEmpty(snippet.EncryptedCode) ? _encryptionService.Decrypt(snippet.EncryptedCode) : string.Empty,
                CreatedAt = snippet.CreatedAt,
                UpdatedAt = snippet.UpdatedAt
            }).ToList();

            return Ok(snippetDtos);
        }

        [HttpGet("{id}")]
        public IActionResult GetSnippet(int id)
        {
            var currentUserId = GetCurrentUserId();

            var snippet = _context.Snippets
                .FirstOrDefault(s => s.Id == id && s.UserId == currentUserId);

            if (snippet == null)
                return NotFound();

            var snippetDto = new SnippetDto
            {
                Id = snippet.Id,
                Language = snippet.Language,
                Code = !string.IsNullOrEmpty(snippet.EncryptedCode) ? _encryptionService.Decrypt(snippet.EncryptedCode) : string.Empty,
                CreatedAt = snippet.CreatedAt,
                UpdatedAt = snippet.UpdatedAt
            };

            return Ok(snippetDto);
        }

       [HttpPost]
public IActionResult CreateSnippet([FromBody] Snippet snippet)
{
    if (snippet == null || string.IsNullOrWhiteSpace(snippet.Language) || string.IsNullOrWhiteSpace(snippet.Code))
    {
        // Return a clear error if required fields are missing
        return BadRequest(new { error = "Both 'language' and 'code' are required." });
    }

    var currentUserId = GetCurrentUserId();

    // Set the UserId to the current authenticated user
    snippet.UserId = currentUserId;

    // Encrypt code before saving
    snippet.EncryptedCode = _encryptionService.Encrypt(snippet.Code);
    snippet.CreatedAt = DateTime.UtcNow;
    snippet.UpdatedAt = DateTime.UtcNow;

    _context.Snippets.Add(snippet);
    _context.SaveChanges();

    // Return with decrypted code
    var snippetDto = new SnippetDto
    {
        Id = snippet.Id,
        Language = snippet.Language,
        Code = snippet.Code,
        CreatedAt = snippet.CreatedAt,
        UpdatedAt = snippet.UpdatedAt
    };

    return CreatedAtAction(nameof(GetSnippet), new { id = snippet.Id }, snippetDto);
}

        [HttpDelete("{id}")]
        public IActionResult DeleteSnippet(int id)
        {
            var currentUserId = GetCurrentUserId();

            var snippet = _context.Snippets
                .FirstOrDefault(s => s.Id == id && s.UserId == currentUserId); // Ensure user owns the snippet

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