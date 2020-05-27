using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ECMS.Data;
using ECMS.Security;
using Newtonsoft.Json;

namespace ECMS.Service
{
    public class UserManagementService
    {
        private Dictionary<string, UserData> _userDataMap = new Dictionary<string, UserData>();

        public void LoadData()
        {
            if (File.Exists("userdata.json"))
            {
                _userDataMap = JsonConvert.DeserializeObject<Dictionary<string, UserData>>(File.ReadAllText("userdata.json"));
            }
        }

        public void SaveData()
        {
            var v = JsonConvert.SerializeObject(_userDataMap);
            File.WriteAllText("userdata.json", v);
        }

        public void UserJoined(string username)
        {
            var user = new UserData()
            {
                Alias = username,
                Bio = StaticConfig.DefaultBio,
                JoinedTime = DateTime.UtcNow,
                LastActiveTime = DateTime.UtcNow,
                Username = username
            };
            _userDataMap[username] = user;
        }

        public void UserLeft(string username)
        {
            _userDataMap.Remove(username);
        }

        public void UserLoggedIn(string username)
        {

        }
    }
}
