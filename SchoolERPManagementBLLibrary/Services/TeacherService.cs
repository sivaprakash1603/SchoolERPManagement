using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Teacher;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using SchoolERPManagementBLLibrary.Helpers;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class TeacherService : ITeacherService
{
    private readonly IRepository<int, Teacher> _teacherRepository;
    private readonly IRepository<int, User> _userRepository;
    private readonly IRepository<int, Subject> _subjectRepository;
    private readonly IRepository<int, Class> _classRepository;
    private readonly IRepository<int, Teachersubject> _teacherSubjectRepository;
    private readonly IRepository<int, Timetable> _timetableRepository;
    private readonly IRepository<int, Role> _roleRepository;

    public TeacherService(
        IRepository<int, Teacher> teacherRepository,
        IRepository<int, User> userRepository,
        IRepository<int, Subject> subjectRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Teachersubject> teacherSubjectRepository,
        IRepository<int, Timetable> timetableRepository,
        IRepository<int, Role> roleRepository)
    {
        _teacherRepository = teacherRepository;
        _userRepository = userRepository;
        _subjectRepository = subjectRepository;
        _classRepository = classRepository;
        _teacherSubjectRepository = teacherSubjectRepository;
        _timetableRepository = timetableRepository;
        _roleRepository = roleRepository;
    }

    public async Task<SchoolERPManagementBLLibrary.DTOs.Report.Query.PagedResponse<TeacherResponseDTO>> GetAllTeachersAsync(TeacherQueryRequest request, CancellationToken cancellationToken)
    {
        var query = _teacherRepository.Query(true)
            .Include(t => t.User)
            .Include(t => t.Classes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.ToLower();
            query = query.Where(t => (t.Name != null && t.Name.ToLower().Contains(search)) || (t.Phonenumber != null && t.Phonenumber.Contains(search)) || (t.User != null && t.User.Email != null && t.User.Email.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "All")
        {
            if (request.Status == "Active")
                query = query.Where(t => t.User.Isactive == true);
            else if (request.Status == "Inactive")
                query = query.Where(t => t.User.Isactive != true);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            bool isDesc = request.SortDirection?.ToLower() == "desc";
            query = request.SortBy.ToLower() switch
            {
                "name" => isDesc ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name),
                "joiningdate" => isDesc ? query.OrderByDescending(t => t.Joiningdate) : query.OrderBy(t => t.Joiningdate),
                _ => query.OrderByDescending(t => t.Id)
            };
        }
        else
        {
            query = query.OrderByDescending(t => t.Id);
        }

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(teacher => new TeacherResponseDTO(
            teacher.Id, 
            teacher.Userid, 
            teacher.Name, 
            teacher.Phonenumber, 
            teacher.Joiningdate, 
            teacher.Qualifications, 
            null, 
            teacher.User!.Username, 
            teacher.Classes.FirstOrDefault()?.Classname, 
            teacher.Classes.FirstOrDefault()?.Section
        )).ToList();

        return new SchoolERPManagementBLLibrary.DTOs.Report.Query.PagedResponse<TeacherResponseDTO>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    public async Task<TeacherResponseDTO> GetTeacherByIdAsync(int id, CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.Query(true).Include(t=>t.User).Include(t=>t.Classes).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return teacher is null
            ? throw new EntityNotFoundException("Teacher", id.ToString())
            : new TeacherResponseDTO(teacher.Id, teacher.Userid, teacher.Name, teacher.Phonenumber, teacher.Joiningdate, teacher.Qualifications, null, teacher.User.Username, teacher.Classes.FirstOrDefault()?.Classname, teacher.Classes.FirstOrDefault()?.Section);
    }

    public async Task<TeacherResponseDTO> GetTeacherByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.Query(true).Include(t=>t.User).Include(t=>t.Classes).FirstOrDefaultAsync(x => x.User.Username == username, cancellationToken);
        return teacher is null
            ? throw new EntityNotFoundException("Teacher", username)
            : new TeacherResponseDTO(teacher.Id, teacher.Userid, teacher.Name, teacher.Phonenumber, teacher.Joiningdate, teacher.Qualifications,null, teacher.User.Username, teacher.Classes.FirstOrDefault()?.Classname, teacher.Classes.FirstOrDefault()?.Section);
    }

    public async Task<TeacherResponseDTO> AddTeacherAsync(CreateTeacherDTO dto, CancellationToken cancellationToken)
    {
        if (await _userRepository.Query(true).AnyAsync(x => x.Email == dto.Email, cancellationToken))
        {
            throw new DuplicateEntityException("User", "email", dto.Email);
        }

        var role = await _roleRepository.Query(true).FirstOrDefaultAsync(r => r.Rolename == "Teacher", cancellationToken);
        if (role is null)
        {
            throw new EntityNotFoundException("Role", "Teacher");
        }

        int totalTeachers = await _teacherRepository.Query(false).CountAsync(cancellationToken);
        string generatedUsername = $"T{(totalTeachers + 1):D3}{DateTime.UtcNow.Year}";
        string generatedPassword = Guid.NewGuid().ToString().Substring(0, 8); // Simple random password

        var user = new User
        {
            Username = generatedUsername,
            Email = dto.Email,
            Passwordhash = PasswordHasher.Hash(generatedPassword),
            Roleid = role.Id,
            Isactive = true,
            Createdat = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, save: true, ct: cancellationToken);

        var teacher = new Teacher
        {
            Userid = user.Id,
            Name = dto.Name,
            Phonenumber = dto.Phonenumber,
            Joiningdate = DateOnly.FromDateTime(DateTime.UtcNow),
            Qualifications = dto.Qualifications
        };

        await _teacherRepository.AddAsync(teacher, save: true, ct: cancellationToken);
        return new TeacherResponseDTO(teacher.Id, teacher.Userid, teacher.Name, teacher.Phonenumber, teacher.Joiningdate, teacher.Qualifications, generatedPassword, generatedUsername,null,null);
    }

    public async Task<TeacherSubjectResponseDTO> AssignSubjectAsync(AssignTeacherSubjectDTO dto, CancellationToken cancellationToken)
    {
        if (await _teacherRepository.GetByIdAsync(dto.TeacherId) is null)
        {
            throw new EntityNotFoundException("Teacher", dto.TeacherId.ToString());
        }

        if (await _subjectRepository.GetByIdAsync(dto.SubjectId) is null)
        {
            throw new EntityNotFoundException("Subject", dto.SubjectId.ToString());
        }

        if (await _classRepository.GetByIdAsync(dto.ClassId) is null)
        {
            throw new EntityNotFoundException("Class", dto.ClassId.ToString());
        }

        var existing = await _teacherSubjectRepository.Query(true)
            .FirstOrDefaultAsync(x => x.Teacherid == dto.TeacherId && x.Subjectid == dto.SubjectId && x.Classid == dto.ClassId, cancellationToken);

        if (existing is null)
        {
            existing = new Teachersubject
            {
                Teacherid = dto.TeacherId,
                Subjectid = dto.SubjectId,
                Classid = dto.ClassId
            };

            await _teacherSubjectRepository.AddAsync(existing, save: true, ct: cancellationToken);
        }

        return new TeacherSubjectResponseDTO(existing.Id, existing.Teacherid, existing.Subjectid, existing.Classid);
    }
    public async Task<bool> VerifyTeacherAssignmentAsync(int userId, int teacherId, int classId, int subjectId, CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.GetByIdAsync(teacherId);
        if (teacher is null || teacher.Userid != userId)
        {
            return false;
        }

        bool isAssigned = await _teacherSubjectRepository.Query(true)
            .AnyAsync(x => x.Teacherid == teacherId && x.Classid == classId && x.Subjectid == subjectId, cancellationToken);

        bool isAssignedInTimetable = await _timetableRepository.Query(true)
            .AnyAsync(t => t.Teacherid == teacherId && t.Classid == classId && t.Subjectid == subjectId, cancellationToken);

        return isAssigned || isAssignedInTimetable;
    }
}
