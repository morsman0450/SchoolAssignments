namespace SchoolAssignments.Models
{
    public class SubmissionFile
    {
        public int Id { get; set; }
        public int SubmissionId { get; set; }
        public string FilePath { get; set; } = null!;
        public string FileName { get; set; } = null!;

        public Submission Submission { get; set; } = null!;
    }
}
