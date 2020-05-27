using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ECMS.Data;
using ECMS.Security;
using ECMS.Service;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ECMS.Service
{
    public class EcmsAuthenticationStateProvider : AuthenticationStateProvider
    {
        private ILocalStorageService _localStorageService;
        private Authenticator _authenticator;
        private UserManagementService _managementService;

        public EcmsAuthenticationStateProvider(ILocalStorageService localStorageService, Authenticator authenticator, UserManagementService managementService)
        {
            _localStorageService = localStorageService;
            _authenticator = authenticator;
            _managementService = managementService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var session = await _localStorageService.GetItemAsync<string>("ecmssessiondata");
            if(!string.IsNullOrEmpty(session))
            {
                var data = _authenticator.GetSessionData(session);
                if(data != null)
                {
                    var state = AuthenticateUser(data);
                    if(state != null) return state;
                }
            }
            var user = new ClaimsPrincipal(new ClaimsIdentity());
            return new AuthenticationState(user);
        }

        public async Task SignOut()
        {
            if (await _localStorageService.ContainKeyAsync("ecmssessiondata"))
            {
                var session = await _localStorageService.GetItemAsync<string>("ecmssessiondata");
                if (!string.IsNullOrEmpty(session))
                {
                    _authenticator.LogOut(session);
                    await _localStorageService.RemoveItemAsync("ecmssessiondata");
                }
            }
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        /// <summary>
        /// Tries to register the user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>true if successful</returns>
        public bool RegisterUser(string username, string password)
        {
            username = username.ToLower().Trim();
            return _authenticator.RegisterUser(username, password);
        }

        /// <summary>
        /// Tries to authenticate the user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="auth"></param>
        /// <returns>true if successful, else incorrect details</returns>
        public async ValueTask<bool> AuthenticateUser(string username, string password, NavigationManager redirectManager, string redirectUrl)
        {
            username = username.ToLower().Trim();
            if (_authenticator.Validate(username, password))
            {
                redirectManager.NavigateTo(redirectUrl);
                await Task.Delay(200);
                var identity = new ClaimsIdentity(new []
                {
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, _authenticator.GetUserRole(username).ToString())
                }, "apiauth_type");
                var user = new ClaimsPrincipal(identity);
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
                _managementService.UserLoggedIn(username);

                // Save session state
                var session = _authenticator.CreateSession(username);
                _ = _localStorageService.SetItemAsync("ecmssessiondata", session);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to authenticate the user
        /// </summary>
        /// <returns>true if successful, else incorrect details</returns>
        public AuthenticationState AuthenticateUser(SessionData session)
        {
            if (_authenticator.UserExists(session.Username))
            {
                var identity = new ClaimsIdentity(new []
                {
                    new Claim(ClaimTypes.Name, session.Username),
                    new Claim(ClaimTypes.Role, _authenticator.GetUserRole(session.Username).ToString())
                }, "apiauth_type");
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            return null;
        }
    }
}
