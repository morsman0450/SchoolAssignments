using Microsoft.EntityFrameworkCore;
using SchoolAssignments.Data;
using SchoolAssignments.Models;

namespace SchoolAssignments.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly AppDbContext _db;

        public SubjectService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<List<Subject>> GetAllSubjectsAsync()
        {
            return await _db.Subjects.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<Subject> CreateSubjectAsync(Subject subject)
        {
            _db.Subjects.Add(subject);
            await _db.SaveChangesAsync();
            return subject;
        }

        public async Task DeleteSubjectAsync(int id)
        {
            var subject = await _db.Subjects.FindAsync(id);
            if (subject != null)
            {
                _db.Subjects.Remove(subject);
                await _db.SaveChangesAsync();
            }
        }
    }
}
