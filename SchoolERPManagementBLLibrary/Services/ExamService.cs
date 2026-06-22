using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Exam;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class ExamService : IExamService
{
    private readonly IRepository<int, Exam> _examRepository;
    private readonly IRepository<int, Examresult> _examResultRepository;
    private readonly IRepository<int, Academicyear> _academicYearRepository;
    private readonly IRepository<int, Subject> _subjectRepository;
    private readonly IRepository<int, Student> _studentRepository;
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;
    private readonly IRepository<int, Examschedule> _examScheduleRepository;
    private readonly IRepository<int, Class> _classRepository;
    private readonly IMapper _mapper;

    public ExamService(
        IRepository<int, Exam> examRepository,
        IRepository<int, Examresult> examResultRepository,
        IRepository<int, Academicyear> academicYearRepository,
        IRepository<int, Subject> subjectRepository,
        IRepository<int, Student> studentRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository,
        IRepository<int, Examschedule> examScheduleRepository,
        IRepository<int, Class> classRepository,
        IMapper mapper)
    {
        _examRepository = examRepository;
        _examResultRepository = examResultRepository;
        _academicYearRepository = academicYearRepository;
        _subjectRepository = subjectRepository;
        _studentRepository = studentRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
        _examScheduleRepository = examScheduleRepository;
        _classRepository = classRepository;
        _mapper = mapper;
    }

    public async Task<ExamResponseDTO> CreateExamAsync(CreateExamDTO dto, CancellationToken cancellationToken)
    {
        if (dto.AcademicyearId.HasValue && await _academicYearRepository.GetByIdAsync(dto.AcademicyearId.Value) is null)
        {
            throw new EntityNotFoundException("Academic year", dto.AcademicyearId.Value.ToString());
        }

        var exam = new Exam
        {
            Examname = dto.Examname,
            Academicyearid = dto.AcademicyearId
        };

        await _examRepository.AddAsync(exam, save: true, ct: cancellationToken);
        return _mapper.Map<ExamResponseDTO>(exam);
    }

    public async Task<ExamResultResponseDTO> PublishResultAsync(PublishResultDTO dto, CancellationToken cancellationToken)
    {
        if (await _examRepository.GetByIdAsync(dto.ExamId) is null)
        {
            throw new EntityNotFoundException("Exam", dto.ExamId.ToString());
        }

        if (await _subjectRepository.GetByIdAsync(dto.SubjectId) is null)
        {
            throw new EntityNotFoundException("Subject", dto.SubjectId.ToString());
        }

        if (await _studentRepository.GetByIdAsync(dto.StudentId) is null)
        {
            throw new EntityNotFoundException("Student", dto.StudentId.ToString());
        }

        var latestEnrollment = await _studentEnrollmentRepository.Query(true)
            .Where(e => e.Studentid == dto.StudentId)
            .OrderByDescending(e => e.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestEnrollment == null)
        {
            throw new BusinessRuleException("The student is not currently enrolled in any class.");
        }

        bool isScheduled = await _examScheduleRepository.Query(true)
            .AnyAsync(s => s.Examid == dto.ExamId && s.Classid == latestEnrollment.Classid && s.Subjectid == dto.SubjectId, cancellationToken);

        if (!isScheduled)
        {
            throw new BusinessRuleException("This exam/subject is not scheduled for the student's class.");
        }

        var result = await _examResultRepository.Query(true)
            .FirstOrDefaultAsync(x => x.Examid == dto.ExamId && x.Subjectid == dto.SubjectId && x.Studentid == dto.StudentId, cancellationToken);

        if (result is null)
        {
            result = new Examresult
            {
                Examid = dto.ExamId,
                Subjectid = dto.SubjectId,
                Studentid = dto.StudentId,
                Marks = dto.Marks,
                Uploadedcorrectedanswersheeturl = dto.UploadedCorrectedAnswerSheetUrl
            };

            await _examResultRepository.AddAsync(result, save: true, ct: cancellationToken);
        }
        else
        {
            result.Marks = dto.Marks;
            result.Uploadedcorrectedanswersheeturl = dto.UploadedCorrectedAnswerSheetUrl;
            await _examResultRepository.UpdateAsync(result, save: true, ct: cancellationToken);
        }

        return _mapper.Map<ExamResultResponseDTO>(result);
    }

    public async Task<IReadOnlyList<ExamResultResponseDTO>> GetStudentResultsAsync(int studentId, int userId, string userRole, CancellationToken cancellationToken)
    {
        if (userRole == "Student")
        {
            var student = await _studentRepository.Query(true).FirstOrDefaultAsync(s => s.Userid == userId, cancellationToken);
            if (student == null || student.Id != studentId)
            {
                throw new UnauthorizedAccessException("You are not authorized to view these results.");
            }
        }
        else if (userRole == "Parent")
        {
            var student = await _studentRepository.GetByIdAsync(studentId);
            if (student == null) throw new EntityNotFoundException("Student", studentId.ToString());
            
            var parent = await _studentRepository.Query(true)
                .Include(s => s.Studentparents)
                .ThenInclude(sp => sp.Parent)
                .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);
            
            if (parent == null || !parent.Studentparents.Any(sp => sp.Parent != null && sp.Parent.Userid == userId))
            {
                throw new UnauthorizedAccessException("You are not authorized to view these results.");
            }
        }

        var items = await _examResultRepository.Query(true)
            .Where(result => result.Studentid == studentId)
            .OrderByDescending(result => result.Id)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ExamResultResponseDTO>>(items);
    }

    public async Task<IReadOnlyList<ExamResponseDTO>> GetAllExamsAsync(CancellationToken cancellationToken)
    {
        var exams = await _examRepository.Query(true)
            .OrderByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<ExamResponseDTO>>(exams);
    }

    public async Task<ExamScheduleResponseDTO> CreateExamScheduleAsync(CreateExamScheduleDTO dto, CancellationToken cancellationToken)
    {
        if (await _examRepository.GetByIdAsync(dto.ExamId) is null)
        {
            throw new EntityNotFoundException("Exam", dto.ExamId.ToString());
        }

        if (await _subjectRepository.GetByIdAsync(dto.SubjectId) is null)
        {
            throw new EntityNotFoundException("Subject", dto.SubjectId.ToString());
        }

        if (await _classRepository.GetByIdAsync(dto.ClassId) is null)
        {
            throw new EntityNotFoundException("Class", dto.ClassId.ToString());
        }

        var existing = await _examScheduleRepository.Query(true)
            .FirstOrDefaultAsync(x => x.Examid == dto.ExamId && x.Classid == dto.ClassId && x.Subjectid == dto.SubjectId, cancellationToken);

        if (existing != null)
        {
            throw new BusinessRuleException("This subject is already scheduled for the selected exam and class.");
        }

        var schedule = new Examschedule
        {
            Examid = dto.ExamId,
            Subjectid = dto.SubjectId,
            Classid = dto.ClassId,
            Examdate = dto.ExamDate,
            Durationminutes = dto.DurationMinutes,
            Session = dto.Session
        };

        await _examScheduleRepository.AddAsync(schedule, save: true, ct: cancellationToken);

        var savedSchedule = await _examScheduleRepository.Query(true)
            .Include(x => x.Subject)
            .Include(x => x.Class)
            .FirstOrDefaultAsync(x => x.Id == schedule.Id, cancellationToken);

        return _mapper.Map<ExamScheduleResponseDTO>(savedSchedule);
    }

    public async Task<IReadOnlyList<ExamScheduleResponseDTO>> GetExamSchedulesByExamIdAsync(int examId, CancellationToken cancellationToken)
    {
        var schedules = await _examScheduleRepository.Query(true)
            .Include(x => x.Subject)
            .Include(x => x.Class)
            .Where(x => x.Examid == examId)
            .OrderBy(x => x.Examdate)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<ExamScheduleResponseDTO>>(schedules);
    }

    public async Task<IReadOnlyList<ExamResultResponseDTO>> GetExamResultsByClassAsync(int examId, int classId, int subjectId, CancellationToken cancellationToken)
    {
        var studentIds = await _studentEnrollmentRepository.Query(true)
            .Where(e => e.Classid == classId)
            .Select(e => e.Studentid)
            .ToListAsync(cancellationToken);

        var results = await _examResultRepository.Query(true)
            .Where(r => r.Examid == examId && r.Subjectid == subjectId && studentIds.Contains(r.Studentid))
            .ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<ExamResultResponseDTO>>(results);
    }
}
