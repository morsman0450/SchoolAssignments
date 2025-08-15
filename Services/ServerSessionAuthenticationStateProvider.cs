using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using SchoolAssignments.Models;
using System.Security.Claims;

namespace SchoolAssignments.Services
{
    public class ServerSessionAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthService _authService;
        private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        public ServerSessionAuthenticationStateProvider(IHttpContextAccessor httpContextAccessor, IAuthService authService)
        {
            _httpContextAccessor = httpContextAccessor;
            _authService = authService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var context = _httpContextAccessor.HttpContext;
                if (context?.Session != null)
                {
                    var userId = context.Session.GetString("UserId");
                    var username = context.Session.GetString("Username");
                    var role = context.Session.GetString("Role");

                    if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(role))
                    {
                        // Ověření platnosti session
                        var user = await _authService.GetUserByIdAsync(int.Parse(userId));
                        if (user != null)
                        {
                            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                            {
                                new(ClaimTypes.Name, user.Username),
                                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                new(ClaimTypes.Email, user.Email),
                                new(ClaimTypes.GivenName, user.FirstName),
                                new(ClaimTypes.Surname, user.LastName),
                                new(ClaimTypes.Role, user.Role.ToString())
                            }, "ServerSession"));

                            return new AuthenticationState(claimsPrincipal);
                        }
                    }
                }
            }
            catch
            {
                // Pokud nastane chyba, vrátíme anonymního uživatele
            }

            return new AuthenticationState(_anonymous);
        }

        public Task LoginAsync(User user)
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Session != null)
            {
                context.Session.SetString("UserId", user.Id.ToString());
                context.Session.SetString("Username", user.Username);
                context.Session.SetString("Role", user.Role.ToString());
                context.Session.SetString("Email", user.Email);
                context.Session.SetString("FirstName", user.FirstName);
                context.Session.SetString("LastName", user.LastName);

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
                {
                    new(ClaimTypes.Name, user.Username),
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.GivenName, user.FirstName),
                    new(ClaimTypes.Surname, user.LastName),
                    new(ClaimTypes.Role, user.Role.ToString())
                }, "ServerSession"));

                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
            }
            return Task.CompletedTask;
        }

        public  Task LogoutAsync()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Session != null)
            {
                context.Session.Clear();
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
            }
            return Task.CompletedTask;
        }
    }
}