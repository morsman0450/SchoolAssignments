using SchoolAssignments.Models;
using SchoolAssignments.ViewModels;

namespace SchoolAssignments.Services
{
    public interface IAuthService
    {
        Task<User?> LoginAsync(string username, string password);
        Task<bool?> RegisterAsync(RegisterModel model);
        Task<User?> GetUserByIdAsync(int userId);
        Task<bool> IsUsernameAvailableAsync(string username);
        Task<bool> IsEmailAvailableAsync(string email);
        Task<User?> UpdateUserAsync(User user);
        Task<User?> ChangePassword(User user, string newPassword, string currentPassword);
    }
 
}
