using SchoolERPManagementBLLibrary.DTOs.Asset;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IAssetService
{
    Task<AssetResponseDTO> AddAssetAsync(CreateAssetDTO dto, CancellationToken cancellationToken);
    Task<AssetReportResponseDTO> ReportAssetIssueAsync(AssetIssueDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetResponseDTO>> GetAssetsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetTypeResponseDTO>> GetAssetTypesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<AssetReportResponseDTO>> GetAssetReportsAsync(CancellationToken cancellationToken);
    Task<AssetStatsDTO> GetAssetStatsAsync(CancellationToken cancellationToken);
}
