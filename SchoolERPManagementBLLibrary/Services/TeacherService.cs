using AutoMapper;
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
    private readonly IRepository<int, Role> _roleRepository;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public TeacherService(
        IRepository<int, Teacher> teacherRepository,
        IRepository<int, User> userRepository,
        IRepository<int, Subject> subjectRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Teachersubject> teacherSubjectRepository,
        IRepository<int, Role> roleRepository,
        IEmailService emailService,
        IMapper mapper)
    {
        _teacherRepository = teacherRepository;
        _userRepository = userRepository;
        _subjectRepository = subjectRepository;
        _classRepository = classRepository;
        _teacherSubjectRepository = teacherSubjectRepository;
        _roleRepository = roleRepository;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<TeacherResponseDTO>> GetAllTeachersAsync(CancellationToken cancellationToken)
    {
        var items = await _teacherRepository.Query(true)
            .Include(t => t.User)
            .Include(t => t.Classes)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<TeacherResponseDTO>>(items);
    }

    public async Task<TeacherResponseDTO> GetTeacherByIdAsync(int id, CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.Query(true).Include(t=>t.User).Include(t=>t.Classes).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return teacher is null
            ? throw new EntityNotFoundException("Teacher", id.ToString())
            : _mapper.Map<TeacherResponseDTO>(teacher);
    }

    public async Task<TeacherResponseDTO> GetTeacherByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.Query(true).Include(t=>t.User).Include(t=>t.Classes).FirstOrDefaultAsync(x => x.User.Username == username, cancellationToken);
        return teacher is null
            ? throw new EntityNotFoundException("Teacher", username)
            : _mapper.Map<TeacherResponseDTO>(teacher);
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
        string generatedUsername = $"T{(totalTeachers + 1):D3}{DateTime.Now.Year}";
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
            Joiningdate = DateOnly.FromDateTime(DateTime.Now),
            Qualifications = dto.Qualifications
        };

        await _teacherRepository.AddAsync(teacher, save: true, ct: cancellationToken);

        string emailBody = $@"
        <h2>Welcome to School ERP System</h2>
        <p>Dear {dto.Name},</p>
        <p>Your teacher account has been successfully created. Here are your login details:</p>
        <ul>
            <li><strong>Username:</strong> {generatedUsername}</li>
            <li><strong>Password:</strong> {generatedPassword}</li>
        </ul>
        <p>Please log in and change your password as soon as possible.</p>";

        try
        {
            await _emailService.SendEmailAsync(dto.Email, "Your Teacher Account Details", emailBody, cancellationToken);
        }
        catch
        {
            // Log email sending failure here
            
        }

        var response = _mapper.Map<TeacherResponseDTO>(teacher);
        return response with { GeneratedPassword = generatedPassword, Username = generatedUsername };
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

        return _mapper.Map<TeacherSubjectResponseDTO>(existing);
    }
    public async Task<bool> VerifyTeacherAssignmentAsync(int userId, int teacherId, int classId, int subjectId, CancellationToken cancellationToken)
    {
        var teacher = await _teacherRepository.GetByIdAsync(teacherId);
        if (teacher is null || teacher.Userid != userId)
        {
            return false;
        }

        return await _teacherSubjectRepository.Query(true)
            .AnyAsync(x => x.Teacherid == teacherId && x.Classid == classId && x.Subjectid == subjectId, cancellationToken);
    }
}
