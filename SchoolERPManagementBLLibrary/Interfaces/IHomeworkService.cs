using SchoolERPManagementBLLibrary.DTOs.Homework;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IHomeworkService
{
    Task<HomeworkResponseDTO> CreateHomeworkAsync(CreateHomeworkDTO dto, CancellationToken cancellationToken);
    Task<HomeworkSubmissionResponseDTO> SubmitHomeworkAsync(HomeworkSubmissionDTO dto, CancellationToken cancellationToken);
    Task<HomeworkSubmissionResponseDTO> EvaluateHomeworkAsync(EvaluateHomeworkDTO dto, CancellationToken cancellationToken);
}
