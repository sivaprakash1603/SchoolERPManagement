using SchoolERPManagementBLLibrary.DTOs.Attendance;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IAttendanceService
{
    Task<AttendanceResponseDTO> MarkAttendanceAsync(MarkAttendanceDTO dto, CancellationToken cancellationToken);
    Task<IReadOnlyList<AttendanceResponseDTO>> GetAttendanceByStudentAsync(int studentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AttendanceResponseDTO>> GetAttendanceByClassAsync(int classId, DateTime date, CancellationToken cancellationToken);
}
