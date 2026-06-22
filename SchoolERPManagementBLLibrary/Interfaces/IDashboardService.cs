using SchoolERPManagementBLLibrary.DTOs.Dashboard;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardDTO> GetAdminDashboardMetricsAsync(int? academicYearId, CancellationToken cancellationToken);
    Task<TeacherDashboardDTO> GetTeacherDashboardMetricsAsync(int teacherId, int? academicYearId, CancellationToken cancellationToken);
}
