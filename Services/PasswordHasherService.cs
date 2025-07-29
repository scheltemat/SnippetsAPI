using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace SnippetsAPI.Services
{
    public interface IPasswordHasherService
    {
        string HashPassword(string password);
    }
    public class PasswordHasherService : IPasswordHasherService
    {
        public string HashPassword(string password)
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));

            // Store salt and hash together (Base64)
            return $"{Convert.ToBase64String(salt)}.{hashed}";
        }
    }
}
