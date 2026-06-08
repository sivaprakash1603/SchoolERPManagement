using SchoolERPManagementBLLibrary.DTOs.Teacher;

namespace SchoolERPManagementBLLibrary.Interfaces;

public interface ITeacherService
{
    Task<IReadOnlyList<TeacherResponseDTO>> GetAllTeachersAsync(CancellationToken cancellationToken);
    Task<TeacherResponseDTO> GetTeacherByIdAsync(int id, CancellationToken cancellationToken);
    Task<TeacherResponseDTO> GetTeacherByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<TeacherResponseDTO> AddTeacherAsync(CreateTeacherDTO dto, CancellationToken cancellationToken);
    Task<TeacherSubjectResponseDTO> AssignSubjectAsync(AssignTeacherSubjectDTO dto, CancellationToken cancellationToken);
    Task<bool> VerifyTeacherAssignmentAsync(int userId, int teacherId, int classId, int subjectId, CancellationToken cancellationToken);
}
