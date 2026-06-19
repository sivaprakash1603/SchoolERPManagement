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
    private readonly IRepository<int, Academicyear> _academicYearRepository;
    private readonly IMapper _mapper;

    public ClassService(
        IRepository<int, Class> classRepository, 
        IRepository<int, Teacher> teacherRepository, 
        IRepository<int, Academicyear> academicYearRepository,
        IMapper mapper)
    {
        _classRepository = classRepository;
        _teacherRepository = teacherRepository;
        _academicYearRepository = academicYearRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ClassResponseDTO>> GetAllClassesAsync(int? academicYearId, CancellationToken cancellationToken)
    {
        if (!academicYearId.HasValue)
        {
            var currentYear = await _academicYearRepository.Query(false)
                .FirstOrDefaultAsync(y => y.Iscurrent == true, cancellationToken);
            if (currentYear != null)
            {
                academicYearId = currentYear.Id;
            }
        }

        var query = _classRepository.Query(true)
            .Include(c => c.Studentenrollments)
            .AsQueryable();
        if (academicYearId.HasValue)
        {
            query = query.Where(c => c.Academicyearid == academicYearId.Value);
        }

        var items = await query.ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ClassResponseDTO>>(items);
    }

    public async Task<ClassResponseDTO> CreateClassAsync(CreateClassDTO dto, CancellationToken cancellationToken)
    {
        int? academicYearId = dto.AcademicyearId;
        if (!academicYearId.HasValue)
        {
            var currentYear = await _academicYearRepository.Query(false)
                .FirstOrDefaultAsync(y => y.Iscurrent == true, cancellationToken);
            if (currentYear != null)
            {
                academicYearId = currentYear.Id;
            }
        }

        if (dto.ClassteacherId.HasValue)
        {
            if (await _teacherRepository.GetByIdAsync(dto.ClassteacherId.Value) is null)
            {
                throw new EntityNotFoundException("Teacher", dto.ClassteacherId.Value.ToString());
            }

            bool isAlreadyClassTeacher = await _classRepository.Query(false)
                .AnyAsync(c => c.Classteacherid == dto.ClassteacherId.Value && c.Academicyearid == academicYearId, cancellationToken);
            
            if (isAlreadyClassTeacher)
            {
                throw new BusinessRuleException("A teacher cannot be the class teacher for more than one class in the same academic year.");
            }
        }

        var classEntity = new Class
        {
            Classname = dto.Classname ?? string.Empty,
            Section = dto.Section ?? string.Empty,
            Classteacherid = dto.ClassteacherId,
            Academicyearid = academicYearId
        };

        await _classRepository.AddAsync(classEntity, save: true, ct: cancellationToken);
        return _mapper.Map<ClassResponseDTO>(classEntity);
    }
}
