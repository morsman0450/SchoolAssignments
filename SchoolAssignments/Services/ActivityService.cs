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

        public async Task CreateActivityAsync(Activity activity)
        {
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
        }
        public async Task<int> CreateActivityWithQuestionsAsync(Activity activity, List<Question> questions)
        {
            if (activity.DueDate.HasValue && activity.DueDate.Value.Kind == DateTimeKind.Unspecified)
            {
                activity.DueDate = DateTime.SpecifyKind(activity.DueDate.Value, DateTimeKind.Utc);
            }
            // Přidání aktivity do kontextu
            _context.Activities.Add(activity);
            await _context.SaveChangesAsync(); // potřebujeme znát activity.Id
           

            // Pro každou otázku nastavíme ActivityId a přidáme odpovědi
            foreach (var question in questions)
            {
                question.ActivityId = activity.Id;

                _context.Questions.Add(question);

                foreach (var option in question.Options)
                {
                    option.Question = question; // naváže se k otázce
                    _context.AnswerOptions.Add(option);
                }
            }

            await _context.SaveChangesAsync(); // Uložíme otázky a odpovědi

            return activity.Id;
        }
        public async Task<Activity?> GetActivityWithDetailsAsync(int activityId)
        {
            return await _context.Activities
                .Include(a => a.Class)
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.Id == activityId);
        }
        public async Task<Activity?> GetActivityWithQuestionsAsync(int activityId)
        {
            return await _context.Activities
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.Id == activityId);
        }

    }


}
