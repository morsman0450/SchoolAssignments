using SchoolAssignments.Models;

namespace SchoolAssignments.Services
{
    public interface IActivityService
    {
        Task CreateActivityAsync(Activity activity);

        Task<int> CreateActivityWithQuestionsAsync(Activity activity, List<Question> questions);
        Task<Activity?> GetActivityWithDetailsAsync(int activityId);
        Task<Activity?> GetActivityWithQuestionsAsync(int activityId);

    }
}
