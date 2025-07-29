using System.Security.Cryptography;
using System.Text;

namespace SnippetsAPI.Services
{
    public interface IEncryptionService
    {
        string Encrypt(string plaintext);
        string Decrypt(string ciphertext);
    }

    public class EncryptionService : IEncryptionService
    {
        private readonly string _key;

        public EncryptionService(IConfiguration configuration)
        {
            _key = configuration["EncryptionKey"] ?? "MySecretKey12345"; // Should be 16+ chars
        }

        public string Encrypt(string plaintext)
        {
            if (string.IsNullOrEmpty(plaintext))
                return plaintext;

            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_key.PadRight(32).Substring(0, 32));
            aes.IV = new byte[16]; // Simple IV for demo - use random IV in production

            using var encryptor = aes.CreateEncryptor();
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            var ciphertextBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
            
            return Convert.ToBase64String(ciphertextBytes);
        }

        public string Decrypt(string ciphertext)
        {
            if (string.IsNullOrEmpty(ciphertext))
                return ciphertext;

            try
            {
                using var aes = Aes.Create();
                aes.Key = Encoding.UTF8.GetBytes(_key.PadRight(32).Substring(0, 32));
                aes.IV = new byte[16]; // Same IV as encryption

                using var decryptor = aes.CreateDecryptor();
                var ciphertextBytes = Convert.FromBase64String(ciphertext);
                var plaintextBytes = decryptor.TransformFinalBlock(ciphertextBytes, 0, ciphertextBytes.Length);
                
                return Encoding.UTF8.GetString(plaintextBytes);
            }
            catch
            {
                return ciphertext; // Return original if decryption fails
            }
        }
    }
}