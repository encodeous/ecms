using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ECMS.Data;
using ECMS.Security;
using ECMS.Utils;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Newtonsoft.Json;

namespace ECMS.Service
{
    public class Authenticator
    {
        public IPasswordHasher Hasher = new Aes512PasswordHasher();
        private Dictionary<string, AuthData> _userDictionary = new Dictionary<string, AuthData>();
        private Dictionary<string, SessionData> _sessionMap = new Dictionary<string, SessionData>();
        private UserManagementService _managementService;

        public Authenticator(UserManagementService management)
        {
            _managementService = management;
        }

        public void LoadData()
        {
            if (File.Exists("auth.json"))
            {
                _userDictionary = JsonConvert.DeserializeObject<Dictionary<string, AuthData>>(File.ReadAllText("auth.json"));
            }
            if (File.Exists("sessions.json"))
            {
                _sessionMap = JsonConvert.DeserializeObject<Dictionary<string, SessionData>>(File.ReadAllText("sessions.json"));
            }
        }

        public void SaveData()
        {
            var v = JsonConvert.SerializeObject(_userDictionary);
            File.WriteAllText("auth.json", v);

            var k = JsonConvert.SerializeObject(_sessionMap);
            File.WriteAllText("sessions.json", k);
        }

        public bool RemoveUser(string username, string password)
        {
            username = username.ToLower().Trim();
            if (UserExists(username))
            {
                if (!Validate(username, password)) return false;
                _userDictionary.Remove(username);
                _managementService.UserLeft(username);
                return true;
            }

            return false;
        }

        public bool ForceRemoveUser(string username)
        {
            username = username.ToLower().Trim();
            if (UserExists(username))
            {
                _userDictionary.Remove(username);
                _managementService.UserLeft(username);
                return true;
            }

            return false;
        }

        public string CreateSession(string username)
        {
            username = username.ToLower().Trim();
            if (UserExists(username))
            {
                var data = _userDictionary[username];
                var session = new SessionData(data.PasswordHash, data.PasswordSalt, data.Username);
                var token = Convert.ToBase64String(session.SessionToken);
                _sessionMap[token] = session;
                return token;
            }
            return null;
        }

        public SessionData GetSessionData(string session)
        {
            if (_sessionMap.ContainsKey(session))
            {
                var data = _sessionMap[session];
                if (_userDictionary.ContainsKey(data.Username))
                {
                    var authInfo = _userDictionary[data.Username];
                    bool valid = data.ValidateSession(authInfo.PasswordHash, authInfo.PasswordSalt);
                    if (valid)
                    {
                        return data;
                    }
                }
                _sessionMap.Remove(session);
            }
            return null;
        }

        public void SetUser(string username, string password)
        {
            username = username.ToLower().Trim();
            var salt = Hasher.GenerateSalt();
            _userDictionary[username] = new AuthData()
            {
                PasswordHash = Hasher.HashPassword(password, salt),
                PasswordSalt = salt,
                Username = username
            };
            foreach (var k in _sessionMap)
            {
                if (k.Value.Username == username)
                {
                    _sessionMap.Remove(k.Key);
                }
            }
        }

        public bool UserExists(string username)
        {
            if (string.IsNullOrEmpty(username)) return false;
            username = username.ToLower().Trim();
            return _userDictionary.ContainsKey(username);
        }

        public bool Validate(string username, string password)
        {
            if (string.IsNullOrEmpty(username)) return false;
            username = username.ToLower().Trim();
            if (UserExists(username))
            {
                var stored = _userDictionary[username];

                var hash = Hasher.HashPassword(password, stored.PasswordSalt);

                return Utilities.Compare(stored.PasswordHash, hash);
            }

            return false;
        }

        public SecurityRole GetUserRole(string username)
        {
            username = username.ToLower().Trim();

            if (UserExists(username))
            {
                return _userDictionary[username].UserRole;
            }

            return SecurityRole.Visitor;
        }

        /// <summary>
        /// Tries to set the users role
        /// </summary>
        /// <param name="username"></param>
        /// <param name="role"></param>
        /// <param name="auth"></param>
        /// <returns>true if success, else it is not found</returns>
        public bool SetUserRole(string username, SecurityRole role)
        {
            if (string.IsNullOrEmpty(username)) return false;
            if (!UserExists(username)) return false;
            _userDictionary[username].UserRole = role;
            return true;
        }

        /// <summary>
        /// Tries to register the user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="auth"></param>
        /// <returns>true if successful, else it already exists</returns>
        public bool RegisterUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username)) return false;
            if (UserExists(username)) return false;
            _managementService.UserJoined(username);
            SetUser(username, password);
            return true;
        }

        /// <summary>
        /// Parses Role from name
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public SecurityRole? GetSecurityRoleFromName(string role)
        {
            Enum.TryParse(typeof(SecurityRole), role, out var k);
            return (SecurityRole?)k;
        }

        public bool LogOut(string session)
        {
            return _sessionMap.Remove(session);
        }
    }
}
