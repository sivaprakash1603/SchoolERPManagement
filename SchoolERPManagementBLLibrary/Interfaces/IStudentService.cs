using SchoolERPManagementBLLibrary.DTOs.Student;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface IStudentService
{
    Task<SchoolERPManagementBLLibrary.DTOs.Report.Query.PagedResponse<StudentQueryResponseDTO>> GetAllStudentsAsync(StudentQueryRequest request, CancellationToken cancellationToken);
    Task<StudentResponseDTO> GetStudentByIdAsync(int id, CancellationToken cancellationToken);
    Task<StudentResponseDTO> AddStudentAsync(CreateStudentDTO dto, CancellationToken cancellationToken);
    Task<StudentResponseDTO> UpdateStudentAsync(int id, UpdateStudentDTO dto, CancellationToken cancellationToken);
    Task DeleteStudentAsync(int id, CancellationToken cancellationToken);
    Task<int?> GetStudentIdByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<IEnumerable<StudentResponseDTO>> GetStudentsByClassIdAsync(int classId, CancellationToken cancellationToken);
    Task EnrollStudentAsync(int studentId, EnrollStudentDTO dto, CancellationToken cancellationToken);
    Task BulkEnrollStudentsAsync(BulkEnrollStudentsDTO dto, CancellationToken cancellationToken);
}
