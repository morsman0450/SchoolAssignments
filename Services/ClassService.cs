using Microsoft.EntityFrameworkCore;
using SchoolAssignments.Data;
using SchoolAssignments.Models;

namespace SchoolAssignments.Services
{
    public class ClassService : IClassService
    {
        private readonly AppDbContext _context;

        public ClassService(AppDbContext context)
        {
            _context = context;
        }

        // Získá všechny třídy konkrétního učitele
        public async Task<List<Class>> GetTeacherClassesAsync(int teacherId)
        {
            return await _context.Classes
                                 .Where(c => c.TeacherId == teacherId && c.IsActive)
                                 .Include(c => c.Students)
                                 .Include(c => c.Activities)
                                 .OrderBy(c => c.Name)
                                 .ToListAsync();
        }

        // Získá konkrétní třídu podle ID
        public async Task<Class?> GetClassByIdAsync(int classId)
        {
            return await _context.Classes
                                 .Include(c => c.Students)
                                 .Include(c => c.Activities)
                                 .FirstOrDefaultAsync(c => c.Id == classId);
        }

        // Vytvoří novou třídu
        public async Task CreateClassAsync(Class newClass)
        {
            newClass.CreatedAt = DateTime.UtcNow;
            _context.Classes.Add(newClass);
            await _context.SaveChangesAsync();
        }

        // Aktualizuje existující třídu
        public async Task UpdateClassAsync(Class updatedClass)
        {
            _context.Classes.Update(updatedClass);
            await _context.SaveChangesAsync();
        }

        // Smaže třídu (nastaví IsActive na false)
        public async Task DeleteClassAsync(int classId)
        {
            var classToDelete = await _context.Classes.FindAsync(classId);
            if (classToDelete != null)
            {
                classToDelete.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
        // Získá všechny aktivní třídy
        public async Task<List<Class>> GetAllClassesAsync()
        {
            return await _context.Classes
                                 .Where(c => c.IsActive)          // jen aktivní
                                 .Include(c => c.Students)        // volitelně, pokud chceš
                                 .Include(c => c.Activities)      // volitelně
                                 .OrderBy(c => c.Name)
                                 .ToListAsync();
        }


    }
}
