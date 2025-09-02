using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using SchoolAssignments.Data;
using SchoolAssignments.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SchoolAssignments.Services
{
    public class SubmissionService : ISubmissionService
    {
        private readonly AppDbContext _context;
        private readonly IJudge0Service _judge;

        public SubmissionService(AppDbContext context, IJudge0Service judge)
        {
            _context = context;
            _judge = judge;
        }

        // Vytvoření nové submission s odpověďmi
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

        // Získat submission podle ID (jen základní)
        public async Task<Submission?> GetSubmissionByIdAsync(int submissionId)
        {
            return await _context.Submissions
                .Include(s => s.Student)
                .Include(s => s.Activity)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }

        // Získat submission s kompletními detaily (pro UI)
        public async Task<Submission?> GetSubmissionWithDetailsAsync(int submissionId)
        {
            return await _context.Submissions
                .Include(s => s.Activity)
                    .ThenInclude(a => a.Questions)
                        .ThenInclude(q => q.Options)
                .Include(s => s.StudentAnswers)
                    .ThenInclude(sa => sa.AnswerOption)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == submissionId);
        }

        // Načíst všechny submission pro aktivitu (pro učitele)
        public async Task<List<Submission>> GetSubmissionsByActivityAsync(int activityId)
        {
            return await _context.Submissions
                .Where(s => s.ActivityId == activityId)
                .Include(s => s.Student)
                .Include(s => s.StudentAnswers)
                    .ThenInclude(sa => sa.AnswerOption)
                .ToListAsync();
        }

        // Spustit a ohodnotit submission přes Judge0
        public async Task<Submission> RunAndGradeSubmissionAsync(Submission submission)
        {
            var result = await _judge.RunCodeAsync(
                submission.LanguageId!.Value,
                submission.Code!,
                submission.StdInput
            );

            // pokud je compileOutput neprázdný, použijeme ho, jinak stderr
            submission.Stderr = !string.IsNullOrEmpty(result.CompileOutput) ? result.CompileOutput : result.Stderr;
            submission.StdOutput = result.Stdout;
            submission.ExitCode = result.ExitCode;

            // jednoduché tolerantní hodnocení
            submission.Points = CompareOutput(submission.Activity.ExpectedOutput ?? "", result.Stdout ?? "") && (result.ExitCode ?? 0) == 0
                                ? submission.Activity.MaxPoints
                                : 0;

            submission.Feedback = submission.Points == submission.Activity.MaxPoints
                                  ? "Výstup odpovídá očekávání."
                                  : $"Chyba ve výstupu:\nOčekávané: {submission.Activity.ExpectedOutput}\nDostali jsme: {result.Stdout}\nChyby: {submission.Stderr}";

            submission.Status = SubmissionStatus.Graded;
            submission.SubmittedAt = DateTime.UtcNow;

            var exists = await _context.Submissions.AnyAsync(s => s.Id == submission.Id);
            if (exists)
                _context.Submissions.Update(submission);
            else
                _context.Submissions.Add(submission);

            await _context.SaveChangesAsync();
            return submission;
        }


        // --- Tolerantní porovnání ---
        private bool CompareOutput(string expected, string actual, double numberTolerance = 1e-6)
        {
            expected ??= "";
            actual ??= "";

            string Normalize(string s)
            {
                s = s.Replace("\r\n", "\n").Replace("\r", "\n");
                var lines = s.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                             .Select(line => line.Trim());
                return Regex.Replace(string.Join("\n", lines), @"\s+", " ").ToLower();
            }

            expected = Normalize(expected);
            actual = Normalize(actual);

            if (double.TryParse(expected, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var e) &&
                double.TryParse(actual, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var a))
            {
                return Math.Abs(e - a) <= numberTolerance;
            }

            return expected == actual;
        }


        public async Task UpdateSubmissionAsync(Submission submission)
        {
            _context.Submissions.Update(submission);
            await _context.SaveChangesAsync();
        }

        public async Task<Submission> UploadFilesAsync(int activityId, int studentId, List<IBrowserFile> files)
        {
            if (files == null || !files.Any())
                throw new InvalidOperationException("Nebyl vybrán žádný soubor.");

            if (files.Count > 5)
                throw new InvalidOperationException("Maximálně můžete odevzdat 5 souborů.");

            var uploadsFolder = Path.Combine("wwwroot", "uploads", "submissions");
            Directory.CreateDirectory(uploadsFolder);

            var submission = new Submission
            {
                ActivityId = activityId,
                StudentId = studentId,
                SubmittedAt = DateTime.UtcNow,
                Status = SubmissionStatus.Submitted
            };

            foreach (var file in files)
            {
                var uniqueFileName = $"{Guid.NewGuid()}_{file.Name}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.OpenReadStream(maxAllowedSize: 20_000_000).CopyToAsync(stream); // 20 MB limit

                submission.Files.Add(new SubmissionFile
                {
                    FileName = file.Name,
                    FilePath = $"/uploads/submissions/{uniqueFileName}"
                });
            }

            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            return submission;
        }


    }
}
