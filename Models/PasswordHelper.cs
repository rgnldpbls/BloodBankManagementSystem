using System.Security.Cryptography;


namespace BBMS.Models
{
    public class PasswordHelper
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32;  // 256 bit
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            using(var algorithm = new Rfc2898DeriveBytes(password, SaltSize, Iterations, HashAlgorithmName.SHA256))
            {
                var salt = algorithm.Salt;
                var key = algorithm.GetBytes(KeySize);
                var hash = Convert.ToBase64String(Combine(salt, key));
                return hash;
            }
        }

        private static byte[] Combine(byte[] salt, byte[] key)
        {
            var res = new byte[salt.Length + key.Length];
            Buffer.BlockCopy(salt, 0, res, 0, salt.Length);
            Buffer.BlockCopy(key, 0, res, salt.Length, key.Length);
            return res;
        }

        public static bool VerifyPassword(string password, string hashedPassword)
        {
            var salt = new byte[SaltSize];
            var key = new byte[KeySize];

            var hashBytes = Convert.FromBase64String(hashedPassword);
            Buffer.BlockCopy(hashBytes, 0, salt, 0, SaltSize);
            Buffer.BlockCopy(hashBytes, SaltSize, key, 0, KeySize);

            using(var algorithm = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                var keyToCheck = algorithm.GetBytes(KeySize);
                return keyToCheck.SequenceEqual(key);
            }
        }
    }
}
