using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Document;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Strategies;

public class StudentDocumentVerificationStrategy : IDocumentVerificationStrategy
{
    private readonly IRepository<int, Studentdocument> _studentDocumentRepository;
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;

    public StudentDocumentVerificationStrategy(
        IRepository<int, Studentdocument> studentDocumentRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository)
    {
        _studentDocumentRepository = studentDocumentRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
    }

    public bool CanHandle(string documentType)
    {
        return documentType.Equals("student", StringComparison.OrdinalIgnoreCase);
    }

    public async Task VerifyAsync(VerifyDocumentDTO dto, int verifyingUserId, string userRole, CancellationToken cancellationToken)
    {
        var doc = await _studentDocumentRepository.GetByIdAsync(dto.DocumentId);
        if (doc is null) throw new EntityNotFoundException("StudentDocument", dto.DocumentId.ToString());

        if (userRole == "Teacher")
        {
            var isAuthorized = await _studentEnrollmentRepository.Query(true)
                .Include(e => e.Class)
                .ThenInclude(c => c!.Classteacher)
                .AnyAsync(e => e.Studentid == doc.Studentid && e.Class != null && e.Class.Classteacher != null && e.Class.Classteacher.Userid == verifyingUserId, cancellationToken);
            
            if (!isAuthorized)
            {
                throw new BusinessRuleException("Only the class teacher or an admin can verify this document.");
            }
        }

        doc.Status = dto.Status!.ToLower();
        await _studentDocumentRepository.UpdateAsync(doc, save: true, ct: cancellationToken);
    }
}
