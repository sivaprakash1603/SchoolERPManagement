using SchoolERPManagementBLLibrary.DTOs.Parent;

using SchoolERPManagementBLLibrary.DTOs.Report.Query;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IParentService
{
    Task<SchoolERPManagementBLLibrary.DTOs.Report.Query.PagedResponse<ParentResponseDTO>> GetAllParentsAsync(ParentQueryRequest request, CancellationToken cancellationToken);
    Task<ParentResponseDTO> GetParentByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ChildResponseDTO>> GetChildrenAsync(int parentId, CancellationToken cancellationToken);
    Task<ParentResponseDTO> AddParentAsync(CreateParentDTO dto, CancellationToken cancellationToken);
    Task<int?> GetParentIdByUserIdAsync(int userId, CancellationToken cancellationToken);
}
