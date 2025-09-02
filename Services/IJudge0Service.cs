using SchoolAssignments.DTOs;

namespace SchoolAssignments.Services
{
    public interface IJudge0Service
    {
        Task<Judge0RunResult> RunCodeAsync(int languageId, string sourceCode, string? stdin);
    }
}
