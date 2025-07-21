namespace SchoolAssignments.Models
{
    public class Activity
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int ClassId { get; set; }
        public ActivityType Type { get; set; }
        public int MaxPoints { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsAutoGraded { get; set; } = false; // Automatické vs manuální opravování
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigační vlastnosti
        public Class Class { get; set; } = null!;
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
        public ICollection<Question> Questions { get; set; } = new List<Question>();

    }
    public enum ActivityType
    {
        Code = 1,      // Kód - může být automaticky opravován
        Text = 2,      // Text - může být automaticky opravován
        File = 3,      // Soubor - manuální opravování
        Image = 4,     // Obrázek - manuální opravování
        Test = 5        
    }
}
