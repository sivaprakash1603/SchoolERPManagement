using System;

namespace SchoolERPManagementBLLibrary.DTOs.Report.Query;

public class StaffAttendanceQueryRequest : PagedQueryRequest
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? Status { get; set; }
    public int? UserId { get; set; }
}
