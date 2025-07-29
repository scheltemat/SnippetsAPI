using Microsoft.EntityFrameworkCore;
using SnippetsAPI.Models;

namespace SnippetsAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Snippet> Snippets { get; set; }
        public DbSet<User> Users { get; set; }
    }
}
