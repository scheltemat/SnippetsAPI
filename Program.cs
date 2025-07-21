using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using SnippetsAPI.Data;
using SnippetsAPI.Models;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Run migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    if (!db.Snippets.Any())
    {
        var jsonData = File.ReadAllText("seed/SeedData.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var snippets = JsonSerializer.Deserialize<List<Snippet>>(jsonData, options);
        if (snippets != null)
        {
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
