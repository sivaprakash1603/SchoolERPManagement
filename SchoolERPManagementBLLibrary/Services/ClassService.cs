using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Class;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class ClassService : IClassService
{
    private readonly IRepository<int, Class> _classRepository;
    private readonly IRepository<int, Teacher> _teacherRepository;
    private readonly IMapper _mapper;

    public ClassService(IRepository<int, Class> classRepository, IRepository<int, Teacher> teacherRepository, IMapper mapper)
    {
        _classRepository = classRepository;
        _teacherRepository = teacherRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ClassResponseDTO>> GetAllClassesAsync(CancellationToken cancellationToken)
    {
        var items = await _classRepository.Query(true).ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ClassResponseDTO>>(items);
    }

    public async Task<ClassResponseDTO> CreateClassAsync(CreateClassDTO dto, CancellationToken cancellationToken)
    {
        if (dto.ClassteacherId.HasValue)
        {
            if (await _teacherRepository.GetByIdAsync(dto.ClassteacherId.Value) is null)
            {
                throw new EntityNotFoundException("Teacher", dto.ClassteacherId.Value.ToString());
            }

            bool isAlreadyClassTeacher = await _classRepository.Query(false)
                .AnyAsync(c => c.Classteacherid == dto.ClassteacherId.Value, cancellationToken);
            
            if (isAlreadyClassTeacher)
            {
                throw new BusinessRuleException("A teacher cannot be the class teacher for more than one class.");
            }
        }

        var classEntity = new Class
        {
            Classname = dto.Classname ?? string.Empty,
            Section = dto.Section ?? string.Empty,
            Classteacherid = dto.ClassteacherId
        };

        await _classRepository.AddAsync(classEntity, save: true, ct: cancellationToken);
        return _mapper.Map<ClassResponseDTO>(classEntity);
    }
}
