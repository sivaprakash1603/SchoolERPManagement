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
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;
    private readonly IRepository<int, Teachersubject> _teacherSubjectRepository;
    private readonly IRepository<int, Timetable> _timetableRepository;
    private readonly IRepository<int, Homework> _homeworkRepository;
    private readonly IRepository<int, Feestructure> _feeStructureRepository;
    private readonly IRepository<int, Examschedule> _examScheduleRepository;
    private readonly IRepository<int, Asset> _assetRepository;
    private readonly IRepository<int, Classsubject> _classSubjectRepository;
    private readonly IMapper _mapper;

    public ClassService(
        IRepository<int, Class> classRepository, 
        IRepository<int, Teacher> teacherRepository, 
        IRepository<int, Academicyear> academicYearRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository,
        IRepository<int, Teachersubject> teacherSubjectRepository,
        IRepository<int, Timetable> timetableRepository,
        IRepository<int, Homework> homeworkRepository,
        IRepository<int, Feestructure> feeStructureRepository,
        IRepository<int, Examschedule> examScheduleRepository,
        IRepository<int, Asset> assetRepository,
        IRepository<int, Classsubject> classSubjectRepository,
        IMapper mapper)
    {
        _classRepository = classRepository;
        _teacherRepository = teacherRepository;
        _academicYearRepository = academicYearRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
        _teacherSubjectRepository = teacherSubjectRepository;
        _timetableRepository = timetableRepository;
        _homeworkRepository = homeworkRepository;
        _feeStructureRepository = feeStructureRepository;
        _examScheduleRepository = examScheduleRepository;
        _assetRepository = assetRepository;
        _classSubjectRepository = classSubjectRepository;
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
            .Include(c => c.Classsubjects)
                .ThenInclude(cs => cs.Subject)
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

        bool classExists = await _classRepository.Query(false)
            .AnyAsync(c => c.Classname != null && c.Section != null
                           && c.Classname.ToLower() == (dto.Classname ?? "").ToLower() 
                           && c.Section.ToLower() == (dto.Section ?? "").ToLower() 
                           && c.Academicyearid == academicYearId, cancellationToken);
        if (classExists)
        {
            throw new BusinessRuleException("A class with the same name and section already exists in this academic year.");
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

        if (dto.SubjectIds != null && dto.SubjectIds.Any())
        {
            foreach (var subjectId in dto.SubjectIds)
            {
                await _classSubjectRepository.AddAsync(new Classsubject
                {
                    Classid = classEntity.Id,
                    Subjectid = subjectId
                }, save: false, ct: cancellationToken);
            }
            await _classSubjectRepository.SaveChangesAsync(cancellationToken);
            
            // Reload with includes
            classEntity = await _classRepository.Query(true)
                .Include(c => c.Classsubjects)
                    .ThenInclude(cs => cs.Subject)
                .FirstOrDefaultAsync(c => c.Id == classEntity.Id, cancellationToken);
        }

        return _mapper.Map<ClassResponseDTO>(classEntity);
    }

    public async Task<ClassResponseDTO> UpdateClassAsync(int id, UpdateClassDTO dto, CancellationToken cancellationToken)
    {
        var classEntity = await _classRepository.Query(false)
            .Include(c => c.Studentenrollments)
            .Include(c => c.Classsubjects)
                .ThenInclude(cs => cs.Subject)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (classEntity is null)
        {
            throw new EntityNotFoundException("Class", id.ToString());
        }

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

        bool classExists = await _classRepository.Query(false)
            .AnyAsync(c => c.Id != id 
                           && c.Classname != null && c.Section != null
                           && c.Classname.ToLower() == (dto.Classname ?? "").ToLower() 
                           && c.Section.ToLower() == (dto.Section ?? "").ToLower() 
                           && c.Academicyearid == academicYearId, cancellationToken);
        if (classExists)
        {
            throw new BusinessRuleException("A class with the same name and section already exists in this academic year.");
        }

        if (dto.ClassteacherId.HasValue && dto.ClassteacherId.Value != classEntity.Classteacherid)
        {
            if (await _teacherRepository.GetByIdAsync(dto.ClassteacherId.Value) is null)
            {
                throw new EntityNotFoundException("Teacher", dto.ClassteacherId.Value.ToString());
            }

            bool isAlreadyClassTeacher = await _classRepository.Query(false)
                .AnyAsync(c => c.Id != id && c.Classteacherid == dto.ClassteacherId.Value && c.Academicyearid == academicYearId, cancellationToken);
            
            if (isAlreadyClassTeacher)
            {
                throw new BusinessRuleException("A teacher cannot be the class teacher for more than one class in the same academic year.");
            }
        }

        classEntity.Classname = dto.Classname ?? string.Empty;
        classEntity.Section = dto.Section ?? string.Empty;
        classEntity.Classteacherid = dto.ClassteacherId;
        classEntity.Academicyearid = academicYearId;

        await _classRepository.UpdateAsync(classEntity, save: true, ct: cancellationToken);

        if (dto.SubjectIds != null)
        {
            var existingSubjects = await _classSubjectRepository.Query(false)
                .Where(cs => cs.Classid == classEntity.Id)
                .ToListAsync(cancellationToken);
            
            foreach (var es in existingSubjects)
            {
                await _classSubjectRepository.DeleteAsync(es, save: false, ct: cancellationToken);
            }

            foreach (var subjectId in dto.SubjectIds)
            {
                await _classSubjectRepository.AddAsync(new Classsubject
                {
                    Classid = classEntity.Id,
                    Subjectid = subjectId
                }, save: false, ct: cancellationToken);
            }
            await _classSubjectRepository.SaveChangesAsync(cancellationToken);
            
            classEntity = await _classRepository.Query(true)
                .Include(c => c.Classsubjects)
                    .ThenInclude(cs => cs.Subject)
                .FirstOrDefaultAsync(c => c.Id == classEntity.Id, cancellationToken);
        }

        return _mapper.Map<ClassResponseDTO>(classEntity);
    }

    public async Task DeleteClassAsync(int id, CancellationToken cancellationToken)
    {
        var classEntity = await _classRepository.Query(false)
            .Include(c => c.Studentenrollments)
            .Include(c => c.Teachersubjects)
            .Include(c => c.Timetables)
            .Include(c => c.Homeworks)
            .Include(c => c.Feestructures)
            .Include(c => c.Examschedules)
            .Include(c => c.Assets)
            .Include(c => c.Classsubjects)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (classEntity is null)
        {
            throw new EntityNotFoundException("Class", id.ToString());
        }

        if (classEntity.Studentenrollments != null)
        {
            foreach (var se in classEntity.Studentenrollments.ToList())
            {
                await _studentEnrollmentRepository.DeleteAsync(se, save: false, ct: cancellationToken);
            }
        }

        if (classEntity.Classsubjects != null)
        {
            foreach (var cs in classEntity.Classsubjects.ToList())
            {
                await _classSubjectRepository.DeleteAsync(cs, save: false, ct: cancellationToken);
            }
        }

        if (classEntity.Teachersubjects != null)
        {
            foreach (var ts in classEntity.Teachersubjects.ToList())
            {
                await _teacherSubjectRepository.DeleteAsync(ts, save: false, ct: cancellationToken);
            }
        }

        if (classEntity.Timetables != null)
        {
            foreach (var tt in classEntity.Timetables.ToList())
            {
                await _timetableRepository.DeleteAsync(tt, save: false, ct: cancellationToken);
            }
        }

        if (classEntity.Homeworks != null)
        {
            foreach (var hw in classEntity.Homeworks.ToList())
            {
                await _homeworkRepository.DeleteAsync(hw, save: false, ct: cancellationToken);
            }
        }

        if (classEntity.Feestructures != null)
        {
            foreach (var fs in classEntity.Feestructures.ToList())
            {
                await _feeStructureRepository.DeleteAsync(fs, save: false, ct: cancellationToken);
            }
        }

        if (classEntity.Examschedules != null)
        {
            foreach (var es in classEntity.Examschedules.ToList())
            {
                await _examScheduleRepository.DeleteAsync(es, save: false, ct: cancellationToken);
            }
        }

        if (classEntity.Assets != null)
        {
            foreach (var asset in classEntity.Assets.ToList())
            {
                asset.Assignedclassid = null;
                await _assetRepository.UpdateAsync(asset, save: false, ct: cancellationToken);
            }
        }

        await _classRepository.DeleteAsync(classEntity, save: true, ct: cancellationToken);
    }
}
