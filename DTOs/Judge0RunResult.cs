namespace SchoolAssignments.DTOs
{
    public class Judge0RunResult
    {
        public string? Stdout { get; set; }
        public string? Stderr { get; set; }          // runtime errors
        public string? CompileOutput { get; set; }   // compile errors
        public int? ExitCode { get; set; }
        public string StatusDescription { get; set; } = "";
    }

}
