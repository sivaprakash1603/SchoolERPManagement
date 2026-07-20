using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Document;
using SchoolERPManagementBLLibrary.DTOs.Notification;
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
    private readonly INotificationService _notificationService;
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
        INotificationService notificationService,
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
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<StudentDocumentResponseDTO> UploadStudentDocumentAsync(IFormFile file, int studentId, string? documentName, string? userRole, CancellationToken cancellationToken)
    {
        ValidateDocumentFormat(file, documentName);

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
        ValidateDocumentFormat(file, documentName);

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

        // Send notifications based on document type
        int targetUserId = 0;
        string documentName = "";
        if (dto.DocumentType.Equals("student", StringComparison.OrdinalIgnoreCase))
        {
            var doc = await _studentDocumentRepository.Query(true)
                .Include(d => d.Student)
                .FirstOrDefaultAsync(d => d.Id == dto.DocumentId, cancellationToken);
            if (doc != null && doc.Student != null)
            {
                targetUserId = doc.Student.Userid;
                documentName = doc.Documentname;
            }
        }
        else if (dto.DocumentType.Equals("teacher", StringComparison.OrdinalIgnoreCase))
        {
            var doc = await _teacherDocumentRepository.Query(true)
                .Include(d => d.Teacher)
                .FirstOrDefaultAsync(d => d.Id == dto.DocumentId, cancellationToken);
            if (doc != null && doc.Teacher != null)
            {
                targetUserId = doc.Teacher.Userid;
                documentName = doc.Documentname;
            }
        }
        else if (dto.DocumentType.Equals("parent", StringComparison.OrdinalIgnoreCase))
        {
            var doc = await _parentDocumentRepository.Query(true)
                .Include(d => d.Parent)
                .FirstOrDefaultAsync(d => d.Id == dto.DocumentId, cancellationToken);
            if (doc != null && doc.Parent != null)
            {
                targetUserId = doc.Parent.Userid;
                documentName = doc.Documentname;
            }
        }

        if (targetUserId > 0)
        {
            var notificationDto = new SendNotificationDTO(
                Title: $"Document {dto.Status}",
                Message: $"Your uploaded document '{documentName}' has been {dto.Status!.ToLower()}.",
                CreatedByUserId: verifyingUserId,
                TargetUserIds: new[] { targetUserId }
            );
            await _notificationService.SendNotificationAsync(notificationDto, cancellationToken);
        }
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

    public async Task<IReadOnlyList<PendingDocumentDTO>> GetPendingDocumentsAsync(CancellationToken cancellationToken)
    {
        var studentPendingDocs = await _studentDocumentRepository.Query(true)
            .Include(d => d.Student)
                .ThenInclude(s => s.Studentenrollments)
            .Where(d => d.Status == "Pending")
            .ToListAsync(cancellationToken);

        var teacherPendingDocs = await _teacherDocumentRepository.Query(true)
            .Include(d => d.Teacher)
            .ThenInclude(t => t.User)
            .Where(d => d.Status == "Pending")
            .ToListAsync(cancellationToken);

        var result = new List<PendingDocumentDTO>();

        foreach (var d in studentPendingDocs)
        {
            var enrollment = d.Student?.Studentenrollments?.OrderByDescending(e => e.Academicyearid).FirstOrDefault();
            result.Add(new PendingDocumentDTO(
                d.Id,
                d.Documentname,
                d.Documenttype,
                d.Bloburl,
                d.Uploadedat,
                d.Studentid,
                d.Student != null ? d.Student.FirstName + " " + d.Student.LastName : "Unknown Student",
                "Student",
                d.Student?.Regno ?? "N/A",
                enrollment?.Classid
            ));
        }

        foreach (var d in teacherPendingDocs)
        {
            result.Add(new PendingDocumentDTO(
                d.Id,
                d.Documentname,
                d.Documenttype,
                d.Bloburl,
                d.Uploadedat,
                d.Teacherid,
                d.Teacher != null ? d.Teacher.FirstName + " " + d.Teacher.LastName : "Unknown Teacher",
                "Teacher",
                d.Teacher?.User?.Username ?? "N/A"
            ));
        }

        return result;
    }

    private void ValidateDocumentFormat(IFormFile file, string? documentName)
    {
        var ext = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
        var isProfilePhoto = documentName == "Profile Photo";

        if (isProfilePhoto)
        {
            if (ext != "jpg" && ext != "jpeg")
            {
                throw new BusinessRuleException("Only JPG/JPEG files are allowed for Profile Photo.");
            }
        }
        else
        {
            if (ext != "pdf")
            {
                throw new BusinessRuleException("Only PDF files are allowed for this document type.");
            }
        }
    }
}
