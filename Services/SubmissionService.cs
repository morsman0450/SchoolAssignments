using Microsoft.EntityFrameworkCore;
using SchoolAssignments.Data;
using SchoolAssignments.Models;

namespace SchoolAssignments.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly AppDbContext _context;

        public SubmissionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateSubmissionAsync(Submission submission, List<StudentAnswer> answers)
        {
            var activity = await _context.Activities
                .FirstOrDefaultAsync(a => a.Id == submission.ActivityId);

            if (activity == null)
                throw new InvalidOperationException("Aktivita neexistuje.");

            // Zjistit kolik pokusů už má student
            var existingAttempts = await _context.Submissions
                .Where(s => s.ActivityId == submission.ActivityId && s.StudentId == submission.StudentId)
                .OrderByDescending(s => s.AttemptNumber)
                .ToListAsync();

            var nextAttempt = existingAttempts.Any() ? existingAttempts.First().AttemptNumber + 1 : 1;

            // Pokud je nastaven limit a student už má maximum → chyba
            if (activity.MaxAttempts > 0 && nextAttempt > activity.MaxAttempts)
                throw new InvalidOperationException("Byl překročen maximální počet pokusů.");

            submission.AttemptNumber = nextAttempt;
            submission.StudentAnswers = answers;

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();
        }


        public async Task<Submission?> GetSubmissionByIdAsync(int submissionId)
        {
            return await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Activity)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }
        public async Task<Submission> GetSubmissionWithDetailsAsync(int submissionId)
        {
            return await _context.Submissions
                .Include(s => s.Activity)
                    .ThenInclude(a => a.Questions)
                        .ThenInclude(q => q.Options)
                .Include(s => s.StudentAnswers)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }

    }
}
