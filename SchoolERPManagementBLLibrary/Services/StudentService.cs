using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Student;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using SchoolERPManagementBLLibrary.Helpers;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class StudentService : IStudentService
{
    private readonly IRepository<int, Student> _studentRepository;
    private readonly IRepository<int, User> _userRepository;
    private readonly IRepository<int, Parent> _parentRepository;
    private readonly IRepository<int, Role> _roleRepository;
    private readonly IRepository<int, Class> _classRepository;
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;
    private readonly IRepository<int, Academicyear> _academicYearRepository;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public StudentService(
        IRepository<int, Student> studentRepository, 
        IRepository<int, User> userRepository, 
        IRepository<int, Parent> parentRepository, 
        IRepository<int, Role> roleRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository,
        IRepository<int, Academicyear> academicYearRepository,
        IEmailService emailService,
        IMapper mapper)
    {
        _studentRepository = studentRepository;
        _userRepository = userRepository;
        _parentRepository = parentRepository;
        _roleRepository = roleRepository;
        _classRepository = classRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
        _academicYearRepository = academicYearRepository;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<StudentResponseDTO>> GetAllStudentsAsync(CancellationToken cancellationToken)
    {
        var items = await _studentRepository.Query(true).ToListAsync(cancellationToken);
        return _mapper.Map<IEnumerable<StudentResponseDTO>>(items);
    }

    public async Task<StudentResponseDTO> GetStudentByIdAsync(int id, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.Query(true).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return student is null
            ? throw new EntityNotFoundException("Student", id.ToString())
            : _mapper.Map<StudentResponseDTO>(student);
    }

    public async Task<StudentResponseDTO> AddStudentAsync(CreateStudentDTO dto, CancellationToken cancellationToken)
    {
        if (await _userRepository.Query(true).AnyAsync(x => x.Email == dto.Email, cancellationToken))
        {
            throw new DuplicateEntityException("User", "email", dto.Email);
        }

        var role = await _roleRepository.Query(true).FirstOrDefaultAsync(r => r.Rolename == "Student", cancellationToken);
        if (role is null)
        {
            throw new EntityNotFoundException("Role", "Student");
        }

        var classEntity = await _classRepository.GetByIdAsync(dto.ClassId);
        if (classEntity is null)
        {
            throw new EntityNotFoundException("Class", dto.ClassId.ToString());
        }

        var academicYear = await _academicYearRepository.GetByIdAsync(dto.AcademicYearId);
        if (academicYear is null)
        {
            throw new EntityNotFoundException("AcademicYear", dto.AcademicYearId.ToString());
        }

        if (dto.ParentId.HasValue && await _parentRepository.GetByIdAsync(dto.ParentId.Value) is null)
        {
            throw new EntityNotFoundException("Parent", dto.ParentId.Value.ToString());
        }

        int currentEnrollmentCount = await _studentEnrollmentRepository.Query(false)
            .CountAsync(e => e.Classid == dto.ClassId && e.Academicyearid == dto.AcademicYearId, cancellationToken);
        int rollNo = currentEnrollmentCount + 1;

        string generatedUsername = $"ST{rollNo:D3}{classEntity.Classname}{classEntity.Section}";
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

        var student = new Student
        {
            Userid = user.Id,
            Regno = generatedUsername,
            Name = dto.Name,
            Parentid = dto.ParentId
        };

        await _studentRepository.AddAsync(student, save: true, ct: cancellationToken);

        var enrollment = new Studentenrollment
        {
            Studentid = student.Id,
            Classid = dto.ClassId,
            Academicyearid = dto.AcademicYearId,
            Rollnumber = rollNo
        };

        await _studentEnrollmentRepository.AddAsync(enrollment, save: true, ct: cancellationToken);

        string emailBody = $@"
        <h2>Welcome to School ERP System</h2>
        <p>Dear {dto.Name},</p>
        <p>Your student account has been successfully created. Here are your login details:</p>
        <ul>
            <li><strong>Username:</strong> {generatedUsername}</li>
            <li><strong>Password:</strong> {generatedPassword}</li>
        </ul>
        <p>Please log in and change your password as soon as possible.</p>";

        try
        {
            await _emailService.SendEmailAsync(dto.Email, "Your Student Account Details", emailBody, cancellationToken);
        }
        catch
        {
            // Log email sending failure here, but don't stop the student creation process
        }

        var response = _mapper.Map<StudentResponseDTO>(student);
        return response with { GeneratedPassword = generatedPassword };
    }

    public async Task<StudentResponseDTO> UpdateStudentAsync(int id, UpdateStudentDTO dto, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student is null)
        {
            throw new EntityNotFoundException("Student", id.ToString());
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            student.Name = dto.Name;
        }

        if (!string.IsNullOrWhiteSpace(dto.Gender))
        {
            student.Gender = dto.Gender;
        }

        if (dto.ParentId.HasValue && await _parentRepository.GetByIdAsync(dto.ParentId.Value) is null)
        {
            throw new EntityNotFoundException("Parent", dto.ParentId.Value.ToString());
        }

        student.Parentid = dto.ParentId;
        await _studentRepository.UpdateAsync(student, save: true, ct: cancellationToken);

        return _mapper.Map<StudentResponseDTO>(student);
    }

    public async Task DeleteStudentAsync(int id, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(id);
        if (student is null)
        {
            throw new EntityNotFoundException("Student", id.ToString());
        }

        await _studentRepository.DeleteAsync(student, save: true, ct: cancellationToken);
    }

    public async Task<int?> GetStudentIdByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.Query(true).FirstOrDefaultAsync(x => x.Userid == userId, cancellationToken);
        return student?.Id;
    }
}
