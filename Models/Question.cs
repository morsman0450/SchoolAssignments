namespace SchoolAssignments.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int ActivityId { get; set; }
        public string Text { get; set; } = string.Empty;
        public QuestionType Type { get; set; }

        public string? CorrectAnswerText { get; set; }

        public Activity Activity { get; set; } = null!;
        public ICollection<AnswerOption> Options { get; set; } = new List<AnswerOption>();
    }
    public enum QuestionType
    {
        SingleChoice = 1,    // Radio button
        MultipleChoice = 2,  // Checkboxy
        OpenText = 3         // Textové pole
    }

}
