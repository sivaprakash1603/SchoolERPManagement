namespace SchoolERPManagementBLLibrary.DTOs.Report;

public record AssetInventoryReportDTO(
    int TotalAssets,
    decimal TotalValue,
    Dictionary<string, int> StatusDistribution,
    Dictionary<string, int> TypeDistribution
);
