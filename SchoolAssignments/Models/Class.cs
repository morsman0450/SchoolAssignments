namespace SchoolAssignments.Models
{
    public class Class
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty; // Předmět jako string
        public int TeacherId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = new DateTime(2025, 1, 1);

        // Navigační vlastnosti
        public User Teacher { get; set; } = null!;
        public ICollection<ClassStudent> Students { get; set; } = new List<ClassStudent>();
        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }
}
