using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Security
{
    public class Aes512PasswordHasher : IPasswordHasher
    {
        public byte[] GenerateSalt()
        {
            using var crng = new RNGCryptoServiceProvider();
            var salt = new byte[64];
            crng.GetBytes(salt);
            return salt;
        }

        public byte[] HashPassword(string password, byte[] salt)
        {
            using SHA512 sha = new SHA512CryptoServiceProvider();
            using MemoryStream ms = new MemoryStream();
            ms.Write(Encoding.UTF32.GetBytes(password));
            ms.Write(salt);
            return sha.ComputeHash(ms.ToArray());
        }
    }
}
