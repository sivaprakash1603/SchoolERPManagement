using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Subject;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public class SubjectService : ISubjectService
{
    private readonly IRepository<int, Subject> _subjectRepository;

    public SubjectService(IRepository<int, Subject> subjectRepository)
    {
        _subjectRepository = subjectRepository;
    }

    public async Task<SubjectResponseDTO> CreateSubjectAsync(CreateSubjectDTO dto, CancellationToken cancellationToken)
    {
        var existing = await _subjectRepository.Query(true)
            .FirstOrDefaultAsync(s => s.Subjectname.ToLower() == dto.SubjectName.ToLower(), cancellationToken);

        if (existing != null)
            throw new DuplicateEntityException("Subject", "SubjectName", dto.SubjectName);

        var subject = new Subject
        {
            Subjectname = dto.SubjectName
        };

        await _subjectRepository.AddAsync(subject, save: true, ct: cancellationToken);

        return new SubjectResponseDTO(subject.Id, subject.Subjectname);
    }

    public async Task<IReadOnlyList<SubjectResponseDTO>> GetAllSubjectsAsync(CancellationToken cancellationToken)
    {
        return await _subjectRepository.Query(true)
            .Select(s => new SubjectResponseDTO(s.Id, s.Subjectname))
            .ToListAsync(cancellationToken);
    }

    public async Task<SubjectResponseDTO> GetSubjectByIdAsync(int id, CancellationToken cancellationToken)
    {
        var subject = await _subjectRepository.GetByIdAsync(id);
        if (subject == null)
            throw new EntityNotFoundException("Subject", id.ToString());

        return new SubjectResponseDTO(subject.Id, subject.Subjectname);
    }

    public async Task<SubjectResponseDTO> UpdateSubjectAsync(int id, CreateSubjectDTO dto, CancellationToken cancellationToken)
    {
        var subject = await _subjectRepository.GetByIdAsync(id);
        if (subject == null)
            throw new EntityNotFoundException("Subject", id.ToString());

        var existing = await _subjectRepository.Query(true)
            .FirstOrDefaultAsync(s => s.Id != id && s.Subjectname.ToLower() == dto.SubjectName.ToLower(), cancellationToken);

        if (existing != null)
            throw new DuplicateEntityException("Subject", "SubjectName", dto.SubjectName);

        subject.Subjectname = dto.SubjectName;
        await _subjectRepository.UpdateAsync(subject, save: true, ct: cancellationToken);

        return new SubjectResponseDTO(subject.Id, subject.Subjectname);
    }

    public async Task DeleteSubjectAsync(int id, CancellationToken cancellationToken)
    {
        var subject = await _subjectRepository.GetByIdAsync(id);
        if (subject == null)
            throw new EntityNotFoundException("Subject", id.ToString());

        await _subjectRepository.DeleteAsync(subject, save: true, ct: cancellationToken);
    }
}
