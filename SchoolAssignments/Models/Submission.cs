namespace SchoolAssignments.Models
{
    public class Submission
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public int StudentId { get; set; }
        public string? TextContent { get; set; } // Pro text/kód
        public string? FilePath { get; set; } // Cesta k souboru
        public string? FileName { get; set; } // Původní název souboru
        public DateTime SubmittedAt { get; set; } = new DateTime(2025, 1, 1);

        // Hodnocení
        public int? Points { get; set; }
        public string? Feedback { get; set; }
        public DateTime? GradedAt { get; set; }
        public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

        // Navigační vlastnosti
        public Activity Activity { get; set; } = null!;
        public User Student { get; set; } = null!;
    }
    public enum SubmissionStatus
    {
        Submitted = 1,    // Odevzdáno
        Graded = 2,       // Opraveno
        Returned = 3      // Vráceno k přepracování
    }
}
