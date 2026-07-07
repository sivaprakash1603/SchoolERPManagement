using SchoolERPManagementBLLibrary.DTOs.Exam;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IExamService
{
    Task<ExamResponseDTO> CreateExamAsync(CreateExamDTO dto, CancellationToken cancellationToken);
    Task<ExamResultResponseDTO> PublishResultAsync(PublishResultDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExamResultResponseDTO>> GetStudentResultsAsync(int studentId, int userId, string userRole, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExamResponseDTO>> GetAllExamsAsync(int? classId, CancellationToken cancellationToken);
    Task<ExamScheduleResponseDTO> CreateExamScheduleAsync(CreateExamScheduleDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExamScheduleResponseDTO>> GetExamSchedulesByExamIdAsync(int examId, CancellationToken cancellationToken);
    Task<ExamScheduleResponseDTO> UpdateExamScheduleAsync(int scheduleId, UpdateExamScheduleDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<ExamResultResponseDTO>> GetExamResultsByClassAsync(int examId, int classId, int subjectId, CancellationToken cancellationToken);
}
