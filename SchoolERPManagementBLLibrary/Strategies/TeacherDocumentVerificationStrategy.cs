using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Document;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Strategies;

public class TeacherDocumentVerificationStrategy : IDocumentVerificationStrategy
{
    private readonly IRepository<int, Teacherdocument> _teacherDocumentRepository;

    public TeacherDocumentVerificationStrategy(IRepository<int, Teacherdocument> teacherDocumentRepository)
    {
        _teacherDocumentRepository = teacherDocumentRepository;
    }

    public bool CanHandle(string documentType)
    {
        return documentType.Equals("teacher", StringComparison.OrdinalIgnoreCase);
    }

    public async Task VerifyAsync(VerifyDocumentDTO dto, int verifyingUserId, string userRole, CancellationToken cancellationToken)
    {
        if (userRole != "Admin")
        {
            throw new BusinessRuleException("Only admins can verify teacher documents.");
        }
        
        var doc = await _teacherDocumentRepository.GetByIdAsync(dto.DocumentId);
        if (doc is null) throw new EntityNotFoundException("TeacherDocument", dto.DocumentId.ToString());
        
        doc.Status = dto.Status!.ToLower();
        await _teacherDocumentRepository.UpdateAsync(doc, save: true, ct: cancellationToken);
    }
}
