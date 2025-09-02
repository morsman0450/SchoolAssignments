namespace SchoolAssignments.Models
{
    public class CodeActivityModel
    {
        public ActivityType Type { get; set; } = ActivityType.Code;
        public string Language { get; set; } = "csharp";
        public string InitialCode { get; set; } = "// Sem napište svůj kód...";
    }


}
