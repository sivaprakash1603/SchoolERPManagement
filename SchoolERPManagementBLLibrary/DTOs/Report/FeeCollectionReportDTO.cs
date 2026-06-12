namespace SchoolERPManagementBLLibrary.DTOs.Report;

public record FeeCollectionReportDTO(
    decimal TotalCollected,
    int TransactionCount,
    Dictionary<string, decimal> CollectionByPaymentMethod
);
