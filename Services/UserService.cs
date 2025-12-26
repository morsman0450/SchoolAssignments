using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using SchoolAssignments.Data;
using SchoolAssignments.Models;
using SchoolAssignments.ViewModels;

namespace SchoolAssignments.Services
{
    public class UserService
    {
        private readonly IAuthService _authService;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public UserService(IAuthService authService, IWebHostEnvironment environment, AppDbContext context)
        {
            _authService = authService;
            _environment = environment;
            _context = context;
        }

        // 🔹 Získání uživatele
        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _authService.GetUserByIdAsync(userId);
        }

        // 🔹 Ověření dostupnosti
        public async Task<bool> IsUsernameAvailableAsync(string username)
        {
            return await _authService.IsUsernameAvailableAsync(username);
        }

        public async Task<bool> IsEmailAvailableAsync(string email)
        {
            return await _authService.IsEmailAvailableAsync(email);
        }

        // 🔹 Login / registrace / změna hesla
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

        // 🔹 Upload profilové fotky
        public async Task<User?> UpdateProfilePictureAsync(int userId, Stream imageStream, string fileName)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return null;

            var uploadDir = Path.Combine(_environment.WebRootPath, "uploads", "profile_pics");
            if (!Directory.Exists(uploadDir))
                Directory.CreateDirectory(uploadDir);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
            var filePath = Path.Combine(uploadDir, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            // Smazání starého souboru (pokud existuje)
            if (!string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                var oldFile = Path.Combine(_environment.WebRootPath, user.ProfilePicturePath.TrimStart('/'));
                if (File.Exists(oldFile))
                    File.Delete(oldFile);
            }

            user.ProfilePicturePath = $"/uploads/profile_pics/{uniqueFileName}";
            await _context.SaveChangesAsync();

            return user;
        }

        // =====================================================================
        // 🔹 ADMIN / DASHBOARD METODY
        // =====================================================================

        // Počet všech uživatelů
        public async Task<int> GetUserCountAsync()
        {
            return await _context.Users.CountAsync();
        }

        // Počet aktivních uživatelů
        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _context.Users.CountAsync(u => u.IsActive);
        }

        // Počet přihlášených během posledních 24 hodin
        public async Task<int> GetActiveLast24hAsync()
        {
            var since = DateTime.UtcNow.AddHours(-24);
            return await _context.Users.CountAsync(u => u.LastLoginAt != null && u.LastLoginAt > since);
        }

        // Posledních 5 přihlášení
        public async Task<List<User>> GetLastLoginsAsync(int limit = 5)
        {
            return await _context.Users
                .Where(u => u.LastLoginAt != null)
                .OrderByDescending(u => u.LastLoginAt)
                .Take(limit)
                .ToListAsync();
        }

        // Vrátí všechny uživatele pro admin sekci
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.LastName)
                .ToListAsync();
        }

        // Změní stav (aktivní / neaktivní)
        public async Task<bool> ToggleUserActiveAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            user.IsActive = !user.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        // Smazání uživatele (včetně profilovky)
        public async Task<bool> DeleteUserAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            if (!string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                var path = Path.Combine(_environment.WebRootPath, user.ProfilePicturePath.TrimStart('/'));
                if (File.Exists(path))
                    File.Delete(path);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        // Počet administrátorů
        public async Task<int> GetAdminCountAsync()
        {
            return await _context.Users.CountAsync(u => u.Role == UserRole.Admin);
        }

        // Počet studentů
        public async Task<int> GetStudentCountAsync()
        {
            return await _context.Users.CountAsync(u => u.Role == UserRole.Student);
        }

        // Počet učitelů
        public async Task<int> GetTeacherCountAsync()
        {
            return await _context.Users.CountAsync(u => u.Role == UserRole.Teacher);
        }
        public async Task<int> GetActiveUserCountAsync()
        {
            return await _context.Users.CountAsync(u => u.IsActive);
        }
      

        // 🔹 Počet chyb nebo logů (příklad)
        public async Task<int> GetSystemLogCountAsync()
        {
            // Pokud máš tabulku LogEntries, použij:
            // return await _context.LogEntries.CountAsync();

            // Pokud zatím nemáš tabulku logů, vrátí se napevno 0 (placeholder)
            return await Task.FromResult(0);
        }
    }
}
