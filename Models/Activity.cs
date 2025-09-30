namespace SchoolAssignments.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int MaxAttempts { get; set; } = 1;
        public int UsedAttempts { get; set; }
        public ActivityType Type { get; set; }
        public int MaxPoints { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsAutoGraded { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        public int CreatedByUserId { get; set; }
        public User CreatedByUser { get; set; } = null!;

        public int ClassTeacherSubjectId { get; set; }
        public ClassTeacherSubject ClassTeacherSubject { get; set; } = null!;

        // Judge0 nastavení
        public int? LanguageId { get; set; }
        public string? InitialCode { get; set; }
        public string? StdInput { get; set; }
        public string? ExpectedOutput { get; set; }

        // Navigace
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();
    }


    public enum ActivityType
    {
        Code = 1, Text = 2, File = 3, Image = 4, Test = 5
    }
}
