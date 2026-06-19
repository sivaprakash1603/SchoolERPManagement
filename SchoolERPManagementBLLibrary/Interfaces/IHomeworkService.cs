using SchoolERPManagementBLLibrary.DTOs.Homework;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IHomeworkService
{
    Task<HomeworkResponseDTO> CreateHomeworkAsync(CreateHomeworkDTO dto, CancellationToken cancellationToken);
    Task<HomeworkSubmissionResponseDTO> SubmitHomeworkAsync(HomeworkSubmissionDTO dto, int? userId, string userRole, CancellationToken cancellationToken);
    Task<HomeworkSubmissionResponseDTO> EvaluateHomeworkAsync(EvaluateHomeworkDTO dto, int? userId, string userRole, CancellationToken cancellationToken);
    Task<IReadOnlyList<HomeworkResponseDTO>> GetHomeworksAsync(int classId, int? subjectId, CancellationToken cancellationToken);
    Task<IReadOnlyList<HomeworkResponseDTO>> GetHomeworksByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<HomeworkResponseDTO>> GetHomeworksByStudentIdAsync(int studentId, CancellationToken cancellationToken);
}
