using AutoMapper;
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
    private readonly IMapper _mapper;

    public StaffAttendanceService(IRepository<int, Staffattendance> staffAttendanceRepository, IRepository<int, User> userRepository, IMapper mapper)
    {
        _staffAttendanceRepository = staffAttendanceRepository;
        _userRepository = userRepository;
        _mapper = mapper;
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
            User = user,
            Date = dto.Date,
            Status = dto.Status,
            Attendancetype = dto.AttendanceType,
            Remarks = dto.Remarks
        };

        await _staffAttendanceRepository.AddAsync(attendance, true, cancellationToken);

        return _mapper.Map<StaffAttendanceResponseDTO>(attendance);
    }

    public async Task<IReadOnlyList<StaffAttendanceResponseDTO>> GetAttendanceByUserAsync(int userId, CancellationToken cancellationToken)
    {
        var items = await _staffAttendanceRepository.Query(true)
            .Where(a => a.Userid == userId)
            .Include(a => a.User)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<StaffAttendanceResponseDTO>>(items);
    }

    public async Task<IReadOnlyList<StaffAttendanceResponseDTO>> GetAllAttendanceByDateAsync(DateOnly date, CancellationToken cancellationToken)
    {
        var items = await _staffAttendanceRepository.Query(true)
            .Where(a => a.Date == date)
            .Include(a => a.User)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<StaffAttendanceResponseDTO>>(items);
    }
}
