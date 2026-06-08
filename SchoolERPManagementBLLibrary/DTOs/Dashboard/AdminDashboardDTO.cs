namespace SchoolERPManagementBLLibrary.DTOs.Dashboard;

public record AdminDashboardDTO(
    int TotalStudents,
    int TotalTeachers,
    int TotalParents,
    decimal TotalRevenue,
    int TotalClasses,
    int TotalAssets
);
