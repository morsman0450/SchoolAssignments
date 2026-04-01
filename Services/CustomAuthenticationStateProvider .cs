using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SchoolAssignments.Models;
using SchoolAssignments.Services;
using System.Security.Claims;

public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ProtectedLocalStorage _localStorage;
    private readonly IAuthService _authService;
    private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(
        ProtectedSessionStorage sessionStorage,
        ProtectedLocalStorage localStorage,
        IAuthService authService)
    {
        _sessionStorage = sessionStorage;
        _localStorage = localStorage;
        _authService = authService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var sessionResult = await _sessionStorage.GetAsync<UserSession>("UserSession");
            if (sessionResult.Success && sessionResult.Value != null)
            {
                return await CreateAuthenticationState(sessionResult.Value);
            }

            var localResult = await _localStorage.GetAsync<UserSession>("UserSession");
            if (localResult.Success && localResult.Value != null)
            {
                await _sessionStorage.SetAsync("UserSession", localResult.Value);
                return await CreateAuthenticationState(localResult.Value);
            }
        }
        catch
        {
        }
        return new AuthenticationState(_anonymous);
    }

    private async Task<AuthenticationState> CreateAuthenticationState(UserSession userSession)
    {
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
                new(ClaimTypes.Role, user.Role.ToString()),
                new Claim("ProfilePicturePath", user.ProfilePicturePath ?? "")

            }, "CustomAuth"));

            return new AuthenticationState(claimsPrincipal);
        }
        return new AuthenticationState(_anonymous);
    }

    public async Task LoginAsync(User user, bool rememberMe = false)
    {
        var userSession = new UserSession
        {
            Id = user.Id,
            Username = user.Username,
            Role = user.Role.ToString()
        };

        // Vždy uložit do session storage
        await _sessionStorage.SetAsync("UserSession", userSession);

        // Pokud je remember me, uložit i do local storage
        if (rememberMe)
        {
            await _localStorage.SetAsync("UserSession", userSession);
        }

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
        await _localStorage.DeleteAsync("UserSession");
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }
}

public class UserSession
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}