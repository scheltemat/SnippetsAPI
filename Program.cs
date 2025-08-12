using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using SnippetsAPI.Data;
using SnippetsAPI.Models;
using SnippetsAPI.Services;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddSwaggerGen();

// Register services
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IJwtService, JwtService>();

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? "MySuperSecretKeyThatShouldBeAtLeast32CharactersOrSomethingIGuess!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "SnippetsAPI",
            ValidateAudience = true,
            ValidAudience = jwtSettings["Audience"] ?? "SnippetsAPIUsers",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Run migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();

    // Seed users first
    if (!db.Users.Any())
    {
        var jsonData = File.ReadAllText("seed/Users.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var users = JsonSerializer.Deserialize<List<User>>(jsonData, options);
        if (users != null)
        {
            foreach (var user in users)
            {
                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    user.Password = passwordHasher.HashPassword(user.Password);
                }
            }
            db.Users.AddRange(users);
            db.SaveChanges();
        }
    }

    // Then seed snippets and assign them to the first user
    if (!db.Snippets.Any())
    {
        var firstUser = db.Users.First();
        var encryptionService = scope.ServiceProvider.GetRequiredService<IEncryptionService>();
        
        var jsonData = File.ReadAllText("seed/Snippets.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var snippets = JsonSerializer.Deserialize<List<Snippet>>(jsonData, options);
        if (snippets != null)
        {
            foreach (var snippet in snippets)
            {
                snippet.UserId = firstUser.Id;
                snippet.EncryptedCode = encryptionService.Encrypt(snippet.Code);
            }
            
            db.Snippets.AddRange(snippets);
            db.SaveChanges();
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();

app.UseAuthentication(); // Add this before UseAuthorization
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.MapGet("/", () => Results.Ok("Application is running!"));

app.Run();