using System;
using System.Threading;
using System.Threading.Tasks;
using SchoolERPManagementBLLibrary.DTOs.Asset;
using SchoolERPManagementBLLibrary.DTOs.Attendance;
using SchoolERPManagementBLLibrary.DTOs.Exam;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.DTOs.Report;
using SchoolERPManagementBLLibrary.DTOs.Report.Query;
using SchoolERPManagementBLLibrary.DTOs.StaffAttendance;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IReportService
{
    Task<FeeCollectionReportDTO> GetFeeCollectionReportAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken);
    Task<StudentAttendanceReportDTO> GetStudentAttendanceReportAsync(DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken);
    Task<StaffAttendanceReportDTO> GetStaffAttendanceReportAsync(DateOnly? startDate, DateOnly? endDate, CancellationToken cancellationToken);
    Task<ExamPerformanceReportDTO> GetExamPerformanceReportAsync(int examId, CancellationToken cancellationToken);
    Task<AssetInventoryReportDTO> GetAssetInventoryReportAsync(CancellationToken cancellationToken);

    Task<PagedResponse<FeePaymentResponseDTO>> QueryFeePaymentsAsync(FeePaymentQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<AttendanceResponseDTO>> QueryStudentAttendanceAsync(StudentAttendanceQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<StaffAttendanceResponseDTO>> QueryStaffAttendanceAsync(StaffAttendanceQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<ExamResultResponseDTO>> QueryExamResultsAsync(ExamResultQueryRequest request, CancellationToken cancellationToken);
    Task<PagedResponse<AssetResponseDTO>> QueryAssetsAsync(AssetQueryRequest request, CancellationToken cancellationToken);
}
