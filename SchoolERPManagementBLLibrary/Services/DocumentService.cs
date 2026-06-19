using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Document;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class DocumentService : IDocumentService
{
    private readonly IRepository<int, Studentdocument> _studentDocumentRepository;
    private readonly IRepository<int, Teacherdocument> _teacherDocumentRepository;
    private readonly IRepository<int, Parentdocument> _parentDocumentRepository;
    private readonly IRepository<int, Student> _studentRepository;
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;
    private readonly IRepository<int, Class> _classRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IRepository<int, Teacher> _teacherRepository;
    private readonly IEnumerable<IDocumentVerificationStrategy> _verificationStrategies;
    private readonly IMapper _mapper;

    public DocumentService(
        IRepository<int, Studentdocument> studentDocumentRepository,
        IRepository<int, Teacherdocument> teacherDocumentRepository,
        IRepository<int, Parentdocument> parentDocumentRepository,
        IRepository<int, Student> studentRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository,
        IRepository<int, Class> classRepository,
        IFileStorageService fileStorageService,
        IRepository<int, Teacher> teacherRepository,
        IEnumerable<IDocumentVerificationStrategy> verificationStrategies,
        IMapper mapper)
    {
        _studentDocumentRepository = studentDocumentRepository;
        _teacherDocumentRepository = teacherDocumentRepository;
        _parentDocumentRepository = parentDocumentRepository;
        _studentRepository = studentRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
        _classRepository = classRepository;
        _fileStorageService = fileStorageService;
        _teacherRepository = teacherRepository;
        _verificationStrategies = verificationStrategies;
        _mapper = mapper;
    }

    public async Task<StudentDocumentResponseDTO> UploadStudentDocumentAsync(IFormFile file, int studentId, string? documentName, string? userRole, CancellationToken cancellationToken)
    {
        if (await _studentRepository.GetByIdAsync(studentId) is null)
        {
            throw new EntityNotFoundException("Student", studentId.ToString());
        }

        string fileUrl = await _fileStorageService.UploadFileAsync(file, "studentdocuments", cancellationToken);

        var document = new Studentdocument
        {
            Studentid = studentId,
            Documentname = string.IsNullOrWhiteSpace(documentName) ? file.FileName : documentName,
            Documenttype = string.IsNullOrWhiteSpace(file.ContentType) ? Path.GetExtension(file.FileName).TrimStart('.') : file.ContentType,
            Bloburl = fileUrl,
            Uploadedat = DateTime.UtcNow,
            Status = userRole == "Admin" ? "Verified" : "Pending"
        };

        await _studentDocumentRepository.AddAsync(document, save: true, ct: cancellationToken);
        return _mapper.Map<StudentDocumentResponseDTO>(document);
    }

    public async Task<TeacherDocumentResponseDTO> UploadTeacherDocumentAsync(IFormFile file, int teacherId, string? documentName, CancellationToken cancellationToken)
    {
        if (await _teacherRepository.GetByIdAsync(teacherId) is null)
        {
            throw new EntityNotFoundException("Teacher", teacherId.ToString());
        }

        string fileUrl = await _fileStorageService.UploadFileAsync(file, "teacherdocuments", cancellationToken);

        var document = new Teacherdocument
        {
            Teacherid = teacherId,
            Documentname = string.IsNullOrWhiteSpace(documentName) ? file.FileName : documentName,
            Documenttype = string.IsNullOrWhiteSpace(file.ContentType) ? Path.GetExtension(file.FileName).TrimStart('.') : file.ContentType,
            Bloburl = fileUrl,
            Uploadedat = DateTime.UtcNow
        };

        await _teacherDocumentRepository.AddAsync(document, save: true, ct: cancellationToken);
        return _mapper.Map<TeacherDocumentResponseDTO>(document);
    }

    public async Task DeleteDocumentAsync(string blobUrl, CancellationToken cancellationToken)
    {
        var document = await _studentDocumentRepository.Query(true).FirstOrDefaultAsync(document => document.Bloburl == blobUrl, cancellationToken);
        if (document is not null)
        {
            await _studentDocumentRepository.DeleteAsync(document, save: true, ct: cancellationToken);
        }

        var teacherDoc = await _teacherDocumentRepository.Query(true).FirstOrDefaultAsync(document => document.Bloburl == blobUrl, cancellationToken);
        if (teacherDoc is not null)
        {
            await _teacherDocumentRepository.DeleteAsync(teacherDoc, save: true, ct: cancellationToken);
        }

        _fileStorageService.DeleteFile(blobUrl);
    }

    public async Task VerifyDocumentAsync(VerifyDocumentDTO dto, int verifyingUserId, string userRole, CancellationToken cancellationToken)
    {
        var strategy = _verificationStrategies.FirstOrDefault(s => s.CanHandle(dto.DocumentType));
        if (strategy == null)
        {
            throw new BusinessRuleException("Invalid document type.");
        }

        await strategy.VerifyAsync(dto, verifyingUserId, userRole, cancellationToken);
    }

    public async Task<IReadOnlyList<StudentDocumentResponseDTO>> GetStudentDocumentsAsync(int studentId, int userId, string userRole, CancellationToken cancellationToken)
    {
        if (userRole == "Student")
        {
            var student = await _studentRepository.Query(true).FirstOrDefaultAsync(s => s.Userid == userId, cancellationToken);
            if (student == null || student.Id != studentId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view these documents.");
            }
        }
        else if (userRole == "Parent")
        {
            var student = await _studentRepository.Query(true)
                .Include(s => s.Studentparents)
                .ThenInclude(sp => sp.Parent)
                .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);
            
            if (student == null || !student.Studentparents.Any(sp => sp.Parent != null && sp.Parent.Userid == userId))
            {
                throw new UnauthorizedAccessException("You are not authorized to view these documents.");
            }
        }

        var documents = await _studentDocumentRepository.Query(true)
            .Where(d => d.Studentid == studentId)
            .OrderByDescending(d => d.Uploadedat)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<StudentDocumentResponseDTO>>(documents);
    }

    public async Task<IReadOnlyList<TeacherDocumentResponseDTO>> GetTeacherDocumentsAsync(int teacherId, CancellationToken cancellationToken)
    {
        var documents = await _teacherDocumentRepository.Query(true)
            .Where(d => d.Teacherid == teacherId)
            .OrderByDescending(d => d.Uploadedat)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<TeacherDocumentResponseDTO>>(documents);
    }
}
