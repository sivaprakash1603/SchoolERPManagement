using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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

    public DocumentService(
        IRepository<int, Studentdocument> studentDocumentRepository,
        IRepository<int, Teacherdocument> teacherDocumentRepository,
        IRepository<int, Parentdocument> parentDocumentRepository,
        IRepository<int, Student> studentRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository,
        IRepository<int, Class> classRepository,
        IFileStorageService fileStorageService,
        IRepository<int, Teacher> teacherRepository,
        IEnumerable<IDocumentVerificationStrategy> verificationStrategies)
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
    }

    public async Task<StudentDocumentResponseDTO> UploadStudentDocumentAsync(IFormFile file, int studentId, CancellationToken cancellationToken)
    {
        if (await _studentRepository.GetByIdAsync(studentId) is null)
        {
            throw new EntityNotFoundException("Student", studentId.ToString());
        }

        string fileUrl = await _fileStorageService.UploadFileAsync(file, "studentdocuments", cancellationToken);

        var document = new Studentdocument
        {
            Studentid = studentId,
            Documenttype = string.IsNullOrWhiteSpace(file.ContentType) ? Path.GetExtension(file.FileName).TrimStart('.') : file.ContentType,
            Bloburl = fileUrl,
            Uploadedat = DateTime.UtcNow
        };

        await _studentDocumentRepository.AddAsync(document, save: true, ct: cancellationToken);
        return new StudentDocumentResponseDTO(document.Id, document.Studentid, document.Documenttype, document.Bloburl, document.Uploadedat);
    }

    public async Task<TeacherDocumentResponseDTO> UploadTeacherDocumentAsync(IFormFile file, int teacherId, CancellationToken cancellationToken)
    {
        if (await _teacherRepository.GetByIdAsync(teacherId) is null)
        {
            throw new EntityNotFoundException("Teacher", teacherId.ToString());
        }

        string fileUrl = await _fileStorageService.UploadFileAsync(file, "teacherdocuments", cancellationToken);

        var document = new Teacherdocument
        {
            Teacherid = teacherId,
            Documenttype = string.IsNullOrWhiteSpace(file.ContentType) ? Path.GetExtension(file.FileName).TrimStart('.') : file.ContentType,
            Bloburl = fileUrl,
            Uploadedat = DateTime.UtcNow
        };

        await _teacherDocumentRepository.AddAsync(document, save: true, ct: cancellationToken);
        return new TeacherDocumentResponseDTO(document.Id, document.Teacherid, document.Documenttype, document.Bloburl, document.Uploadedat);
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
}
