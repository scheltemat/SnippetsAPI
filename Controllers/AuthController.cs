using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnippetsAPI.Data;
using SnippetsAPI.Models;
using SnippetsAPI.Services;

namespace SnippetsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPasswordHasherService _passwordHasher;
        private readonly IJwtService _jwtService;

        public AuthController(AppDbContext context, IPasswordHasherService passwordHasher, IJwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] User user)
        {
            if (user == null || string.IsNullOrWhiteSpace(user.Password))
            {
                return BadRequest("Invalid user data.");
            }

            // Check if user already exists
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                return BadRequest("User with this email already exists.");
            }

            user.Password = _passwordHasher.HashPassword(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();

            // Exclude password in the response
            var userResponse = new { user.Id, user.Name, user.Email };
            return CreatedAtAction("GetUser", "Users", new { id = user.Id }, userResponse);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDto loginDto)
        {
            if (loginDto == null || string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest("Invalid login data.");
            }

            var user = _context.Users.SingleOrDefault(u => u.Email == loginDto.Email);
            if (user == null || !_passwordHasher.VerifyPassword(user.Password, loginDto.Password))
            {
                return Unauthorized("Invalid email or password.");
            }

            // Generate JWT token
            var token = _jwtService.GenerateToken(user.Id, user.Email);

            return Ok(new { Token = token, UserId = user.Id, Email = user.Email });
        }

        public class UserLoginDto
        {
            public string Email { get; set; } = string.Empty; // Changed from Username to Email
            public string Password { get; set; } = string.Empty;
        }
    }
}