using Microsoft.EntityFrameworkCore;
using SchoolAssignments.Data;
using SchoolAssignments.Models;
using SchoolAssignments.ViewModels;

namespace SchoolAssignments.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        public AuthService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<User?> LoginAsync(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
           
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                return null; // Uživatelské jméno nebo heslo je nesprávné
            }

            user.LastLoginAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return user; // Přihlášení úspěšné

        }
        public async Task<bool?> RegisterAsync(RegisterModel model)
        {
            // Kontrola dostupnosti username a emailu
            if (!await IsUsernameAvailableAsync(model.Username))
                return false;

            if (!await IsEmailAvailableAsync(model.Email))
                return false;

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Role = model.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
        }

        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return !await _context.Users.AnyAsync(u => u.Username == username);
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }
        public async Task<User?> UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null || !existingUser.IsActive)
            {
                return null; // Uživatel neexistuje nebo není aktivní
            }
            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.Role = user.Role;
            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            return existingUser; // Vrátí aktualizovaného uživatele
        }
        public async Task<User?> ChangePassword(User user, string newPassword, string currentPassword)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null || !existingUser.IsActive)
            {
                return null; 
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(currentPassword, existingUser.PasswordHash);
            if (!isPasswordValid)
            {
                return null; 
            }

            existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            return existingUser; 
        }
       


    }
}
