using AutoMapper;
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
    private readonly IMapper _mapper;

    public TimetableService(
        IRepository<int, Timetable> timetableRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Subject> subjectRepository,
        IRepository<int, Teacher> teacherRepository,
        IMapper mapper)
    {
        _timetableRepository = timetableRepository;
        _classRepository = classRepository;
        _subjectRepository = subjectRepository;
        _teacherRepository = teacherRepository;
        _mapper = mapper;
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
            Dayofweek = dto.DayOfWeek!.ToLower(),
            Starttime = dto.StartTime,
            Endtime = dto.EndTime,
            Roomno = dto.RoomNo
        };

        await _timetableRepository.AddAsync(timetable, save: true, ct: cancellationToken);
        return _mapper.Map<TimetableResponseDTO>(timetable);
    }

    public async Task<IReadOnlyList<TimetableResponseDTO>> GetClassTimetableAsync(int classId, CancellationToken cancellationToken)
    {
        var items = await _timetableRepository.Query(true)
            .Where(timetable => timetable.Classid == classId)
            .OrderBy(timetable => timetable.Dayofweek)
            .ThenBy(timetable => timetable.Starttime)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TimetableResponseDTO>>(items);
    }

    public async Task<IReadOnlyList<TimetableResponseDTO>> GetTeacherTimetableAsync(int teacherId, CancellationToken cancellationToken)
    {
        var items = await _timetableRepository.Query(true)
            .Where(timetable => timetable.Teacherid == teacherId)
            .OrderBy(timetable => timetable.Dayofweek)
            .ThenBy(timetable => timetable.Starttime)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TimetableResponseDTO>>(items);
    }

    public async Task<IReadOnlyList<TeacherRequirementDTO>> GetTeacherRequirementsAsync(int periodsPerDay, int freePeriodsPerStaff, CancellationToken cancellationToken)
    {
        int maxPeriodsPerTeacher = periodsPerDay - freePeriodsPerStaff;
        if (maxPeriodsPerTeacher <= 0) 
        {
            throw new BusinessRuleException("Free periods cannot be greater than or equal to total periods per day.");
        }

        var classes = await _classRepository.Query(false)
            .Include(c => c.Classsubjects)
            .ToListAsync(cancellationToken);

        var subjects = await _subjectRepository.Query(true)
            .Include(s => s.Classsubjects)
            .Include(s => s.Teachersubjects)
            .ToListAsync(cancellationToken);

        var requirements = new List<TeacherRequirementDTO>();

        foreach (var subject in subjects)
        {
            int totalClasses = subject.Classsubjects?.Count ?? 0;
            
            double totalRequiredPeriodsPerDay = 0;
            if (subject.Classsubjects != null)
            {
                foreach (var classSubject in subject.Classsubjects)
                {
                    var cls = classes.FirstOrDefault(c => c.Id == classSubject.Classid);
                    if (cls != null && cls.Classsubjects != null && cls.Classsubjects.Count > 0)
                    {
                        totalRequiredPeriodsPerDay += (double)periodsPerDay / cls.Classsubjects.Count;
                    }
                }
            }

            int requiredTeachers = (int)Math.Ceiling(totalRequiredPeriodsPerDay / maxPeriodsPerTeacher);
            int availableTeachers = subject.Teachersubjects?.Select(ts => ts.Teacherid).Distinct().Count() ?? 0;

            string status = availableTeachers >= requiredTeachers ? "Sufficient" : "Shortage";

            requirements.Add(new TeacherRequirementDTO(
                subject.Id,
                subject.Subjectname,
                totalClasses,
                requiredTeachers,
                availableTeachers,
                status
            ));
        }

        return requirements;
    }

    public async Task<IReadOnlyList<TimetableResponseDTO>> GenerateTimetableAsync(GenerateTimetableRequestDTO request, CancellationToken cancellationToken)
    {
        var requirements = await GetTeacherRequirementsAsync(request.PeriodsPerDay, request.FreePeriodsPerStaff, cancellationToken);
        var shortages = requirements.Where(r => r.Status == "Shortage").ToList();
        if (shortages.Any())
        {
            var missingSubjects = string.Join(", ", shortages.Select(s => s.SubjectName));
            throw new BusinessRuleException($"Cannot generate timetable due to teacher shortage in the following subjects: {missingSubjects}. Please assign enough teachers.");
        }

        var classes = await _classRepository.Query(false)
            .Where(c => request.ClassIds.Contains(c.Id))
            .Include(c => c.Classsubjects).ThenInclude(cs => cs.Subject)
            .Include(c => c.Teachersubjects).ThenInclude(ts => ts.Teacher)
            .ToListAsync(cancellationToken);

        var allTeachers = await _teacherRepository.Query(false).ToListAsync(cancellationToken);
        
        var generatedTimetables = new List<Timetable>();
        string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        int maxPeriodsPerDay = request.PeriodsPerDay;
        int freePeriodsPerStaff = request.FreePeriodsPerStaff;
        int teacherMaxPeriods = maxPeriodsPerDay - freePeriodsPerStaff;

        var teacherDailyPeriods = new Dictionary<string, Dictionary<int, int>>();
        foreach (var day in daysOfWeek)
        {
            teacherDailyPeriods[day] = new Dictionary<int, int>();
        }

        int idCounter = -1;

        foreach (var day in daysOfWeek)
        {
            foreach (var classEntity in classes)
            {
                var classSubjects = classEntity.Classsubjects?.Select(cs => cs.Subject).ToList() ?? new List<Subject>();
                if (!classSubjects.Any()) continue;

                var teachersForClass = classEntity.Teachersubjects?.ToList() ?? new List<Teachersubject>();

                var shuffledSubjects = classSubjects.OrderBy(x => Guid.NewGuid()).ToList();
                int subjectIndex = 0;

                foreach (var timing in request.Timings.OrderBy(t => t.PeriodNumber))
                {
                    bool scheduled = false;
                    for (int attempt = 0; attempt < classSubjects.Count; attempt++)
                    {
                        var subject = shuffledSubjects[subjectIndex % shuffledSubjects.Count];
                        subjectIndex++;

                        var ts = teachersForClass.FirstOrDefault(t => t.Subjectid == subject.Id);
                        int teacherId = ts?.Teacherid ?? 0;

                        if (teacherId == 0) continue;

                        bool isBusy = generatedTimetables.Any(t => 
                            t.Dayofweek == day && 
                            t.Teacherid == teacherId && 
                            t.Starttime == TimeOnly.Parse(timing.StartTime)
                        );

                        if (isBusy) continue;

                        if (!teacherDailyPeriods[day].ContainsKey(teacherId))
                            teacherDailyPeriods[day][teacherId] = 0;

                        if (teacherDailyPeriods[day][teacherId] >= teacherMaxPeriods)
                            continue;

                        teacherDailyPeriods[day][teacherId]++;
                        
                        generatedTimetables.Add(new Timetable
                        {
                            Id = idCounter--,
                            Classid = classEntity.Id,
                            Class = classEntity,
                            Subjectid = subject.Id,
                            Subject = subject,
                            Teacherid = teacherId,
                            Teacher = allTeachers.FirstOrDefault(t => t.Id == teacherId),
                            Dayofweek = day,
                            Starttime = TimeOnly.Parse(timing.StartTime),
                            Endtime = TimeOnly.Parse(timing.EndTime)
                        });

                        scheduled = true;
                        break; 
                    }
                }
            }
        }

        var dtos = generatedTimetables.Select(t => new TimetableResponseDTO(
            t.Id,
            t.Classid,
            t.Subjectid,
            t.Teacherid,
            t.Dayofweek,
            t.Starttime,
            t.Endtime,
            t.Roomno
        )).ToList();

        return dtos;
    }

    public async Task SaveGeneratedTimetableAsync(IReadOnlyList<TimetableResponseDTO> generatedTimetable, CancellationToken cancellationToken)
    {
        if (generatedTimetable == null || !generatedTimetable.Any()) return;

        var classIds = generatedTimetable.Select(t => t.ClassId).Distinct().ToList();

        var existingTimetables = await _timetableRepository.Query(false)
            .Where(t => classIds.Contains(t.Classid))
            .ToListAsync(cancellationToken);

        foreach (var tt in existingTimetables)
        {
            await _timetableRepository.DeleteAsync(tt, save: false, ct: cancellationToken);
        }

        foreach (var dto in generatedTimetable)
        {
            var entity = new Timetable
            {
                Classid = dto.ClassId,
                Subjectid = dto.SubjectId,
                Teacherid = dto.TeacherId,
                Dayofweek = dto.DayOfWeek,
                Starttime = dto.StartTime,
                Endtime = dto.EndTime,
                Roomno = dto.RoomNo
            };
            await _timetableRepository.AddAsync(entity, save: false, ct: cancellationToken);
        }

        await _timetableRepository.SaveChangesAsync(cancellationToken);
    }
}
