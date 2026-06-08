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

    public DashboardService(
        IRepository<int, Student> studentRepository,
        IRepository<int, Teacher> teacherRepository,
        IRepository<int, Parent> parentRepository,
        IRepository<int, Feepayment> feePaymentRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Asset> assetRepository)
    {
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _parentRepository = parentRepository;
        _feePaymentRepository = feePaymentRepository;
        _classRepository = classRepository;
        _assetRepository = assetRepository;
    }

    public async Task<AdminDashboardDTO> GetAdminDashboardMetricsAsync(CancellationToken cancellationToken)
    {
        var totalStudents = await _studentRepository.Query(true).CountAsync(cancellationToken);
        var totalTeachers = await _teacherRepository.Query(true).CountAsync(cancellationToken);
        var totalParents = await _parentRepository.Query(true).CountAsync(cancellationToken);
        var totalRevenue = await _feePaymentRepository.Query(true).SumAsync(f => f.Amountpaid, cancellationToken);
        var totalClasses = await _classRepository.Query(true).CountAsync(cancellationToken);
        var totalAssets = await _assetRepository.Query(true).CountAsync(cancellationToken);

        return new AdminDashboardDTO(
            totalStudents,
            totalTeachers,
            totalParents,
            totalRevenue,
            totalClasses,
            totalAssets
        );
    }
}
