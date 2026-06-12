using AutoMapper;
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
    private readonly IMapper _mapper;

    public SubjectService(IRepository<int, Subject> subjectRepository, IMapper mapper)
    {
        _subjectRepository = subjectRepository;
        _mapper = mapper;
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

        return _mapper.Map<SubjectResponseDTO>(subject);
    }

    public async Task<IReadOnlyList<SubjectResponseDTO>> GetAllSubjectsAsync(CancellationToken cancellationToken)
    {
        var items = await _subjectRepository.Query(true).ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<SubjectResponseDTO>>(items);
    }

    public async Task<SubjectResponseDTO> GetSubjectByIdAsync(int id, CancellationToken cancellationToken)
    {
        var subject = await _subjectRepository.GetByIdAsync(id);
        if (subject == null)
            throw new EntityNotFoundException("Subject", id.ToString());

        return _mapper.Map<SubjectResponseDTO>(subject);
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

        return _mapper.Map<SubjectResponseDTO>(subject);
    }

    public async Task DeleteSubjectAsync(int id, CancellationToken cancellationToken)
    {
        var subject = await _subjectRepository.GetByIdAsync(id);
        if (subject == null)
            throw new EntityNotFoundException("Subject", id.ToString());

        await _subjectRepository.DeleteAsync(subject, save: true, ct: cancellationToken);
    }
}
