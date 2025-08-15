namespace SchoolAssignments.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxAttempts { get; set; } = 1;
        public int ClassId { get; set; }
        public ActivityType Type { get; set; }
        public int MaxPoints { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsAutoGraded { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ↓↓↓ Přidáno pro Code aktivitu
        public int? LanguageId { get; set; }        // Judge0 language_id (např. 51 = C#, 71 = Python)
        public string? InitialCode { get; set; }    // volitelné – šablona pro studenta
        public string? StdInput { get; set; }       // vstup pro program (stdin)
        public string? ExpectedOutput { get; set; } // očekávaný výstup k porovnání

        // Navigace
        public Class Class { get; set; } = null!;
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }

    public enum ActivityType
    {
        Code = 1, Text = 2, File = 3, Image = 4, Test = 5
    }
}
