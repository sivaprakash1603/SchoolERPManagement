namespace SchoolERPManagementBLLibrary.DTOs.Report.Query;

public abstract class PagedQueryRequest
{
    public int PageNumber { get; set; } = 1;
    
    private int _pageSize = 10;
    public int PageSize 
    { 
        get => _pageSize; 
        set => _pageSize = value > 100 ? 100 : value; 
    }
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "desc";
}
