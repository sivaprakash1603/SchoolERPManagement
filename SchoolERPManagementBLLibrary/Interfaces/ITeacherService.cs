using SchoolERPManagementBLLibrary.DTOs.Teacher;

using SchoolERPManagementBLLibrary.DTOs.Report.Query;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface ITeacherService
{
    Task<SchoolERPManagementBLLibrary.DTOs.Report.Query.PagedResponse<TeacherResponseDTO>> GetAllTeachersAsync(TeacherQueryRequest request, CancellationToken cancellationToken);
    Task<TeacherResponseDTO> GetTeacherByIdAsync(int id, CancellationToken cancellationToken);
    Task<TeacherResponseDTO> GetTeacherByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<TeacherResponseDTO> AddTeacherAsync(CreateTeacherDTO dto, CancellationToken cancellationToken);
    Task<TeacherSubjectResponseDTO> AssignSubjectAsync(AssignTeacherSubjectDTO dto, CancellationToken cancellationToken);
    Task<IEnumerable<TeacherAssignmentDTO>> GetTeacherAssignmentsAsync(int teacherId, CancellationToken cancellationToken);
    Task DeleteTeacherAssignmentAsync(int teacherId, int classId, int subjectId, CancellationToken cancellationToken);
    Task<bool> VerifyTeacherAssignmentAsync(int userId, int teacherId, int classId, int subjectId, CancellationToken cancellationToken);
    Task<TeacherResponseDTO> UpdateTeacherAsync(int id, UpdateTeacherDTO dto, CancellationToken cancellationToken);
    Task DeleteTeacherAsync(int id, CancellationToken cancellationToken);
}
