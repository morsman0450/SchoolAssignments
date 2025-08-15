namespace SchoolAssignments.Models
{
    public class AutomationStatus
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty; // Název služby
        public bool IsRunning { get; set; } = true;
        public DateTime LastCheck { get; set; } = new DateTime(2025, 1, 1);
        public string? LastError { get; set; }
    }
}
