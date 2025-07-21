using SchoolAssignments.Models;

namespace SchoolAssignments.Services
{
    public interface ISubmissionService
    {
        Task CreateSubmissionAsync(Submission submission, List<StudentAnswer> answers);
        Task<Submission?> GetSubmissionByIdAsync(int submissionId);
    }
}
