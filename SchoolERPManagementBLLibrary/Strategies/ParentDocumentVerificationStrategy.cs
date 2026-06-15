using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Document;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Strategies;

public class ParentDocumentVerificationStrategy : IDocumentVerificationStrategy
{
    private readonly IRepository<int, Parentdocument> _parentDocumentRepository;
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;

    public ParentDocumentVerificationStrategy(
        IRepository<int, Parentdocument> parentDocumentRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository)
    {
        _parentDocumentRepository = parentDocumentRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
    }

    public bool CanHandle(string documentType)
    {
        return documentType.Equals("parent", StringComparison.OrdinalIgnoreCase);
    }

    public async Task VerifyAsync(VerifyDocumentDTO dto, int verifyingUserId, string userRole, CancellationToken cancellationToken)
    {
        var doc = await _parentDocumentRepository.GetByIdAsync(dto.DocumentId);
        if (doc is null) throw new EntityNotFoundException("ParentDocument", dto.DocumentId.ToString());
        
        if (userRole == "Teacher")
        {
            var isAuthorized = await _studentEnrollmentRepository.Query(true)
                .Include(e => e.Class)
                .ThenInclude(c => c!.Classteacher)
                .AnyAsync(e => e.Student != null && e.Student.Parentid == doc.Parentid && e.Class != null && e.Class.Classteacher != null && e.Class.Classteacher.Userid == verifyingUserId, cancellationToken);
            
            if (!isAuthorized)
            {
                throw new BusinessRuleException("Only the class teacher of a parent's child or an admin can verify this document.");
            }
        }
        
        doc.Status = dto.Status;
        await _parentDocumentRepository.UpdateAsync(doc, save: true, ct: cancellationToken);
    }
}
