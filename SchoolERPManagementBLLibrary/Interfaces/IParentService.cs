using SchoolERPManagementBLLibrary.DTOs.Parent;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IParentService
{
    Task<ParentResponseDTO> GetParentByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ChildResponseDTO>> GetChildrenAsync(int parentId, CancellationToken cancellationToken);
    Task<ParentResponseDTO> AddParentAsync(CreateParentDTO dto, CancellationToken cancellationToken);
    Task<int?> GetParentIdByUserIdAsync(int userId, CancellationToken cancellationToken);
}
