using System.Collections.Generic;
using SchoolERPManagementBLLibrary.DTOs.Asset;
using SchoolERPManagementBLLibrary.DTOs.Attendance;
using SchoolERPManagementBLLibrary.DTOs.Exam;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.DTOs.StaffAttendance;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IPdfReportService
{
    byte[] GenerateFeeCollectionPdf(IReadOnlyList<FeePaymentResponseDTO> records);
    byte[] GenerateStudentAttendancePdf(IReadOnlyList<AttendanceResponseDTO> records);
    byte[] GenerateStaffAttendancePdf(IReadOnlyList<StaffAttendanceResponseDTO> records);
    byte[] GenerateExamResultsPdf(IReadOnlyList<ExamResultResponseDTO> records);
    byte[] GenerateAssetsPdf(IReadOnlyList<AssetResponseDTO> records);
}
