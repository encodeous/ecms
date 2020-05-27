using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ECMS.Security
{
    public interface IPasswordHasher
    {
        public byte[] GenerateSalt();

        public byte[] HashPassword(string password, byte[] salt);
    }
}
