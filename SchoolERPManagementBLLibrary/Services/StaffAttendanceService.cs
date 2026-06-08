using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.StaffAttendance;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public class StaffAttendanceService : IStaffAttendanceService
{
    private readonly IRepository<int, Staffattendance> _staffAttendanceRepository;
    private readonly IRepository<int, User> _userRepository;

    public StaffAttendanceService(IRepository<int, Staffattendance> staffAttendanceRepository, IRepository<int, User> userRepository)
    {
        _staffAttendanceRepository = staffAttendanceRepository;
        _userRepository = userRepository;
    }

    public async Task<StaffAttendanceResponseDTO> MarkAttendanceAsync(StaffAttendanceRequestDTO dto, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null)
            throw new EntityNotFoundException("User", dto.UserId.ToString());

        var existing = await _staffAttendanceRepository.Query(true)
            .FirstOrDefaultAsync(a => a.Userid == dto.UserId && a.Date == dto.Date, cancellationToken);

        if (existing != null)
            throw new DuplicateEntityException("StaffAttendance", "Date", dto.Date.ToString());

        var attendance = new Staffattendance
        {
            Userid = dto.UserId,
            Date = dto.Date,
            Status = dto.Status,
            Attendancetype = dto.AttendanceType,
            Remarks = dto.Remarks
        };

        await _staffAttendanceRepository.AddAsync(attendance, true, cancellationToken);

        return new StaffAttendanceResponseDTO(
            attendance.Id,
            attendance.Userid,
            user.Username,
            attendance.Date,
            attendance.Status,
            attendance.Attendancetype,
            attendance.Remarks
        );
    }

    public async Task<IReadOnlyList<StaffAttendanceResponseDTO>> GetAttendanceByUserAsync(int userId, CancellationToken cancellationToken)
    {
        return await _staffAttendanceRepository.Query(true)
            .Where(a => a.Userid == userId)
            .Select(a => new StaffAttendanceResponseDTO(
                a.Id,
                a.Userid,
                a.User.Username,
                a.Date,
                a.Status,
                a.Attendancetype,
                a.Remarks
            ))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StaffAttendanceResponseDTO>> GetAllAttendanceByDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        return await _staffAttendanceRepository.Query(true)
            .Where(a => a.Date == date)
            .Select(a => new StaffAttendanceResponseDTO(
                a.Id,
                a.Userid,
                a.User.Username,
                a.Date,
                a.Status,
                a.Attendancetype,
                a.Remarks
            ))
            .ToListAsync(cancellationToken);
    }
}
