using Microsoft.EntityFrameworkCore;
using SchoolAssignments.Data;
using SchoolAssignments.Models;

namespace SchoolAssignments.Services
{
    public class ActivityService : IActivityService
    {
        private readonly AppDbContext _context;

        public ActivityService(AppDbContext context)
        {
            _context = context;
        }

        // 🟢 vytvoření jednoduché aktivity
        public async Task<int> CreateActivityAsync(Activity activity, int teacherId)
        {
            activity.CreatedByUserId = teacherId; // nastavíme autora
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            return activity.Id;
        }

        // 🟢 vytvoření aktivity s otázkami
        public async Task<int> CreateActivityWithQuestionsAsync(Activity activity, List<Question> questions, int teacherId)
        {
            if (activity.DueDate.HasValue && activity.DueDate.Value.Kind == DateTimeKind.Unspecified)
            {
                activity.DueDate = DateTime.SpecifyKind(activity.DueDate.Value, DateTimeKind.Utc);
            }

            activity.CreatedByUserId = teacherId; // nastavíme autora

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync(); // potřebujeme ID aktivity

            // Přidáme otázky + odpovědi
            foreach (var question in questions)
            {
                question.ActivityId = activity.Id;
                _context.Questions.Add(question);

                foreach (var option in question.Options)
                {
                    option.Question = question;
                    _context.AnswerOptions.Add(option);
                }
            }

            await _context.SaveChangesAsync();
            return activity.Id;
        }

        // 🟢 Detail aktivity s class + otázkami
        public async Task<Activity?> GetActivityWithDetailsAsync(int activityId)
        {
            return await _context.Activities
                .Include(a => a.Class)
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.Id == activityId);
        }

        // 🟢 Jen otázky k aktivitě
        public async Task<Activity?> GetActivityWithQuestionsAsync(int activityId)
        {
            return await _context.Activities
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.Id == activityId);
        }

        // 🟢 Přehled aktivit učitele
        public async Task<List<Activity>> GetTeacherActivitiesAsync(int teacherId)
        {
            return await _context.Activities
                .Where(a => a.CreatedByUserId == teacherId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }
        public async Task UpdateActivityAsync(Activity activity)
        {
            // Aktualizuje existující entitu v DbContext
            _context.Activities.Update(activity);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteActivityAsync(int activityId)
        {
            var activity = await _context.Activities.FindAsync(activityId);
            if (activity != null)
            {
                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();
            }
        }


    }
}
