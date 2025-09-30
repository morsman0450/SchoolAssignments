using SchoolAssignments.Models;

namespace SchoolAssignments.Services
{
    public interface ISubjectService
    {
        Task<List<Subject>> GetAllSubjectsAsync();
        Task<Subject> CreateSubjectAsync(Subject subject);
        Task DeleteSubjectAsync(int id);
    }
}
