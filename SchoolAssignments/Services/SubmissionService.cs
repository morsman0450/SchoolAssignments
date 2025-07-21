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
            // Přidáme submission
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Nyní nastavíme SubmissionId u studentAnswers a přidáme je
            foreach (var answer in answers)
            {
                answer.SubmissionId = submission.Id;
                _context.StudentAnswers.Add(answer);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Submission?> GetSubmissionByIdAsync(int submissionId)
        {
            return await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Activity)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }
    }
}
