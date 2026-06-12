namespace SchoolERPManagementBLLibrary.DTOs.Report.Query;

public class ExamResultQueryRequest : PagedQueryRequest
{
    public int? ExamId { get; set; }
    public int? SubjectId { get; set; }
    public int? StudentId { get; set; }
    public decimal? MinMarks { get; set; }
    public decimal? MaxMarks { get; set; }
}
