namespace SchoolAssignments.Models
{
    public class ClassStudent
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int StudentId { get; set; }
        public DateTime EnrolledAt { get; set; } = new DateTime(2025, 1, 1);

        // Navigační vlastnosti
        public Class Class { get; set; } = null!;
        public User Student { get; set; } = null!;
    }
}
