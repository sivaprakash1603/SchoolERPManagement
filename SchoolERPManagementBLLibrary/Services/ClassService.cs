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

    public ClassService(IRepository<int, Class> classRepository, IRepository<int, Teacher> teacherRepository)
    {
        _classRepository = classRepository;
        _teacherRepository = teacherRepository;
    }

    public async Task<IReadOnlyList<ClassResponseDTO>> GetAllClassesAsync(CancellationToken cancellationToken)
    {
        return await _classRepository.Query(true)
            .Select(classEntity => new ClassResponseDTO(classEntity.Id, classEntity.Classname, classEntity.Section, classEntity.Classteacherid))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClassResponseDTO> CreateClassAsync(CreateClassDTO dto, CancellationToken cancellationToken)
    {
        if (dto.ClassteacherId.HasValue && await _teacherRepository.GetByIdAsync(dto.ClassteacherId.Value) is null)
        {
            throw new EntityNotFoundException("Teacher", dto.ClassteacherId.Value.ToString());
        }

        var classEntity = new Class
        {
            Classname = dto.Classname,
            Section = dto.Section,
            Classteacherid = dto.ClassteacherId
        };

        await _classRepository.AddAsync(classEntity, save: true, ct: cancellationToken);
        return new ClassResponseDTO(classEntity.Id, classEntity.Classname, classEntity.Section, classEntity.Classteacherid);
    }
}
