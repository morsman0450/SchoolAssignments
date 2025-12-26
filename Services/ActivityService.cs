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

        // Vytvoření jednoduché aktivity
        public async Task<int> CreateActivityAsync(Activity activity, int teacherId, int classTeacherSubjectId)
        {
            activity.CreatedByUserId = teacherId;
            activity.ClassTeacherSubjectId = classTeacherSubjectId;

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
            return activity.Id;
        }

        // Vytvoření aktivity s otázkami
        public async Task<int> CreateActivityWithQuestionsAsync(Activity activity, List<Question> questions, int teacherId, int classTeacherSubjectId)
        {
           

            activity.CreatedByUserId = teacherId;
            activity.ClassTeacherSubjectId = classTeacherSubjectId;

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

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
            DateTime now = activity.CreatedAt;

            await _context.SaveChangesAsync();
            return activity.Id;
        }

        // Detail aktivity s ClassTeacherSubject + otázkami
        public async Task<Activity?> GetActivityWithDetailsAsync(int activityId)
        {
            return await _context.Activities
                .Include(a => a.ClassTeacherSubject)
                    .ThenInclude(cts => cts.Class)
                .Include(a => a.ClassTeacherSubject)
                    .ThenInclude(cts => cts.Subject)
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.Id == activityId);
        }

        // Jen otázky k aktivitě
        public async Task<Activity?> GetActivityWithQuestionsAsync(int activityId)
        {
            return await _context.Activities
                .Include(a => a.Questions)
                    .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.Id == activityId);
        }

        // Přehled aktivit učitele
        public async Task<List<Activity>> GetTeacherActivitiesAsync(int teacherId)
        {
            return await _context.Activities
    .Where(a => a.CreatedByUserId == teacherId && a.IsActive)
    .Include(a => a.ClassTeacherSubject)
        .ThenInclude(cts => cts.Subject)
    .OrderByDescending(a => a.CreatedAt)
    .ToListAsync();

        }


        public async Task<List<Activity>> GetPastTeacherActivitiesAsync(int teacherId)
        {
            return await _context.Activities
                .Where(a => a.CreatedByUserId == teacherId && a.IsActive)
                .Include(a => a.ClassTeacherSubject)
                .OrderByDescending(a => a.CreatedAt)
                .Include(a => a.ClassTeacherSubject.Subject)
                .ToListAsync();
        }

        // Přehled aktivit studenta
        public async Task<List<Activity>> GetStudentActivitiesAsync(int studentId)
        {
            var classTeacherSubjectIds = await _context.ClassStudents
                .Where(cs => cs.StudentId == studentId)
                .SelectMany(cs => cs.Class.ClassTeacherSubjects.Select(cts => cts.Id))
                .ToListAsync();

            var activities = await _context.Activities
                .Where(a => classTeacherSubjectIds.Contains(a.ClassTeacherSubjectId) && a.IsActive)
                .Include(a => a.Submissions)
                .Include(a => a.ClassTeacherSubject)              // přidá ClassTeacherSubject
                    .ThenInclude(cts => cts.Subject)              // a z něj Subject
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            foreach (var a in activities)
            {
                a.UsedAttempts = a.Submissions.Count(s => s.StudentId == studentId);
            }

            return activities;
        }


        public async Task<List<Activity>> GetPastStudentActivitiesAsync(int studentId)
        {
            var now = DateTime.UtcNow;

            var classTeacherSubjectIds = await _context.ClassStudents
                .Where(cs => cs.StudentId == studentId)
                .SelectMany(cs => cs.Class.ClassTeacherSubjects.Select(cts => cts.Id))
                .ToListAsync();

            var activities = await _context.Activities
                .Where(a => classTeacherSubjectIds.Contains(a.ClassTeacherSubjectId) && a.IsActive && a.DueDate < now)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return activities;
        }

        public async Task UpdateActivityAsync(Activity activity)
        {
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

        public async Task<int> GetActiveActivitiesCountAsync()
        {
            return await _context.Activities.CountAsync(a => a.IsActive);
        }
    }
}
