using SchoolAssignments.Models;

namespace SchoolAssignments.Services
{
    public interface IActivityService
    {
        Task<int> CreateActivityAsync(Activity activity, int teacherId, int classTeacherSubjectId);

        Task<int> CreateActivityWithQuestionsAsync(Activity activity, List<Question> questions, int teacherId, int classTeacherSubjectId);
        Task<Activity?> GetActivityWithDetailsAsync(int activityId);
        Task<Activity?> GetActivityWithQuestionsAsync(int activityId);
        Task<List<Activity>> GetTeacherActivitiesAsync(int teacherId);
        Task UpdateActivityAsync(Activity activity);
        Task<List<Activity>> GetStudentActivitiesAsync(int studentId);
        Task DeleteActivityAsync(int activityId);
        Task<List<Activity>> GetPastTeacherActivitiesAsync(int teacherId);
        Task<List<Activity>> GetPastStudentActivitiesAsync(int studentId);
        Task<int> GetActiveActivitiesCountAsync();



    }
}
