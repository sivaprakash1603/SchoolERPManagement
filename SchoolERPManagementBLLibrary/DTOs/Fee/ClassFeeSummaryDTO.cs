namespace SchoolERPManagementBLLibrary.DTOs.Fee
{
    public record ClassFeeSummaryDTO(
        int StudentId,
        string StudentName,
        string RegNo,
        decimal TotalFeeAmount,
        decimal TotalPaid,
        decimal PendingAmount
    );
}
