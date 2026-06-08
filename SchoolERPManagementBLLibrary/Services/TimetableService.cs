using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Timetable;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class TimetableService : ITimetableService
{
    private readonly IRepository<int, Timetable> _timetableRepository;
    private readonly IRepository<int, Class> _classRepository;
    private readonly IRepository<int, Subject> _subjectRepository;
    private readonly IRepository<int, Teacher> _teacherRepository;

    public TimetableService(
        IRepository<int, Timetable> timetableRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Subject> subjectRepository,
        IRepository<int, Teacher> teacherRepository)
    {
        _timetableRepository = timetableRepository;
        _classRepository = classRepository;
        _subjectRepository = subjectRepository;
        _teacherRepository = teacherRepository;
    }

    public async Task<TimetableResponseDTO> CreateTimetableAsync(CreateTimetableDTO dto, CancellationToken cancellationToken)
    {
        if (await _classRepository.GetByIdAsync(dto.ClassId) is null)
        {
            throw new EntityNotFoundException("Class", dto.ClassId.ToString());
        }

        if (await _subjectRepository.GetByIdAsync(dto.SubjectId) is null)
        {
            throw new EntityNotFoundException("Subject", dto.SubjectId.ToString());
        }

        if (await _teacherRepository.GetByIdAsync(dto.TeacherId) is null)
        {
            throw new EntityNotFoundException("Teacher", dto.TeacherId.ToString());
        }

        var hasClassClash = await _timetableRepository.Query(true)
            .AnyAsync(item => item.Classid == dto.ClassId && item.Dayofweek == dto.DayOfWeek && item.Starttime < dto.EndTime && dto.StartTime < item.Endtime, cancellationToken);

        if (hasClassClash)
        {
            throw new BusinessRuleException("The class timetable overlaps with an existing slot.");
        }

        var hasTeacherClash = await _timetableRepository.Query(true)
            .AnyAsync(item => item.Teacherid == dto.TeacherId && item.Dayofweek == dto.DayOfWeek && item.Starttime < dto.EndTime && dto.StartTime < item.Endtime, cancellationToken);

        if (hasTeacherClash)
        {
            throw new BusinessRuleException("The teacher already has a timetable slot at this time.");
        }

        var timetable = new Timetable
        {
            Classid = dto.ClassId,
            Subjectid = dto.SubjectId,
            Teacherid = dto.TeacherId,
            Dayofweek = dto.DayOfWeek,
            Starttime = dto.StartTime,
            Endtime = dto.EndTime,
            Roomno = dto.RoomNo
        };

        await _timetableRepository.AddAsync(timetable, save: true, ct: cancellationToken);
        return new TimetableResponseDTO(timetable.Id, timetable.Classid, timetable.Subjectid, timetable.Teacherid, timetable.Dayofweek, timetable.Starttime, timetable.Endtime, timetable.Roomno);
    }

    public async Task<IReadOnlyList<TimetableResponseDTO>> GetClassTimetableAsync(int classId, CancellationToken cancellationToken)
    {
        return await _timetableRepository.Query(true)
            .Where(timetable => timetable.Classid == classId)
            .OrderBy(timetable => timetable.Dayofweek)
            .ThenBy(timetable => timetable.Starttime)
            .Select(timetable => new TimetableResponseDTO(timetable.Id, timetable.Classid, timetable.Subjectid, timetable.Teacherid, timetable.Dayofweek, timetable.Starttime, timetable.Endtime, timetable.Roomno))
            .ToListAsync(cancellationToken);
    }
}
