using System;

namespace SchoolERPManagementBLLibrary.DTOs.Report.Query;

public class FeePaymentQueryRequest : PagedQueryRequest
{
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? PaymentMethod { get; set; }
    public int? StudentId { get; set; }
}
