namespace SnippetsAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        // Navigation property - one user can have many snippets
        public ICollection<Snippet> Snippets { get; set; } = new List<Snippet>();
    }
}