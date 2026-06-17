using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Asset;
using SchoolERPManagementBLLibrary.DTOs.Attendance;
using SchoolERPManagementBLLibrary.DTOs.Exam;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.DTOs.Report;
using SchoolERPManagementBLLibrary.DTOs.Report.Query;
using SchoolERPManagementBLLibrary.DTOs.StaffAttendance;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public class ReportService : IReportService
{
    private readonly IRepository<int, Feepayment> _feePaymentRepository;
    private readonly IRepository<int, Attendance> _studentAttendanceRepository;
    private readonly IRepository<int, Staffattendance> _staffAttendanceRepository;
    private readonly IRepository<int, Examresult> _examResultRepository;
    private readonly IRepository<int, Exam> _examRepository;
    private readonly IRepository<int, Asset> _assetRepository;
    private readonly IMapper _mapper;

    public ReportService(
        IRepository<int, Feepayment> feePaymentRepository,
        IRepository<int, Attendance> studentAttendanceRepository,
        IRepository<int, Staffattendance> staffAttendanceRepository,
        IRepository<int, Examresult> examResultRepository,
        IRepository<int, Exam> examRepository,
        IRepository<int, Asset> assetRepository,
        IMapper mapper)
    {
        _feePaymentRepository = feePaymentRepository;
        _studentAttendanceRepository = studentAttendanceRepository;
        _staffAttendanceRepository = staffAttendanceRepository;
        _examResultRepository = examResultRepository;
        _examRepository = examRepository;
        _assetRepository = assetRepository;
        _mapper = mapper;
    }

    public async Task<FeeCollectionReportDTO> GetFeeCollectionReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        var query = _feePaymentRepository.Query(true);

        if (startDate.HasValue)
            query = query.Where(fp => fp.Paymentdate >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(fp => fp.Paymentdate <= endDate.Value);

        var payments = await query.ToListAsync(cancellationToken);

        decimal totalCollected = payments.Sum(fp => fp.Amountpaid);
        int transactionCount = payments.Count;

        var collectionByMethod = payments
            .GroupBy(fp => string.IsNullOrEmpty(fp.Paymentmethod) ? "Unknown" : fp.Paymentmethod)
            .ToDictionary(g => g.Key, g => g.Sum(fp => fp.Amountpaid));

        return new FeeCollectionReportDTO(totalCollected, transactionCount, collectionByMethod);
    }

    public async Task<StudentAttendanceReportDTO> GetStudentAttendanceReportAsync(DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken)
    {
        IQueryable<Attendance> query = _studentAttendanceRepository.Query(true).Include(a => a.Student).ThenInclude(s => s.Studentenrollments).ThenInclude(se => se.Class);

        if (startDate.HasValue)
            query = query.Where(a => a.Date >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(a => a.Date <= endDate.Value);

        var records = await query.ToListAsync(cancellationToken);

        int presentCount = records.Count(a => a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase));
        int absentCount = records.Count(a => a.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase));
        int totalRecords = records.Count;

        double overallPercentage = totalRecords == 0 ? 0 : Math.Round((double)presentCount / totalRecords * 100, 2);

        var attendanceByClass = records
            .GroupBy(a => a.Student.Studentenrollments.FirstOrDefault()?.Class?.Classname ?? "Unknown")
            .ToDictionary(
                g => g.Key, 
                g => Math.Round((double)g.Count(a => a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase)) / g.Count() * 100, 2)
            );

        return new StudentAttendanceReportDTO(totalRecords, presentCount, absentCount, overallPercentage, attendanceByClass);
    }

    public async Task<StaffAttendanceReportDTO> GetStaffAttendanceReportAsync(DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken)
    {
        var query = _staffAttendanceRepository.Query(true);

        if (startDate.HasValue)
            query = query.Where(a => a.Date >= startDate.Value);
        
        if (endDate.HasValue)
            query = query.Where(a => a.Date <= endDate.Value);

        var records = await query.ToListAsync(cancellationToken);

        int presentCount = records.Count(a => a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase));
        int absentCount = records.Count(a => a.Status.Equals("Absent", StringComparison.OrdinalIgnoreCase));
        int totalRecords = records.Count;

        double overallPercentage = totalRecords == 0 ? 0 : Math.Round((double)presentCount / totalRecords * 100, 2);

        var attendanceByType = records
            .GroupBy(a => string.IsNullOrEmpty(a.Attendancetype) ? "Unknown" : a.Attendancetype)
            .ToDictionary(
                g => g.Key, 
                g => Math.Round((double)g.Count(a => a.Status.Equals("Present", StringComparison.OrdinalIgnoreCase)) / g.Count() * 100, 2)
            );

        return new StaffAttendanceReportDTO(totalRecords, presentCount, absentCount, overallPercentage, attendanceByType);
    }

    public async Task<ExamPerformanceReportDTO> GetExamPerformanceReportAsync(int examId, CancellationToken cancellationToken)
    {
        var exam = await _examRepository.GetByIdAsync(examId);
        if (exam == null)
            throw new EntityNotFoundException("Exam", examId.ToString());

        var results = await _examResultRepository.Query(true)
            .Include(er => er.Subject)
            .Where(er => er.Examid == examId)
            .ToListAsync(cancellationToken);

        int totalStudentsAppeared = results.Select(er => er.Studentid).Distinct().Count();
        decimal overallAverage = results.Count == 0 ? 0 : Math.Round(results.Average(er => er.Marks ?? 0), 2);

        var averageBySubject = results
            .GroupBy(er => er.Subject.Subjectname)
            .ToDictionary(
                g => g.Key, 
                g => Math.Round(g.Average(er => er.Marks ?? 0), 2)
            );

        return new ExamPerformanceReportDTO(exam.Id, exam.Examname, totalStudentsAppeared, overallAverage, averageBySubject);
    }

    public async Task<AssetInventoryReportDTO> GetAssetInventoryReportAsync(CancellationToken cancellationToken)
    {
        var assets = await _assetRepository.Query(true)
            .Include(a => a.Assettype)
            .ToListAsync(cancellationToken);

        int totalAssets = assets.Count;
        
        
        
        
        decimal totalValue = 0; 

        var statusDistribution = assets
            .GroupBy(a => string.IsNullOrEmpty(a.Status) ? "Unknown" : a.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var typeDistribution = assets
            .GroupBy(a => a.Assettype?.Typename ?? "Unknown")
            .ToDictionary(g => g.Key, g => g.Count());

        return new AssetInventoryReportDTO(totalAssets, totalValue, statusDistribution, typeDistribution);
    }

    public async Task<PagedResponse<FeePaymentResponseDTO>> QueryFeePaymentsAsync(FeePaymentQueryRequest request, CancellationToken cancellationToken)
    {
        var q = _feePaymentRepository.Query(true);

        if (request.MinAmount.HasValue) q = q.Where(t => t.Amountpaid >= request.MinAmount.Value);
        if (request.MaxAmount.HasValue) q = q.Where(t => t.Amountpaid <= request.MaxAmount.Value);
        if (request.FromDate.HasValue) q = q.Where(t => t.Paymentdate >= request.FromDate.Value);
        if (request.ToDate.HasValue) q = q.Where(t => t.Paymentdate <= request.ToDate.Value);
        if (request.StudentId.HasValue) q = q.Where(t => t.Studentid == request.StudentId.Value);
        if (!string.IsNullOrWhiteSpace(request.PaymentMethod)) q = q.Where(t => t.Paymentmethod.ToLower() == request.PaymentMethod.ToLower());

        var sortBy = (request.SortBy ?? "paymentdate").Trim().ToLowerInvariant();
        var asc = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        q = sortBy switch
        {
            "amountpaid" => asc ? q.OrderBy(t => t.Amountpaid) : q.OrderByDescending(t => t.Amountpaid),
            "studentid" => asc ? q.OrderBy(t => t.Studentid) : q.OrderByDescending(t => t.Studentid),
            "paymentmethod" => asc ? q.OrderBy(t => t.Paymentmethod) : q.OrderByDescending(t => t.Paymentmethod),
            _ => asc ? q.OrderBy(t => t.Paymentdate) : q.OrderByDescending(t => t.Paymentdate)
        };

        var totalCount = await q.CountAsync(cancellationToken);
        var pageSize = Math.Max(1, request.PageSize);
        var pageNumber = Math.Max(1, request.PageNumber);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await q.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        
        return new PagedResponse<FeePaymentResponseDTO>
        {
            Items = _mapper.Map<IEnumerable<FeePaymentResponseDTO>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<PagedResponse<AttendanceResponseDTO>> QueryStudentAttendanceAsync(StudentAttendanceQueryRequest request, CancellationToken cancellationToken)
    {
        var q = _studentAttendanceRepository.Query(true);

        if (request.FromDate.HasValue) q = q.Where(a => a.Date >= request.FromDate.Value);
        if (request.ToDate.HasValue) q = q.Where(a => a.Date <= request.ToDate.Value);
        if (request.StudentId.HasValue) q = q.Where(a => a.Studentid == request.StudentId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status)) q = q.Where(a => a.Status.ToLower() == request.Status.ToLower());

        var sortBy = (request.SortBy ?? "date").Trim().ToLowerInvariant();
        var asc = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        q = sortBy switch
        {
            "studentid" => asc ? q.OrderBy(a => a.Studentid) : q.OrderByDescending(a => a.Studentid),
            "status" => asc ? q.OrderBy(a => a.Status) : q.OrderByDescending(a => a.Status),
            _ => asc ? q.OrderBy(a => a.Date) : q.OrderByDescending(a => a.Date)
        };

        var totalCount = await q.CountAsync(cancellationToken);
        var pageSize = Math.Max(1, request.PageSize);
        var pageNumber = Math.Max(1, request.PageNumber);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await q.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResponse<AttendanceResponseDTO>
        {
            Items = _mapper.Map<IEnumerable<AttendanceResponseDTO>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<PagedResponse<StaffAttendanceResponseDTO>> QueryStaffAttendanceAsync(StaffAttendanceQueryRequest request, CancellationToken cancellationToken)
    {
        var q = _staffAttendanceRepository.Query(true);

        if (request.FromDate.HasValue) q = q.Where(a => a.Date >= request.FromDate.Value);
        if (request.ToDate.HasValue) q = q.Where(a => a.Date <= request.ToDate.Value);
        if (request.UserId.HasValue) q = q.Where(a => a.Userid == request.UserId.Value);
        if (!string.IsNullOrWhiteSpace(request.Status)) q = q.Where(a => a.Status.ToLower() == request.Status.ToLower());

        var sortBy = (request.SortBy ?? "date").Trim().ToLowerInvariant();
        var asc = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        q = sortBy switch
        {
            "userid" => asc ? q.OrderBy(a => a.Userid) : q.OrderByDescending(a => a.Userid),
            "status" => asc ? q.OrderBy(a => a.Status) : q.OrderByDescending(a => a.Status),
            _ => asc ? q.OrderBy(a => a.Date) : q.OrderByDescending(a => a.Date)
        };

        var totalCount = await q.CountAsync(cancellationToken);
        var pageSize = Math.Max(1, request.PageSize);
        var pageNumber = Math.Max(1, request.PageNumber);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await q.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResponse<StaffAttendanceResponseDTO>
        {
            Items = _mapper.Map<IEnumerable<StaffAttendanceResponseDTO>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<PagedResponse<ExamResultResponseDTO>> QueryExamResultsAsync(ExamResultQueryRequest request, CancellationToken cancellationToken)
    {
        var q = _examResultRepository.Query(true);

        if (request.ExamId.HasValue) q = q.Where(er => er.Examid == request.ExamId.Value);
        if (request.SubjectId.HasValue) q = q.Where(er => er.Subjectid == request.SubjectId.Value);
        if (request.StudentId.HasValue) q = q.Where(er => er.Studentid == request.StudentId.Value);
        if (request.MinMarks.HasValue) q = q.Where(er => er.Marks >= request.MinMarks.Value);
        if (request.MaxMarks.HasValue) q = q.Where(er => er.Marks <= request.MaxMarks.Value);

        var sortBy = (request.SortBy ?? "marks").Trim().ToLowerInvariant();
        var asc = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        q = sortBy switch
        {
            "examid" => asc ? q.OrderBy(er => er.Examid) : q.OrderByDescending(er => er.Examid),
            "subjectid" => asc ? q.OrderBy(er => er.Subjectid) : q.OrderByDescending(er => er.Subjectid),
            "studentid" => asc ? q.OrderBy(er => er.Studentid) : q.OrderByDescending(er => er.Studentid),
            _ => asc ? q.OrderBy(er => er.Marks) : q.OrderByDescending(er => er.Marks)
        };

        var totalCount = await q.CountAsync(cancellationToken);
        var pageSize = Math.Max(1, request.PageSize);
        var pageNumber = Math.Max(1, request.PageNumber);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await q.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResponse<ExamResultResponseDTO>
        {
            Items = _mapper.Map<IEnumerable<ExamResultResponseDTO>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<PagedResponse<AssetResponseDTO>> QueryAssetsAsync(AssetQueryRequest request, CancellationToken cancellationToken)
    {
        var q = _assetRepository.Query(true);

        if (!string.IsNullOrWhiteSpace(request.Status)) q = q.Where(a => a.Status.ToLower() == request.Status.ToLower());
        if (request.AssetTypeId.HasValue) q = q.Where(a => a.Assettypeid == request.AssetTypeId.Value);

        var sortBy = (request.SortBy ?? "assetname").Trim().ToLowerInvariant();
        var asc = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);

        q = sortBy switch
        {
            "status" => asc ? q.OrderBy(a => a.Status) : q.OrderByDescending(a => a.Status),
            "purchasedate" => asc ? q.OrderBy(a => a.Purchasedate) : q.OrderByDescending(a => a.Purchasedate),
            _ => asc ? q.OrderBy(a => a.Assetname) : q.OrderByDescending(a => a.Assetname)
        };

        var totalCount = await q.CountAsync(cancellationToken);
        var pageSize = Math.Max(1, request.PageSize);
        var pageNumber = Math.Max(1, request.PageNumber);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await q.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return new PagedResponse<AssetResponseDTO>
        {
            Items = _mapper.Map<IEnumerable<AssetResponseDTO>>(items),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}
