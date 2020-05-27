using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECMS.Security;

namespace ECMS.Data
{
    public class AuthData
    {
        public string Username;
        public byte[] PasswordHash;
        public byte[] PasswordSalt;
        public SecurityRole UserRole = SecurityRole.Visitor;
    }
}
