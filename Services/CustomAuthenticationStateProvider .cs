using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SchoolAssignments.Models;
using SchoolAssignments.Services;
using System.Security.Claims;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IAuthService _authService;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ProtectedSessionStorage sessionStorage, IAuthService authService)
    {
        _sessionStorage = sessionStorage;
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var userSessionStorageResult = await _sessionStorage.GetAsync<UserSession>("UserSession");

            if (userSessionStorageResult.Success && userSessionStorageResult.Value != null)
            {
                var userSession = userSessionStorageResult.Value;
                var user = await _authService.GetUserByIdAsync(userSession.Id);
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
                        }, "CustomAuth"));

                    return new AuthenticationState(claimsPrincipal);
                }
            }
        }
        catch
        {
        }

        return new AuthenticationState(_anonymous);
    }

    public async Task LoginAsync(User user)
    {
        var userSession = new UserSession
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role.ToString()
        };

        await _sessionStorage.SetAsync("UserSession", userSession);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
            {
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.GivenName, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new(ClaimTypes.Role, user.Role.ToString())
            }, "CustomAuth"));

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
    }

    public async Task LogoutAsync()
    {
        await _sessionStorage.DeleteAsync("UserSession");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}

public class UserSession
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}