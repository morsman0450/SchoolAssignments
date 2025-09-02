namespace SchoolAssignments.Models
{
    public class Submission
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public int StudentId { get; set; }

        // Pro text/kód
        public string? TextContent { get; set; }

        // Pro soubor
        public string? FilePath { get; set; }
        public string? FileName { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public int AttemptNumber { get; set; }

        // Hodnocení
        public int? Points { get; set; }
        public string? Feedback { get; set; }
        public DateTime? GradedAt { get; set; }
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

        // ↓↓↓ Nové pro Code aktivitu
        public string? Code { get; set; }          // zdrojový kód
        public int? LanguageId { get; set; }       // Judge0 language id
        public string? StdInput { get; set; }      // stdin
        public string? StdOutput { get; set; } // stdout z Judge0
        public string? CompileOutput { get; set; } // compile output z Judge0
        public string? Stderr { get; set; }        // stderr z Judge0
        public int? ExitCode { get; set; }         // návratový kód

        // Navigační vlastnosti
        public Activity Activity { get; set; } = null!;
        public User Student { get; set; } = null!;
        public ICollection<StudentAnswer> StudentAnswers { get; set; } = new List<StudentAnswer>();
        public ICollection<SubmissionFile> Files { get; set; } = new List<SubmissionFile>();

    }

    public enum SubmissionStatus
    {
        Submitted = 1, // Odevzdáno
        Graded = 2,    // Opraveno
        Returned = 3   // Vráceno k přepracování
    }


}
