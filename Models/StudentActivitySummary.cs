namespace SchoolAssignments.Models
{
    public class StudentActivitySummary
    {
        public int ActivityId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int MaxPoints { get; set; }
        public DateTime LatestSubmissionDate { get; set; }
        public int? LatestPoints { get; set; }
        public int AttemptsCount { get; set; }
    }

}
