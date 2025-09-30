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
            return await _context.ClassTeacherSubjects
                                 .Where(cts => cts.TeacherId == teacherId)
                                 .Include(cts => cts.Class)
                                     .ThenInclude(c => c.Students)
                                 .Include(cts => cts.Class)
                                     .ThenInclude(c => c.Activities)
                                 .Select(cts => cts.Class)
                                 .Distinct()
                                 .OrderBy(c => c.Name)
                                 .ToListAsync();
        }

        // Získá konkrétní třídu podle ID
        public async Task<Class?> GetClassByIdAsync(int classId)
        {
            return await _context.Classes
                .Include(c => c.Students)
                    .ThenInclude(cs => cs.Student)
                .Include(c => c.ClassTeacherSubjects)
                    .ThenInclude(cts => cts.Teacher)
                .Include(c => c.ClassTeacherSubjects)
                    .ThenInclude(cts => cts.Subject)
                .Include(c => c.ClassTeacherSubjects)
                    .ThenInclude(cts => cts.Activities)
                        .ThenInclude(a => a.Questions)
                .FirstOrDefaultAsync(c => c.Id == classId);
        }




        // Vytvoří novou třídu
        public async Task<Class> CreateClassAsync(Class newClass)
        {
            newClass.CreatedAt = DateTime.UtcNow;
            _context.Classes.Add(newClass);
            await _context.SaveChangesAsync();
            return newClass;
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
                                 .Where(c => c.IsActive)
                                 .Include(c => c.Students)
                                 .Include(c => c.Activities)
                                 .OrderBy(c => c.Name)
                                 .ToListAsync();
        }

        // Získá všechny ClassTeacherSubject záznamy, případně jen pro konkrétního učitele
        public async Task<List<ClassTeacherSubject>> GetAllClassTeacherSubjectsAsync(int? teacherId = null)
        {
            var query = _context.ClassTeacherSubjects
                                .Include(cts => cts.Class)
                                .Include(cts => cts.Subject)
                                .Include(cts => cts.Teacher)
                                .AsQueryable();

            if (teacherId.HasValue)
            {
                query = query.Where(cts => cts.TeacherId == teacherId.Value);
            }

            return await query.OrderBy(cts => cts.Class.Name)
                              .ThenBy(cts => cts.Subject.Name)
                              .ToListAsync();
        }
    }
}
