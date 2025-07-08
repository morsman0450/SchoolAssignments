namespace SchoolAssignments.Models
{
    public class SystemLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty; // Co se stalo
        public string Details { get; set; } = string.Empty; // Podrobnosti
        public DateTime CreatedAt { get; set; } = new DateTime(2025, 1, 1);
        public int? UserId { get; set; } // Kdo to udělal

        public User? User { get; set; }
    }
}
