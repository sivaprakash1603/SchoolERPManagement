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
    private readonly IRepository<int, Teachersubject> _teacherSubjectRepository;
    private readonly IRepository<int, Timetable> _timetableRepository;
    private readonly IMapper _mapper;

    public SubjectService(
        IRepository<int, Subject> subjectRepository,
        IRepository<int, Teachersubject> teacherSubjectRepository,
        IRepository<int, Timetable> timetableRepository,
        IMapper mapper)
    {
        _subjectRepository = subjectRepository;
        _teacherSubjectRepository = teacherSubjectRepository;
        _timetableRepository = timetableRepository;
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

    public async Task<IReadOnlyList<SubjectResponseDTO>> GetSubjectsByClassAsync(int classId, CancellationToken cancellationToken)
    {
        var teacherSubjectIds = await _teacherSubjectRepository.Query(true)
            .Where(ts => ts.Classid == classId)
            .Select(ts => ts.Subjectid)
            .ToListAsync(cancellationToken);

        var timetableSubjectIds = await _timetableRepository.Query(true)
            .Where(t => t.Classid == classId)
            .Select(t => t.Subjectid)
            .ToListAsync(cancellationToken);

        var subjectIds = teacherSubjectIds.Union(timetableSubjectIds).Distinct().ToList();

        var subjects = await _subjectRepository.Query(true)
            .Where(s => subjectIds.Contains(s.Id))
            .ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<SubjectResponseDTO>>(subjects);
    }
}
