namespace SchoolAssignments.Models
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        public ICollection<ClassTeacherSubject> ClassTeacherSubjects { get; set; } = new List<ClassTeacherSubject>();
    }

}
