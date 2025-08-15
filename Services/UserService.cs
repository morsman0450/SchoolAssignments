using SchoolAssignments.Models;
using SchoolAssignments.ViewModels;

namespace SchoolAssignments.Services
{
    public class UserService
    {
        private readonly IAuthService _authService;
        public UserService(IAuthService authService)
        {
            _authService = authService;
        }
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _authService.GetUserByIdAsync(userId);
        }
        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return await _authService.IsUsernameAvailableAsync(username);
        }
        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return await _authService.IsEmailAvailableAsync(email);
        }

        public async Task<User?> LoginAsync(string username, string password)
        {
            return await _authService.LoginAsync(username, password);
        }

        public async Task<bool?> RegisterAsync(RegisterModel model)
        {
            return await _authService.RegisterAsync(model);
        }
        public async Task<User?> UpdateUserAsync(User user)
        {
            return await _authService.UpdateUserAsync(user);

        }
        public async Task<User?> ChangePassword(User user, string newPassword, string currentPassword)
        {
            return await _authService.ChangePassword(user, newPassword, currentPassword);
        }
    }
}
