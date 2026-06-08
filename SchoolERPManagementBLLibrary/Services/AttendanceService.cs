using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Attendance;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class AttendanceService : IAttendanceService
{
    private readonly IRepository<int, Attendance> _attendanceRepository;
    private readonly IRepository<int, Student> _studentRepository;
    private readonly IRepository<int, Teacher> _teacherRepository;
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;

    public AttendanceService(
        IRepository<int, Attendance> attendanceRepository,
        IRepository<int, Student> studentRepository,
        IRepository<int, Teacher> teacherRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository)
    {
        _attendanceRepository = attendanceRepository;
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
    }

    public async Task<AttendanceResponseDTO> MarkAttendanceAsync(MarkAttendanceDTO dto, CancellationToken cancellationToken)
    {
        if (await _studentRepository.GetByIdAsync(dto.StudentId) is null)
        {
            throw new EntityNotFoundException("Student", dto.StudentId.ToString());
        }

        if (dto.MarkedByTeacherId.HasValue)
        {
            if (await _teacherRepository.GetByIdAsync(dto.MarkedByTeacherId.Value) is null)
            {
                throw new EntityNotFoundException("Teacher", dto.MarkedByTeacherId.Value.ToString());
            }

            var isClassTeacher = await _studentEnrollmentRepository.Query(true)
                .Include(e => e.Class)
                .AnyAsync(e => e.Studentid == dto.StudentId && e.Class.Classteacherid == dto.MarkedByTeacherId.Value, cancellationToken);

            if (!isClassTeacher)
            {
                throw new BusinessRuleException("Only the designated class teacher can mark attendance for this student.");
            }
        }

        var attendance = await _attendanceRepository.Query(true)
            .FirstOrDefaultAsync(x => x.Studentid == dto.StudentId && x.Date == dto.Date, cancellationToken);

        if (attendance is null)
        {
            attendance = new Attendance
            {
                Studentid = dto.StudentId,
                Date = dto.Date,
                Status = dto.Status,
                Markedbyteacherid = dto.MarkedByTeacherId,
                Remarks = dto.Remarks
            };

            await _attendanceRepository.AddAsync(attendance, save: true, ct: cancellationToken);
        }
        else
        {
            attendance.Status = dto.Status;
            attendance.Markedbyteacherid = dto.MarkedByTeacherId;
            attendance.Remarks = dto.Remarks;
            await _attendanceRepository.UpdateAsync(attendance, save: true, ct: cancellationToken);
        }

        return new AttendanceResponseDTO(attendance.Id, attendance.Studentid, attendance.Date, attendance.Status, attendance.Markedbyteacherid, attendance.Remarks);
    }

    public async Task<IReadOnlyList<AttendanceResponseDTO>> GetAttendanceByStudentAsync(int studentId, CancellationToken cancellationToken)
    {
        return await _attendanceRepository.Query(true)
            .Where(x => x.Studentid == studentId)
            .OrderByDescending(x => x.Date)
            .Select(attendance => new AttendanceResponseDTO(attendance.Id, attendance.Studentid, attendance.Date, attendance.Status, attendance.Markedbyteacherid, attendance.Remarks))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AttendanceResponseDTO>> GetAttendanceByClassAsync(int classId, DateTime date, CancellationToken cancellationToken)
    {
        var studentIds = await _studentEnrollmentRepository.Query(true)
            .Where(enrollment => enrollment.Classid == classId)
            .Select(enrollment => enrollment.Studentid)
            .ToListAsync(cancellationToken);

        var day = DateOnly.FromDateTime(date);

        return await _attendanceRepository.Query(true)
            .Where(attendance => studentIds.Contains(attendance.Studentid) && attendance.Date == day)
            .Select(attendance => new AttendanceResponseDTO(attendance.Id, attendance.Studentid, attendance.Date, attendance.Status, attendance.Markedbyteacherid, attendance.Remarks))
            .ToListAsync(cancellationToken);
    }
}
