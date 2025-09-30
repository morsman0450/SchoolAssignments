namespace SchoolAssignments.Models
{
    public class Class
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty; // Předmět jako string
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }

        // Navigační vlastnosti
        public ICollection<ClassStudent> Students { get; set; } = new List<ClassStudent>();
        public ICollection<ClassTeacherSubject> ClassTeacherSubjects { get; set; } = new List<ClassTeacherSubject>();
        public ICollection<Activity> Activities { get; set; } = new List<Activity>(); // ← Přidáno
        public ICollection<ClassTeacher> ClassTeachers { get; set; } = new List<ClassTeacher>();


    }
}
