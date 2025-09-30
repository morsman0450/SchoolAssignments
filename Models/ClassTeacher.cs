namespace SchoolAssignments.Models
{
    public class ClassTeacher
    {
        public int Id { get; set; }
        public int ClassId { get; set; }
        public int TeacherId { get; set; }

        public Class Class { get; set; } = null!;
        public User Teacher { get; set; } = null!;
    }

}
