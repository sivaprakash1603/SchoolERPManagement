using SchoolERPManagementBLLibrary.DTOs.Class;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IClassService
{
    Task<IReadOnlyList<ClassResponseDTO>> GetAllClassesAsync(CancellationToken cancellationToken);
    Task<ClassResponseDTO> CreateClassAsync(CreateClassDTO dto, CancellationToken cancellationToken);
}
