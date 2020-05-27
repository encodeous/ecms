using ECMS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ECMS.Data
{
    public class SessionData
    {
        public DateTime SessionCreatedTime = DateTime.UtcNow;

        public DateTime SessionExpireTime = DateTime.UtcNow + TimeSpan.FromDays(30);

        public byte[] SessionToken;

        public byte[] RandomBytes;

        public string Username;

        public SessionData(byte[] userPassword, byte[] userSalt, string username)
        {
            Username = username;
            using var crng = new RNGCryptoServiceProvider();
            using SHA512 sha = new SHA512CryptoServiceProvider();
            using MemoryStream ms = new MemoryStream();
            RandomBytes = new byte[512];
            crng.GetBytes(RandomBytes);
            ms.Write(RandomBytes);
            ms.Write(Encoding.Unicode.GetBytes(Username));
            ms.Write(userPassword);
            ms.Write(userSalt);
            ms.Write(BitConverter.GetBytes(SessionCreatedTime.ToBinary()));

            SessionToken = sha.ComputeHash(ms.ToArray());
        }

        public bool ValidateSession(byte[] userPassword, byte[] userSalt)
        {
            using SHA512 sha = new SHA512CryptoServiceProvider();
            using MemoryStream ms = new MemoryStream();
            ms.Write(RandomBytes);
            ms.Write(Encoding.Unicode.GetBytes(Username));
            ms.Write(userPassword);
            ms.Write(userSalt);
            ms.Write(BitConverter.GetBytes(SessionCreatedTime.ToBinary()));

            var match = sha.ComputeHash(ms.ToArray());

            if(Utilities.Compare(match, SessionToken))
            {
                return DateTime.UtcNow < SessionExpireTime && SessionCreatedTime < DateTime.UtcNow;
            }

            return false;
        }
    }
}
