using SchoolAssignments.Models;

namespace SchoolAssignments.DTOs
{
    public class StudentActivityDto
    {
        public int ActivityId { get; set; }
        public string Title { get; set; } = "";
        public string SubjectName { get; set; } = "";
        public int MaxPoints { get; set; }
        public int? Points { get; set; }
        public DateTime? DueDate { get; set; }
        public SubmissionStatus Status { get; set; }
    }

}
