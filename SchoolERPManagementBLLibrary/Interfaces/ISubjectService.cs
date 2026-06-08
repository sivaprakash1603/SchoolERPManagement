using SchoolERPManagementBLLibrary.DTOs.Subject;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface ISubjectService
{
    Task<SubjectResponseDTO> CreateSubjectAsync(CreateSubjectDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<SubjectResponseDTO>> GetAllSubjectsAsync(CancellationToken cancellationToken);
    Task<SubjectResponseDTO> GetSubjectByIdAsync(int id, CancellationToken cancellationToken);
    Task<SubjectResponseDTO> UpdateSubjectAsync(int id, CreateSubjectDTO dto, CancellationToken cancellationToken);
    Task DeleteSubjectAsync(int id, CancellationToken cancellationToken);
}
