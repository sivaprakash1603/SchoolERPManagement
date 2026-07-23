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
    private readonly IRepository<int, Staffattendance> _staffAttendanceRepository;
    private readonly IMapper _mapper;

    public TimetableService(
        IRepository<int, Timetable> timetableRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Subject> subjectRepository,
        IRepository<int, Teacher> teacherRepository,
        IRepository<int, Staffattendance> staffAttendanceRepository,
        IMapper mapper)
    {
        _timetableRepository = timetableRepository;
        _classRepository = classRepository;
        _subjectRepository = subjectRepository;
        _teacherRepository = teacherRepository;
        _staffAttendanceRepository = staffAttendanceRepository;
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

        var teacher = await _teacherRepository.Query(true)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == dto.TeacherId, cancellationToken);

        if (teacher is null)
        {
            throw new EntityNotFoundException("Teacher", dto.TeacherId.ToString());
        }

        if (teacher.User == null || teacher.User.Isactive != true)
        {
            throw new BusinessRuleException("Cannot assign a timetable slot to an inactive teacher.");
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
            .ToListAsync(cancellationToken);

        var resolved = await ResolveSubstitutionsAsync(items, cancellationToken);
        var sorted = resolved.OrderBy(timetable => timetable.Dayofweek)
            .ThenBy(timetable => timetable.Starttime)
            .ToList();

        return _mapper.Map<IReadOnlyList<TimetableResponseDTO>>(sorted);
    }

    public async Task<IReadOnlyList<TimetableResponseDTO>> GetTeacherTimetableAsync(int teacherId, CancellationToken cancellationToken)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var dayOfWeekName = date.ToDateTime(TimeOnly.MinValue).DayOfWeek.ToString().ToLower();

        // 1. Get static weekly slots for this teacher
        var mySlots = await _timetableRepository.Query(true)
            .Where(timetable => timetable.Teacherid == teacherId)
            .ToListAsync(cancellationToken);

        // 2. Resolve substitutions on my own slots (if I am absent today, remove my today-slots)
        var me = await _teacherRepository.GetByIdAsync(teacherId);
        var absentUserIds = await _staffAttendanceRepository.Query(true)
            .Where(a => a.Date == date && a.Status == "absent")
            .Select(a => a.Userid)
            .ToListAsync(cancellationToken);

        var myFinalSlots = new List<Timetable>();
        foreach (var slot in mySlots)
        {
            if (slot.Dayofweek.ToLower() == dayOfWeekName && me != null && absentUserIds.Contains(me.Userid))
            {
                continue;
            }
            myFinalSlots.Add(slot);
        }

        // 3. Find absent teachers' slots today where I am the chosen substitute
        if (me != null && !absentUserIds.Contains(me.Userid))
        {
            var otherTeachers = await _teacherRepository.Query(true)
                .Where(t => t.Id != teacherId)
                .ToListAsync(cancellationToken);

            var absentTeachers = otherTeachers.Where(t => absentUserIds.Contains(t.Userid)).ToList();
            if (absentTeachers.Any())
            {
                var absentTeacherIds = absentTeachers.Select(t => t.Id).ToList();

                var absentSlots = await _timetableRepository.Query(true)
                    .Where(t => absentTeacherIds.Contains(t.Teacherid) && t.Dayofweek.ToLower() == dayOfWeekName)
                    .OrderBy(t => t.Starttime)
                    .ToListAsync(cancellationToken);

                // Track all substitution assignments (teacherId -> list of time ranges) to prevent double-booking
                var substitutionAssignments = new Dictionary<int, List<(TimeOnly Start, TimeOnly End)>>();
                var presentTeachers = otherTeachers.Where(t => !absentUserIds.Contains(t.Userid)).Concat(new[] { me }).OrderBy(t => t.Id).ToList();

                foreach (var slot in absentSlots)
                {
                    Teacher? chosenSub = null;

                    foreach (var candidate in presentTeachers)
                    {
                        // Check DB for original timetable conflicts
                        var hasDbOverlap = await _timetableRepository.Query(true)
                            .AnyAsync(t => t.Teacherid == candidate.Id && t.Dayofweek.ToLower() == dayOfWeekName &&
                                           t.Starttime < slot.Endtime && slot.Starttime < t.Endtime,
                                      cancellationToken);

                        if (hasDbOverlap) continue;

                        // Check already-assigned substitutions for this candidate
                        var hasSubOverlap = false;
                        if (substitutionAssignments.TryGetValue(candidate.Id, out var assignedRanges))
                        {
                            hasSubOverlap = assignedRanges.Any(r => r.Start < slot.Endtime && slot.Starttime < r.End);
                        }

                        if (!hasSubOverlap)
                        {
                            chosenSub = candidate;
                            break;
                        }
                    }

                    if (chosenSub != null)
                    {
                        // Record this assignment
                        if (!substitutionAssignments.ContainsKey(chosenSub.Id))
                            substitutionAssignments[chosenSub.Id] = new List<(TimeOnly, TimeOnly)>();
                        substitutionAssignments[chosenSub.Id].Add((slot.Starttime, slot.Endtime));

                        if (chosenSub.Id == teacherId)
                        {
                            var subSlot = new Timetable
                            {
                                Id = slot.Id,
                                Classid = slot.Classid,
                                Subjectid = slot.Subjectid,
                                Teacherid = teacherId,
                                Dayofweek = slot.Dayofweek,
                                Starttime = slot.Starttime,
                                Endtime = slot.Endtime,
                                Roomno = slot.Roomno,
                                Class = slot.Class,
                                Subject = slot.Subject,
                                Teacher = me
                            };
                            myFinalSlots.Add(subSlot);
                        }
                    }
                }
            }
        }

        var sorted = myFinalSlots.OrderBy(s => s.Dayofweek).ThenBy(s => s.Starttime).ToList();
        return _mapper.Map<IReadOnlyList<TimetableResponseDTO>>(sorted);
    }

    private async Task<List<Timetable>> ResolveSubstitutionsAsync(List<Timetable> slots, CancellationToken cancellationToken)
    {
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        var dayOfWeekName = date.ToDateTime(TimeOnly.MinValue).DayOfWeek.ToString().ToLower();

        var absentUserIds = await _staffAttendanceRepository.Query(true)
            .Where(a => a.Date == date && a.Status == "absent")
            .Select(a => a.Userid)
            .ToListAsync(cancellationToken);

        if (!absentUserIds.Any()) return slots;

        var allTeachers = await _teacherRepository.Query(true).ToListAsync(cancellationToken);
        var substitutedSlots = new List<Timetable>();

        // Track substitution assignments (teacherId -> list of time ranges) to prevent double-booking
        var substitutionAssignments = new Dictionary<int, List<(TimeOnly Start, TimeOnly End)>>();

        foreach (var slot in slots)
        {
            if (slot.Dayofweek.ToLower() != dayOfWeekName)
            {
                substitutedSlots.Add(slot);
                continue;
            }

            var assignedTeacher = allTeachers.FirstOrDefault(t => t.Id == slot.Teacherid);
            if (assignedTeacher != null && absentUserIds.Contains(assignedTeacher.Userid))
            {
                Teacher? substituteTeacher = null;
                foreach (var otherTeacher in allTeachers)
                {
                    if (otherTeacher.Id == assignedTeacher.Id) continue;
                    if (absentUserIds.Contains(otherTeacher.Userid)) continue;

                    // Check DB for original timetable conflicts
                    var hasDbOverlap = await _timetableRepository.Query(true)
                        .AnyAsync(t => t.Teacherid == otherTeacher.Id && t.Dayofweek.ToLower() == dayOfWeekName &&
                                       t.Starttime < slot.Endtime && slot.Starttime < t.Endtime,
                                  cancellationToken);

                    if (hasDbOverlap) continue;

                    // Check already-assigned substitutions for this candidate
                    var hasSubOverlap = false;
                    if (substitutionAssignments.TryGetValue(otherTeacher.Id, out var assignedRanges))
                    {
                        hasSubOverlap = assignedRanges.Any(r => r.Start < slot.Endtime && slot.Starttime < r.End);
                    }

                    if (!hasSubOverlap)
                    {
                        substituteTeacher = otherTeacher;
                        break;
                    }
                }

                if (substituteTeacher != null)
                {
                    // Record this assignment
                    if (!substitutionAssignments.ContainsKey(substituteTeacher.Id))
                        substitutionAssignments[substituteTeacher.Id] = new List<(TimeOnly, TimeOnly)>();
                    substitutionAssignments[substituteTeacher.Id].Add((slot.Starttime, slot.Endtime));

                    var clone = new Timetable
                    {
                        Id = slot.Id,
                        Classid = slot.Classid,
                        Subjectid = slot.Subjectid,
                        Teacherid = substituteTeacher.Id,
                        Dayofweek = slot.Dayofweek,
                        Starttime = slot.Starttime,
                        Endtime = slot.Endtime,
                        Roomno = slot.Roomno,
                        Class = slot.Class,
                        Subject = slot.Subject,
                        Teacher = substituteTeacher
                    };
                    substitutedSlots.Add(clone);
                }
                else
                {
                    substitutedSlots.Add(slot);
                }
            }
            else
            {
                substitutedSlots.Add(slot);
            }
        }

        return substitutedSlots;
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

        var activeTeachers = await _teacherRepository.Query(false)
            .Include(t => t.User)
            .Where(t => t.User != null && t.User.Isactive == true)
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
            int availableTeachers = activeTeachers.Count(t => t.SubjectSpecialtyId == subject.Id || (subject.Teachersubjects != null && subject.Teachersubjects.Any(ts => ts.Teacherid == t.Id)));

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
            .Include(c => c.Teachersubjects).ThenInclude(ts => ts.Teacher).ThenInclude(t => t.User)
            .ToListAsync(cancellationToken);

        var allTeachers = await _teacherRepository.Query(false)
            .Include(t => t.User)
            .Where(t => t.User != null && t.User.Isactive == true)
            .ToListAsync(cancellationToken);
        
        var generatedTimetables = new List<Timetable>();
        string[] daysOfWeek = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday" };

        int maxPeriodsPerDay = request.PeriodsPerDay;
        int freePeriodsPerStaff = request.FreePeriodsPerStaff;
        int teacherMaxPeriods = maxPeriodsPerDay - freePeriodsPerStaff;

        var teacherDailyPeriods = new Dictionary<string, Dictionary<int, int>>();
        var teacherWeeklyPeriods = new Dictionary<int, int>();
        var classSubjectDailyCount = new Dictionary<string, Dictionary<int, Dictionary<int, int>>>();
        var classSubjectWeeklyCount = new Dictionary<int, Dictionary<int, int>>();

        foreach (var day in daysOfWeek)
        {
            teacherDailyPeriods[day] = new Dictionary<int, int>();
            classSubjectDailyCount[day] = new Dictionary<int, Dictionary<int, int>>();
            foreach (var classEntity in classes)
            {
                classSubjectDailyCount[day][classEntity.Id] = new Dictionary<int, int>();
            }
        }
        
        foreach (var classEntity in classes)
        {
            classSubjectWeeklyCount[classEntity.Id] = new Dictionary<int, int>();
        }

        int idCounter = -1;

        foreach (var day in daysOfWeek)
        {
            foreach (var classEntity in classes)
            {
                var classSubjects = classEntity.Classsubjects?.Select(cs => cs.Subject).ToList() ?? new List<Subject>();
                if (!classSubjects.Any()) continue;

                var teachersForClass = classEntity.Teachersubjects?.Where(ts => ts.Teacher != null && ts.Teacher.User != null && ts.Teacher.User.Isactive == true).ToList() ?? new List<Teachersubject>();

                foreach (var timing in request.Timings.OrderBy(t => t.PeriodNumber))
                {
                    // Find available subjects for this period
                    var availableSubjects = new List<Subject>();
                    
                    foreach (var subject in classSubjects)
                    {
                        var ts = teachersForClass.FirstOrDefault(t => t.Subjectid == subject.Id);
                        int teacherId = ts?.Teacherid ?? 0;
                        if (teacherId == 0) continue;

                        bool isBusy = generatedTimetables.Any(t => 
                            t.Dayofweek == day && 
                            t.Teacherid == teacherId && 
                            t.Starttime == TimeOnly.Parse(timing.StartTime)
                        );
                        if (isBusy) continue;

                        if (!teacherDailyPeriods[day].ContainsKey(teacherId)) teacherDailyPeriods[day][teacherId] = 0;
                        if (teacherDailyPeriods[day][teacherId] >= teacherMaxPeriods) continue;

                        availableSubjects.Add(subject);
                    }

                    if (!availableSubjects.Any()) continue; // No subject can be scheduled

                    // Heuristic sorting:
                    // 1. Least daily occurrences for this class
                    // 2. Least weekly occurrences for this class
                    // 3. Least daily periods for the teacher
                    // 4. Least weekly periods for the teacher
                    var bestSubject = availableSubjects.OrderBy(s => 
                    {
                        if (!classSubjectDailyCount[day][classEntity.Id].ContainsKey(s.Id)) classSubjectDailyCount[day][classEntity.Id][s.Id] = 0;
                        return classSubjectDailyCount[day][classEntity.Id][s.Id];
                    }).ThenBy(s => 
                    {
                        if (!classSubjectWeeklyCount[classEntity.Id].ContainsKey(s.Id)) classSubjectWeeklyCount[classEntity.Id][s.Id] = 0;
                        return classSubjectWeeklyCount[classEntity.Id][s.Id];
                    }).ThenBy(s => 
                    {
                        var ts = teachersForClass.FirstOrDefault(t => t.Subjectid == s.Id);
                        int tId = ts?.Teacherid ?? 0;
                        if (!teacherDailyPeriods[day].ContainsKey(tId)) return 0;
                        return teacherDailyPeriods[day][tId];
                    }).ThenBy(s => 
                    {
                        var ts = teachersForClass.FirstOrDefault(t => t.Subjectid == s.Id);
                        int tId = ts?.Teacherid ?? 0;
                        if (!teacherWeeklyPeriods.ContainsKey(tId)) return 0;
                        return teacherWeeklyPeriods[tId];
                    }).First();

                    var selectedTs = teachersForClass.FirstOrDefault(t => t.Subjectid == bestSubject.Id);
                    int selectedTeacherId = selectedTs?.Teacherid ?? 0;

                    // Update tracking counts
                    if (!classSubjectDailyCount[day][classEntity.Id].ContainsKey(bestSubject.Id)) classSubjectDailyCount[day][classEntity.Id][bestSubject.Id] = 0;
                    classSubjectDailyCount[day][classEntity.Id][bestSubject.Id]++;

                    if (!classSubjectWeeklyCount[classEntity.Id].ContainsKey(bestSubject.Id)) classSubjectWeeklyCount[classEntity.Id][bestSubject.Id] = 0;
                    classSubjectWeeklyCount[classEntity.Id][bestSubject.Id]++;

                    if (!teacherDailyPeriods[day].ContainsKey(selectedTeacherId)) teacherDailyPeriods[day][selectedTeacherId] = 0;
                    teacherDailyPeriods[day][selectedTeacherId]++;

                    if (!teacherWeeklyPeriods.ContainsKey(selectedTeacherId)) teacherWeeklyPeriods[selectedTeacherId] = 0;
                    teacherWeeklyPeriods[selectedTeacherId]++;

                    generatedTimetables.Add(new Timetable
                    {
                        Id = idCounter--,
                        Classid = classEntity.Id,
                        Class = classEntity,
                        Subjectid = bestSubject.Id,
                        Subject = bestSubject,
                        Teacherid = selectedTeacherId,
                        Teacher = allTeachers.FirstOrDefault(t => t.Id == selectedTeacherId)!,
                        Dayofweek = day,
                        Starttime = TimeOnly.Parse(timing.StartTime),
                        Endtime = TimeOnly.Parse(timing.EndTime)
                    });
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

        var teacherIds = generatedTimetable.Select(t => t.TeacherId).Distinct().ToList();
        var inactiveTeacherExists = await _teacherRepository.Query(true)
            .Include(t => t.User)
            .AnyAsync(t => teacherIds.Contains(t.Id) && (t.User == null || t.User.Isactive != true), cancellationToken);

        if (inactiveTeacherExists)
        {
            throw new BusinessRuleException("Cannot save timetable because one or more assigned teachers are inactive.");
        }

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

    public async Task<TimetableResponseDTO> UpdateTimetableAsync(int id, UpdateTimetableDTO dto, CancellationToken cancellationToken)
    {
        var slot = await _timetableRepository.GetByIdAsync(id);
        if (slot is null)
        {
            throw new EntityNotFoundException("Timetable Slot", id.ToString());
        }

        if (await _subjectRepository.GetByIdAsync(dto.SubjectId) is null)
        {
            throw new EntityNotFoundException("Subject", dto.SubjectId.ToString());
        }

        var teacher = await _teacherRepository.Query(true)
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == dto.TeacherId, cancellationToken);

        if (teacher is null)
        {
            throw new EntityNotFoundException("Teacher", dto.TeacherId.ToString());
        }

        if (teacher.User == null || teacher.User.Isactive != true)
        {
            throw new BusinessRuleException("Cannot assign a timetable slot to an inactive teacher.");
        }

        // Clash Detection (excluding current slot)
        var hasTeacherClash = await _timetableRepository.Query(true)
            .AnyAsync(item => item.Id != id && item.Teacherid == dto.TeacherId && item.Dayofweek == slot.Dayofweek && item.Starttime < slot.Endtime && slot.Starttime < item.Endtime, cancellationToken);

        if (hasTeacherClash)
        {
            throw new BusinessRuleException($"Teacher is already assigned to another class during this time slot.");
        }

        if (!string.IsNullOrWhiteSpace(dto.RoomNo))
        {
            var hasRoomClash = await _timetableRepository.Query(true)
                .AnyAsync(item => item.Id != id && item.Roomno == dto.RoomNo && item.Dayofweek == slot.Dayofweek && item.Starttime < slot.Endtime && slot.Starttime < item.Endtime, cancellationToken);

            if (hasRoomClash)
            {
                throw new BusinessRuleException($"Room {dto.RoomNo} is already occupied during this time slot.");
            }
        }

        slot.Subjectid = dto.SubjectId;
        slot.Teacherid = dto.TeacherId;
        slot.Roomno = dto.RoomNo;

        await _timetableRepository.UpdateAsync(slot, save: true, ct: cancellationToken);

        var updatedSlot = await _timetableRepository.Query(true)
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .Include(t => t.Teacher)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return _mapper.Map<TimetableResponseDTO>(updatedSlot);
    }
}
