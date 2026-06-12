using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Parent;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using SchoolERPManagementBLLibrary.Helpers;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class ParentService : IParentService
{
    private readonly IRepository<int, Parent> _parentRepository;
    private readonly IRepository<int, Student> _studentRepository;
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;
    private readonly IRepository<int, User> _userRepository;
    private readonly IRepository<int, Role> _roleRepository;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public ParentService(
        IRepository<int, Parent> parentRepository,
        IRepository<int, Student> studentRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository,
        IRepository<int, User> userRepository,
        IRepository<int, Role> roleRepository,
        IEmailService emailService,
        IMapper mapper)
    {
        _parentRepository = parentRepository;
        _studentRepository = studentRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _emailService = emailService;
        _mapper = mapper;
    }

    public async Task<ParentResponseDTO> GetParentByIdAsync(int id, CancellationToken cancellationToken)
    {
        var parent = await _parentRepository.Query(true).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return parent is null
            ? throw new EntityNotFoundException("Parent", id.ToString())
            : _mapper.Map<ParentResponseDTO>(parent);
    }

    public async Task<IReadOnlyList<ChildResponseDTO>> GetChildrenAsync(int parentId, CancellationToken cancellationToken)
    {
        if (await _parentRepository.GetByIdAsync(parentId) is null)
        {
            throw new EntityNotFoundException("Parent", parentId.ToString());
        }

        var children = await _studentRepository.Query(true)
            .Where(student => student.Parentid == parentId)
            .ToListAsync(cancellationToken);

        var studentIds = children.Select(child => child.Id).ToArray();
        var enrollments = await _studentEnrollmentRepository.Query(true)
            .Where(enrollment => studentIds.Contains(enrollment.Studentid))
            .Include(enrollment => enrollment.Class)
            .ToListAsync(cancellationToken);

        return children.Select(child =>
        {
            var enrollment = enrollments.FirstOrDefault(item => item.Studentid == child.Id);
            return new ChildResponseDTO(
                child.Id,
                child.Userid,
                child.Regno,
                child.Name,
                enrollment?.Classid,
                enrollment?.Class?.Classname,
                enrollment?.Class?.Section);
        }).ToList();
    }

    public async Task<ParentResponseDTO> AddParentAsync(CreateParentDTO dto, CancellationToken cancellationToken)
    {
        string generatedUsername = $"P{dto.Phonenumber}";

        if (await _userRepository.Query(true).AnyAsync(x => x.Email == dto.Email || x.Username == generatedUsername, cancellationToken))
        {
            throw new DuplicateEntityException("User", "email/username", dto.Email);
        }

        var role = await _roleRepository.Query(true).FirstOrDefaultAsync(r => r.Rolename == "Parent", cancellationToken);
        if (role is null)
        {
            throw new EntityNotFoundException("Role", "Parent");
        }

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

        var parent = new Parent
        {
            Userid = user.Id,
            Name = dto.Name,
            Relation = dto.Relation,
            Phonenumber = dto.Phonenumber
        };

        await _parentRepository.AddAsync(parent, save: true, ct: cancellationToken);

        string emailBody = $@"
        <h2>Welcome to School ERP System</h2>
        <p>Dear {dto.Name},</p>
        <p>Your parent account has been successfully created. Here are your login details:</p>
        <ul>
            <li><strong>Username:</strong> {generatedUsername}</li>
            <li><strong>Password:</strong> {generatedPassword}</li>
        </ul>
        <p>Please log in and change your password as soon as possible.</p>";

        try
        {
            await _emailService.SendEmailAsync(dto.Email, "Your Parent Account Details", emailBody, cancellationToken);
        }
        catch
        {
            
        }

        var response = _mapper.Map<ParentResponseDTO>(parent);
        return response with { GeneratedPassword = generatedPassword };
    }

    public async Task<int?> GetParentIdByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        var parent = await _parentRepository.Query(true).FirstOrDefaultAsync(x => x.Userid == userId, cancellationToken);
        return parent?.Id;
    }
}
