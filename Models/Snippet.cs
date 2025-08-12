using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SnippetsAPI.Models
{
    public class Snippet
    {
        public int Id { get; set; }
        public string Language { get; set; } = string.Empty;
        
        // Store encrypted code in database
        [Column("Code")]
        public string EncryptedCode { get; set; } = string.Empty;
        
        // Property for plaintext code (not mapped to database)
        [NotMapped]
        public string Code { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Foreign key
        public int UserId { get; set; }
        
        // Navigation property
        [JsonIgnore]
        public User? User { get; set; }
    }
}