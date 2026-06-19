using SchoolERPManagementBLLibrary.DTOs.Dashboard;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardDTO> GetAdminDashboardMetricsAsync(int? academicYearId, CancellationToken cancellationToken);
}
