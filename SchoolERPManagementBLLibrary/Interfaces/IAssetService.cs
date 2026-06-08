using SchoolERPManagementBLLibrary.DTOs.Asset;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IAssetService
{
    Task<AssetResponseDTO> AddAssetAsync(CreateAssetDTO dto, CancellationToken cancellationToken);
    Task<AssetReportResponseDTO> ReportAssetIssueAsync(AssetIssueDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetResponseDTO>> GetAssetsAsync(CancellationToken cancellationToken);
}
