using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using SchoolAssignments.Data;
using SchoolAssignments.Models;
using System.Globalization;
using System.Text.Json;
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
                            .Include(s => s.Student)
                            .Include(s => s.Files) // 👈 tohle je důležité
                            .Include(s => s.StudentAnswers)
                                .ThenInclude(sa => sa.AnswerOption)
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

        public async Task<Submission> RunAndGradeSubmissionAsync(Submission submission)
        {
            if (submission.Activity.Type == ActivityType.Code)
            {
                var result = await _judge.RunCodeAsync(
            submission.LanguageId!.Value,
                submission.Code!,
                submission.StdInput
            );

                submission.Stderr = !string.IsNullOrEmpty(result.CompileOutput) ? result.CompileOutput : result.Stderr;
                submission.StdOutput = result.Stdout;
                submission.ExitCode = result.ExitCode;

                if (!string.IsNullOrEmpty(submission.Stderr) || (result.ExitCode ?? 0) != 0)
                {
                    submission.Points = 0;
                    submission.Feedback = $"Kód nešel spustit nebo selhal:\n{submission.Stderr}";
                }
                else
                {
                    var expectedLines = (submission.Activity.ExpectedOutput ?? "")
                        .Replace("\r\n", "\n")
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    var actualLines = (result.Stdout ?? "")
                        .Replace("\r\n", "\n")
                        .Split('\n', StringSplitOptions.RemoveEmptyEntries);

                    int maxTests = expectedLines.Length;
                    int passed = 0;

                    for (int i = 0; i < maxTests; i++)
                    {
                        string expected = expectedLines[i];
                        string actual = i < actualLines.Length ? actualLines[i] : "";

                        if (CompareOutput(expected, actual))
                            passed++;
                    }

                    submission.Points = (int)Math.Round((double)passed / maxTests * submission.Activity.MaxPoints);

                    submission.Feedback = submission.Points == submission.Activity.MaxPoints
                        ? "Výstup odpovídá očekávání."
                        : $"Správně {passed}/{maxTests} testů.\n\nOčekávaný výstup:\n{submission.Activity.ExpectedOutput}\n\nDostal jsi:\n{result.Stdout}";
                }

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
            else if (submission.Activity.Type == ActivityType.CodeFill)
            {
                var correctJson = submission.Activity.CorrectFillJson ?? "{}";
                var studentJson = submission.FillAnswersJson ?? "{}";

                var correct = JsonSerializer.Deserialize<Dictionary<string, string>>(correctJson);
                var student = JsonSerializer.Deserialize<Dictionary<string, string>>(studentJson);

                int total = correct?.Count ?? 0;
                int correctCount = 0;
                var feedbackLines = new List<string>();

                if (correct != null && student != null)
                {
                    foreach (var kvp in correct)
                    {
                        student.TryGetValue(kvp.Key, out var studentValue);

                        if (Normalize(studentValue) == Normalize(kvp.Value))
                        {
                            correctCount++;
                            feedbackLines.Add($"✅ {kvp.Key}: {studentValue}");
                        }
                        else
                        {
                            feedbackLines.Add($"❌ {kvp.Key}: zadáno '{studentValue}', očekáváno '{kvp.Value}'");
                        }
                    }
                }

                int points = total > 0 ? (int)Math.Round((double)correctCount / total * submission.Activity.MaxPoints) : 0;

                submission.Points = points;
                submission.Feedback = string.Join("\n", feedbackLines);
                submission.Status = SubmissionStatus.Graded;
                submission.SubmittedAt = DateTime.UtcNow;

                // Ulož do DB
                _context.Submissions.Add(submission);
                await _context.SaveChangesAsync();

                return submission;
            }
            throw new InvalidOperationException("Nepodporovaný typ aktivity pro automatické hodnocení.");
        }



        // --- Tolerantní porovnání ---
        private bool CompareOutput(string expected, string actual, double numberTolerance = 1e-6)
        {
            expected ??= "";
            actual ??= "";

            expected = expected.Trim();
            actual = actual.Trim();

            // číselné porovnání s tolerancí
            if (double.TryParse(expected, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var e) &&
                double.TryParse(actual, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var a))
            {
                return Math.Abs(e - a) <= numberTolerance;
            }

            // normální text porovnání (case-insensitive + trim víc mezer)
            string Normalize(string s) => Regex.Replace(s, @"\s+", " ").Trim().ToLower();

            return Normalize(expected) == Normalize(actual);
        }
        private string Normalize(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            return Regex.Replace(text.Trim(), @"\s+", " ").ToLowerInvariant();
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
        public async Task<List<Submission>> GetStudentSubmissionsAsync(int studentId)
        {
            return await _context.Submissions
                .Include(s => s.Activity)
                .Where(s => s.StudentId == studentId)
                .OrderByDescending(s => s.SubmittedAt)
                .ToListAsync();
        }
        public async Task<int> GetUsedAttemptsAsync(int activityId, int studentId)
        {
            return await _context.Submissions
                .Where(s => s.ActivityId == activityId && s.StudentId == studentId)
                .CountAsync();
        }

        public async Task<List<StudentActivitySummary>> GetStudentActivitySummariesAsync(int studentId)
        {
            var submissions = await _context.Submissions
                .Where(s => s.StudentId == studentId)
                .Include(s => s.Activity)
                .ToListAsync();

            var summary = submissions
                .GroupBy(s => s.ActivityId)
                .Select(g => new StudentActivitySummary
                {
                    ActivityId = g.Key,
                    Title = g.First().Activity.Title,
                    MaxPoints = g.First().Activity.MaxPoints,
                    LatestSubmissionDate = g.Max(s => s.SubmittedAt),
                    LatestPoints = g.OrderByDescending(s => s.SubmittedAt).First().Points,
                    AttemptsCount = g.Count()
                })
                .OrderByDescending(s => s.LatestSubmissionDate)
                .ToList();

            return summary;
        }

        public async Task<List<Submission>> GetStudentSubmissionsByActivityAsync(int studentId, int activityId)
        {
            return await _context.Submissions
                .Where(s => s.StudentId == studentId && s.ActivityId == activityId)
                .OrderByDescending(s => s.SubmittedAt) // nejnovější nahoře
                .Include(s => s.Activity)
                .Include(s => s.StudentAnswers)
                    .ThenInclude(sa => sa.AnswerOption)
                .Include(s => s.Files)
                .ToListAsync();
        }
        public async Task<List<StudentActivitySummary>> GetStudentActivitySummariesByTeacherAsync(int studentId, int teacherId)
        {
            var summaries = await _context.Activities
                .Where(a => a.ClassTeacherSubject.TeacherId == teacherId)
                .Select(a => new StudentActivitySummary
                {
                    ActivityId = a.Id,
                    Title = a.Title,
                    MaxPoints = a.MaxPoints,
                    LatestPoints = a.Submissions
                                    .Where(s => s.StudentId == studentId)
                                    .Select(s => s.Points)
                                    .FirstOrDefault(),
                    LatestSubmissionDate = a.Submissions
                                    .Where(s => s.StudentId == studentId)
                                    .Select(s => s.SubmittedAt)
                                    .OrderByDescending(d => d)
                                    .FirstOrDefault(),
                    AttemptsCount = a.Submissions.Count(s => s.StudentId == studentId)
                })
                .ToListAsync();

            return summaries;
        }



    }
}
