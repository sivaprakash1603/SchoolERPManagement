using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Dashboard;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IRepository<int, Student> _studentRepository;
    private readonly IRepository<int, Teacher> _teacherRepository;
    private readonly IRepository<int, Parent> _parentRepository;
    private readonly IRepository<int, Feepayment> _feePaymentRepository;
    private readonly IRepository<int, Class> _classRepository;
    private readonly IRepository<int, Asset> _assetRepository;
    private readonly IRepository<int, Attendance> _attendanceRepository;
    private readonly IRepository<int, Staffattendance> _staffAttendanceRepository;

    public DashboardService(
        IRepository<int, Student> studentRepository,
        IRepository<int, Teacher> teacherRepository,
        IRepository<int, Parent> parentRepository,
        IRepository<int, Feepayment> feePaymentRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Asset> assetRepository,
        IRepository<int, Attendance> attendanceRepository,
        IRepository<int, Staffattendance> staffAttendanceRepository)
    {
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _parentRepository = parentRepository;
        _feePaymentRepository = feePaymentRepository;
        _classRepository = classRepository;
        _assetRepository = assetRepository;
        _attendanceRepository = attendanceRepository;
        _staffAttendanceRepository = staffAttendanceRepository;
    }

    public async Task<AdminDashboardDTO> GetAdminDashboardMetricsAsync(CancellationToken cancellationToken)
    {
        var totalStudents = await _studentRepository.Query(true).CountAsync(cancellationToken);
        var totalTeachers = await _teacherRepository.Query(true).CountAsync(cancellationToken);
        var totalParents = await _parentRepository.Query(true).CountAsync(cancellationToken);
        var feePaymentsQuery = _feePaymentRepository.Query(true);
        var totalRevenue = await feePaymentsQuery.SumAsync(f => f.Amountpaid, cancellationToken);
        var totalClasses = await _classRepository.Query(true).CountAsync(cancellationToken);
        var totalAssets = await _assetRepository.Query(true).CountAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var studentAttendanceRecords = await _attendanceRepository.Query(true)
            .Where(a => a.Date == today)
            .ToListAsync(cancellationToken);
        
        double studentAttendanceRate = 0;
        if (totalStudents > 0)
        {
            var presentCount = studentAttendanceRecords.Count(a => a.Status.ToLower() == "present");
            studentAttendanceRate = Math.Round((double)presentCount / totalStudents * 100, 1);
        }

        var staffAttendanceRecords = await _staffAttendanceRepository.Query(true)
            .Where(a => a.Date == today)
            .ToListAsync(cancellationToken);
        
        var totalStaff = totalTeachers;
        double staffAttendanceRate = 0;
        if (totalStaff > 0)
        {
            var presentCount = staffAttendanceRecords.Count(a => a.Status.ToLower() == "present");
            staffAttendanceRate = Math.Round((double)presentCount / totalStaff * 100, 1);
        }

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
        var paymentsLast6Months = await feePaymentsQuery
            .Where(f => f.Paymentdate >= sixMonthsAgo)
            .ToListAsync(cancellationToken);

        var revenueTrends = paymentsLast6Months
            .Where(p => p.Paymentdate.HasValue)
            .GroupBy(p => new { p.Paymentdate!.Value.Year, p.Paymentdate!.Value.Month })
            .Select(g => new
            {
                Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                Amount = g.Sum(p => p.Amountpaid)
            })
            .OrderBy(x => x.Date)
            .Select(x => new MonthlyRevenueDTO(x.Date.ToString("MMM"), x.Amount))
            .ToList();

        if (!revenueTrends.Any())
        {
            for(int i = 5; i >= 0; i--)
            {
                revenueTrends.Add(new MonthlyRevenueDTO(DateTime.UtcNow.AddMonths(-i).ToString("MMM"), 0));
            }
        }

        var recentTransactions = await feePaymentsQuery
            .Include(f => f.Student)
            .OrderByDescending(f => f.Paymentdate)
            .Take(5)
            .Select(f => new RecentTransactionDTO(
                f.Student != null ? f.Student.Name : "Unknown",
                f.Amountpaid,
                f.Paymentdate ?? DateTime.UtcNow,
                "Completed"
            ))
            .ToListAsync(cancellationToken);

        return new AdminDashboardDTO(
            totalStudents,
            totalTeachers,
            totalParents,
            totalRevenue,
            totalClasses,
            totalAssets,
            studentAttendanceRate,
            staffAttendanceRate,
            revenueTrends,
            recentTransactions
        );
    }
}
