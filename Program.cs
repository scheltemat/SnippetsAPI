using Microsoft.EntityFrameworkCore;
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

builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();

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

    // Seed users
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
                // Encrypt the code before saving
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
app.MapControllers();

app.MapGet("/", () => Results.Ok("Application is running!"));

app.Run();
