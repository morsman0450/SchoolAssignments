using static SchoolAssignments.Pages.TakeTest;

namespace SchoolAssignments.ViewModels
{
    public class QuestionAnswer
    {
        public int? SelectedOptionId { get; set; }                 // SingleChoice
        public Dictionary<int, OptionAnswer> Options { get; set; } = new();  // MultipleChoice
        public string? OpenTextAnswer { get; set; }               // OpenText
    }
}
