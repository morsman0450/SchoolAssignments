namespace SchoolAssignments.Models
{
    public class ClassTeacherSubject
    {
        public int Id { get; set; }

        public int ClassId { get; set; }
        public int TeacherId { get; set; }
        public int SubjectId { get; set; }

        public Class Class { get; set; } = null!;
        public User Teacher { get; set; } = null!;
        public Subject Subject { get; set; } = null!;

        public ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }

}
