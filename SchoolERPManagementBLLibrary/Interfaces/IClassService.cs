using SchoolERPManagementBLLibrary.DTOs.Class;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IClassService
{
    Task<IReadOnlyList<ClassResponseDTO>> GetAllClassesAsync(int? academicYearId, CancellationToken cancellationToken);
    Task<ClassResponseDTO> CreateClassAsync(CreateClassDTO dto, CancellationToken cancellationToken);
    Task<ClassResponseDTO> UpdateClassAsync(int id, UpdateClassDTO dto, CancellationToken cancellationToken);
    Task DeleteClassAsync(int id, CancellationToken cancellationToken);
}
