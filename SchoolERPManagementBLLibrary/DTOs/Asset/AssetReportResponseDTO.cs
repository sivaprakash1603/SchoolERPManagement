namespace SchoolERPManagementBLLibrary.DTOs.Asset;

public record AssetReportResponseDTO(int Id, int AssetId, string Status, string Report, DateTime? CreatedAt);
