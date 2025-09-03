namespace SchoolAssignments.Models
{
    public class StudentAnswer
    {
        public int Id { get; set; }

        public int SubmissionId { get; set; }
        public Submission Submission { get; set; } = null!;

        public int QuestionId { get; set; }
        public Question Question { get; set; } = null!;
        public string? OpenTextAnswer { get; set; } // OpenText

        // Pokud student odpovídá výběrem z možností
        public int? AnswerOptionId { get; set; }
        public AnswerOption? AnswerOption { get; set; }

        // Pokud student odevzdá text (krátká odpověď, esej, otevřená otázka)
        public string? TextAnswer { get; set; }

        // Pokud student odevzdá kód (programovací úloha)
        public string? CodeAnswer { get; set; }

        // Můžeš doplnit i bodování jednotlivých odpovědí
        public int? PointsAwarded { get; set; }
    }
}
