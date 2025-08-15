namespace SchoolAssignments.Models
{
    public class StudentAnswer
    {
        public int Id { get; set; }

        public int SubmissionId { get; set; }
        public Submission Submission { get; set; } = null!;

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;

        public int AnswerOptionId { get; set; }
        public AnswerOption AnswerOption { get; set; } = null!;
    }

}
