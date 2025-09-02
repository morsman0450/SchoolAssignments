using SchoolAssignments.Models;

namespace SchoolAssignments.Services
{
    public interface IActivityService
    {
        Task<int> CreateActivityAsync(Activity activity, int teacherId);

        Task<int> CreateActivityWithQuestionsAsync(Activity activity, List<Question> questions, int teacherId);
        Task<Activity?> GetActivityWithDetailsAsync(int activityId);
        Task<Activity?> GetActivityWithQuestionsAsync(int activityId);
        Task<List<Activity>> GetTeacherActivitiesAsync(int teacherId);
        Task UpdateActivityAsync(Activity activity);
        
        Task DeleteActivityAsync(int activityId);


    }
}
