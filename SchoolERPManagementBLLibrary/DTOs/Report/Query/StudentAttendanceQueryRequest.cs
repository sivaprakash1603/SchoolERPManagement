using System;

namespace SchoolERPManagementBLLibrary.DTOs.Report.Query;

public class StudentAttendanceQueryRequest : PagedQueryRequest
{
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public string? Status { get; set; }
    public int? StudentId { get; set; }
    public int? ClassId { get; set; }
}
