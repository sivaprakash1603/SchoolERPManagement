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

    public async Task<SchoolERPManagementBLLibrary.DTOs.Report.Query.PagedResponse<StudentQueryResponseDTO>> GetAllStudentsAsync(StudentQueryRequest request, CancellationToken cancellationToken)
    {
        var query = _studentRepository.Query(true)
            .Include(s => s.User)
            .Include(s => s.Studentparents).ThenInclude(sp => sp.Parent)
            .Include(s => s.Studentenrollments).ThenInclude(e => e.Class)
            .Include(s => s.Studentdocuments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchQuery))
        {
            var search = request.SearchQuery.ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(search) || s.Regno.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Gender) && request.Gender != "Any Gender")
        {
            query = query.Where(s => s.Gender == request.Gender);
        }

        int? academicYearId = request.AcademicYearId;
        if (!academicYearId.HasValue)
        {
            var currentYear = await _academicYearRepository.Query(false)
                .FirstOrDefaultAsync(y => y.Iscurrent == true, cancellationToken);
            if (currentYear != null)
            {
                academicYearId = currentYear.Id;
            }
        }

        if (academicYearId.HasValue)
        {
            if (request.ClassId.HasValue)
            {
                query = query.Where(s => s.Studentenrollments.Any(e => e.Classid == request.ClassId.Value && e.Academicyearid == academicYearId.Value));
            }
            else
            {
                query = query.Where(s => 
                    s.Studentenrollments.Any(e => e.Academicyearid == academicYearId.Value) ||
                    (s.User.Isactive == true && !s.Studentenrollments.Any(e => e.Academicyearid == academicYearId.Value))
                );
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Status) && request.Status != "All")
        {
            if (request.Status == "Active")
                query = query.Where(s => s.User.Isactive == true);
            else if (request.Status == "Inactive")
                query = query.Where(s => s.User.Isactive != true);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            bool isDesc = request.SortDirection?.ToLower() == "desc";
            query = request.SortBy.ToLower() switch
            {
                "name" => isDesc ? query.OrderByDescending(s => s.Name) : query.OrderBy(s => s.Name),
                "regno" => isDesc ? query.OrderByDescending(s => s.Regno) : query.OrderBy(s => s.Regno),
                "admissiondate" => isDesc ? query.OrderByDescending(s => s.Admissiondate) : query.OrderBy(s => s.Admissiondate),
                _ => query.OrderByDescending(s => s.Id)
            };
        }
        else
        {
            query = query.OrderByDescending(s => s.Id);
        }

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = items.Select(s => 
        {
            var enrollment = s.Studentenrollments.FirstOrDefault(e => e.Academicyearid == academicYearId);
            if (enrollment == null && !request.AcademicYearId.HasValue)
            {
                enrollment = s.Studentenrollments.OrderByDescending(e => e.Academicyearid).FirstOrDefault();
            }
            var parentNames = s.Studentparents.Select(sp => sp.Parent?.Name).Where(n => !string.IsNullOrEmpty(n));
            var photoDoc = s.Studentdocuments?.FirstOrDefault(d => d.Documentname == "Photo");
            var parentsInfo = s.Studentparents.Select(sp => new ParentSelectionDTO(sp.Parentid, sp.Relation)).ToList();
            return new StudentQueryResponseDTO(
                s.Id,
                s.Userid,
                s.Regno,
                s.Name,
                parentNames.Any() ? string.Join(", ", parentNames) : null,
                enrollment?.Class?.Classname,
                enrollment?.Class?.Section,
                s.Gender ?? "Unknown",
                s.Admissiondate,
                s.User.Isactive == true ? "Active" : "Inactive",
                photoDoc?.Bloburl,
                s.Bloodgroup,
                s.Dateofbirth,
                parentsInfo,
                enrollment?.Classid
            );
        }).ToList();

        return new SchoolERPManagementBLLibrary.DTOs.Report.Query.PagedResponse<StudentQueryResponseDTO>
        {
            Items = dtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
        };
    }

    public async Task<StudentResponseDTO> GetStudentByIdAsync(int id, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.Query(true)
            .Include(x => x.Studentenrollments)
            .Include(x => x.Studentdocuments)
            .Include(x => x.Studentparents)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
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

        if (dto.Parents != null && dto.Parents.Any())
        {
            foreach (var p in dto.Parents)
            {
                if (await _parentRepository.GetByIdAsync(p.ParentId) is null)
                {
                    throw new EntityNotFoundException("Parent", p.ParentId.ToString());
                }
            }
        }

        int totalStudentsCount = await _studentRepository.Query(false).CountAsync(cancellationToken);
        int studentIndex = totalStudentsCount + 1;
        int admissionYear = dto.Admissiondate?.Year ?? DateTime.UtcNow.Year;

        string generatedUsername = $"ST{admissionYear}{studentIndex:D4}";
        string generatedPassword = Guid.NewGuid().ToString().Substring(0, 8); 

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
            Gender = dto.Gender,
            Bloodgroup = dto.Bloodgroup,
            Dateofbirth = dto.Dateofbirth,
            Admissiondate = dto.Admissiondate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            Studentparents = dto.Parents != null && dto.Parents.Any()
                ? dto.Parents.Select((p, index) => new Studentparent 
                  { 
                      Parentid = p.ParentId, 
                      Relation = p.Relation, 
                      Isprimarycontact = index == 0 
                  }).ToList()
                : new List<Studentparent>()
        };

        await _studentRepository.AddAsync(student, save: true, ct: cancellationToken);

        int currentEnrollmentCount = await _studentEnrollmentRepository.Query(false)
            .CountAsync(e => e.Classid == dto.ClassId && e.Academicyearid == dto.AcademicYearId, cancellationToken);
        int rollNo = currentEnrollmentCount + 1;

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
            
        }

        var response = _mapper.Map<StudentResponseDTO>(student);
        return response with { GeneratedPassword = generatedPassword };
    }

    public async Task<StudentResponseDTO> UpdateStudentAsync(int id, UpdateStudentDTO dto, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.Query(false).Include(s => s.Studentparents).FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
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

        if (!string.IsNullOrWhiteSpace(dto.Bloodgroup))
        {
            student.Bloodgroup = dto.Bloodgroup;
        }

        if (dto.Dateofbirth.HasValue)
        {
            student.Dateofbirth = dto.Dateofbirth.Value;
        }

        if (dto.Admissiondate.HasValue)
        {
            student.Admissiondate = dto.Admissiondate.Value;
        }

        if (dto.Parents != null && dto.Parents.Any())
        {
            foreach (var p in dto.Parents)
            {
                if (await _parentRepository.GetByIdAsync(p.ParentId) is null)
                {
                    throw new EntityNotFoundException("Parent", p.ParentId.ToString());
                }
            }
        }

        if (dto.Parents != null)
        {
            student.Studentparents.Clear(); 
            var newParents = dto.Parents.Select((p, index) => new Studentparent 
            { 
                Parentid = p.ParentId, 
                Relation = p.Relation, 
                Isprimarycontact = index == 0 
            }).ToList();
            
            foreach (var sp in newParents)
            {
                student.Studentparents.Add(sp);
            }
        }
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

        var user = await _userRepository.GetByIdAsync(student.Userid);
        if (user != null)
        {
            user.Isactive = false;
            await _userRepository.UpdateAsync(user, save: true, ct: cancellationToken);
        }
    }

    public async Task<int?> GetStudentIdByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.Query(true).FirstOrDefaultAsync(x => x.Userid == userId, cancellationToken);
        return student?.Id;
    }

    public async Task<IEnumerable<StudentResponseDTO>> GetStudentsByClassIdAsync(int classId, CancellationToken cancellationToken)
    {
        var enrollments = await _studentEnrollmentRepository.Query(true)
            .Include(e => e.Student)
            .Where(e => e.Classid == classId)
            .ToListAsync(cancellationToken);
            
        var students = enrollments.Select(e => e.Student).Where(s => s != null).ToList();
        return _mapper.Map<IEnumerable<StudentResponseDTO>>(students);
    }

    public async Task EnrollStudentAsync(int studentId, EnrollStudentDTO dto, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.GetByIdAsync(studentId);
        if (student is null)
        {
            throw new EntityNotFoundException("Student", studentId.ToString());
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

        bool isAlreadyEnrolled = await _studentEnrollmentRepository.Query(false)
            .AnyAsync(e => e.Studentid == studentId && e.Academicyearid == dto.AcademicYearId, cancellationToken);
        if (isAlreadyEnrolled)
        {
            throw new BusinessRuleException("The student is already enrolled in a class for this academic year.");
        }

        int currentEnrollmentCount = await _studentEnrollmentRepository.Query(false)
            .CountAsync(e => e.Classid == dto.ClassId && e.Academicyearid == dto.AcademicYearId, cancellationToken);
        int rollNo = currentEnrollmentCount + 1;

        var enrollment = new Studentenrollment
        {
            Studentid = studentId,
            Classid = dto.ClassId,
            Academicyearid = dto.AcademicYearId,
            Rollnumber = rollNo
        };

        await _studentEnrollmentRepository.AddAsync(enrollment, save: true, ct: cancellationToken);
    }

    public async Task BulkEnrollStudentsAsync(BulkEnrollStudentsDTO dto, CancellationToken cancellationToken)
    {
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

        int currentEnrollmentCount = await _studentEnrollmentRepository.Query(false)
            .CountAsync(e => e.Classid == dto.ClassId && e.Academicyearid == dto.AcademicYearId, cancellationToken);

        var enrollmentsToAdd = new List<Studentenrollment>();
        foreach (var studentId in dto.StudentIds)
        {
            bool isAlreadyEnrolled = await _studentEnrollmentRepository.Query(false)
                .AnyAsync(e => e.Studentid == studentId && e.Academicyearid == dto.AcademicYearId, cancellationToken);
            if (isAlreadyEnrolled)
            {
                continue;
            }

            currentEnrollmentCount++;
            enrollmentsToAdd.Add(new Studentenrollment
            {
                Studentid = studentId,
                Classid = dto.ClassId,
                Academicyearid = dto.AcademicYearId,
                Rollnumber = currentEnrollmentCount
            });
        }

        for (int i = 0; i < enrollmentsToAdd.Count; i++)
        {
            bool save = (i == enrollmentsToAdd.Count - 1);
            await _studentEnrollmentRepository.AddAsync(enrollmentsToAdd[i], save: save, ct: cancellationToken);
        }
    }

    public async Task<StudentStatsDTO> GetStudentStatsAsync(CancellationToken cancellationToken)
    {
        var allStudentsQuery = await _studentRepository.ListAsync(cancellationToken);
        var usersQuery = await _userRepository.ListAsync(cancellationToken);
        
        var students = allStudentsQuery.ToList();
        var users = usersQuery.ToList();

        int activeStudents = 0;
        int inactiveStudents = 0;

        foreach (var student in students)
        {
            var user = users.FirstOrDefault(u => u.Id == student.Userid);
            if (user != null && user.Isactive == true)
            {
                activeStudents++;
            }
            else
            {
                inactiveStudents++;
            }
        }

        return new StudentStatsDTO
        {
            TotalStudents = students.Count,
            ActiveStudents = activeStudents,
            InactiveStudents = inactiveStudents,
            Boys = students.Count(s => s.Gender == "Male"),
            Girls = students.Count(s => s.Gender == "Female")
        };
    }
}
