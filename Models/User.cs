namespace SchoolAssignments.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        // Navigační vlastnosti
        public ICollection<Class> TeacherClasses { get; set; } = new List<Class>();
        public ICollection<ClassStudent> StudentClasses { get; set; } = new List<ClassStudent>();
        public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
    public enum UserRole
    {
        Student = 1,
        Teacher = 2,
        Admin = 3
    }
}
