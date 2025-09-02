using Microsoft.AspNetCore.Components.Forms;
using SchoolAssignments.Models;

namespace SchoolAssignments.Services
{
    public interface ISubmissionService
    {
        Task CreateSubmissionAsync(Submission submission, List<StudentAnswer> answers);
        Task<Submission?> GetSubmissionByIdAsync(int submissionId);
        Task<Submission> GetSubmissionWithDetailsAsync(int submissionId);
        Task<Submission> RunAndGradeSubmissionAsync(Submission submission);
        Task<List<Submission>> GetSubmissionsByActivityAsync(int activityId);

        Task UpdateSubmissionAsync(Submission submission);

        Task<Submission> UploadFilesAsync(int activityId, int studentId, List<IBrowserFile> files);

    }
}
