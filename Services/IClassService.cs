using SchoolAssignments.Models;

namespace SchoolAssignments.Services
{
    public interface IClassService
    {
        Task<List<Class>> GetTeacherClassesAsync(int teacherId);
        Task<Class?> GetClassByIdAsync(int classId);
        Task<Class> CreateClassAsync(Class newClass);
        Task UpdateClassAsync(Class updatedClass);
        Task DeleteClassAsync(int classId);
        Task<List<Class>> GetAllClassesAsync();
        Task<List<ClassTeacherSubject>> GetAllClassTeacherSubjectsAsync(int? teacherId = null);


    }
}
