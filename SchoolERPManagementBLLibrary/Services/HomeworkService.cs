using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Homework;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class HomeworkService : IHomeworkService
{
    private readonly IRepository<int, Homework> _homeworkRepository;
    private readonly IRepository<int, Homeworksubmission> _submissionRepository;
    private readonly IRepository<int, Subject> _subjectRepository;
    private readonly IRepository<int, Teacher> _teacherRepository;
    private readonly IRepository<int, Class> _classRepository;
    private readonly IRepository<int, Student> _studentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IMapper _mapper;

    public HomeworkService(
        IRepository<int, Homework> homeworkRepository,
        IRepository<int, Homeworksubmission> submissionRepository,
        IRepository<int, Subject> subjectRepository,
        IRepository<int, Teacher> teacherRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Student> studentRepository,
        IFileStorageService fileStorageService,
        IMapper mapper)
    {
        _homeworkRepository = homeworkRepository;
        _submissionRepository = submissionRepository;
        _subjectRepository = subjectRepository;
        _teacherRepository = teacherRepository;
        _classRepository = classRepository;
        _studentRepository = studentRepository;
        _fileStorageService = fileStorageService;
        _mapper = mapper;
    }

    public async Task<HomeworkResponseDTO> CreateHomeworkAsync(CreateHomeworkDTO dto, CancellationToken cancellationToken)
    {
        await EnsureReferencesAsync(dto.SubjectId, dto.TeacherId, dto.ClassId, cancellationToken);

        string? attachmentUrl = null;
        if (dto.Attachment != null)
        {
            attachmentUrl = await _fileStorageService.UploadFileAsync(dto.Attachment, "homeworks", cancellationToken);
        }

        var homework = new Homework
        {
            Subjectid = dto.SubjectId,
            Teacherid = dto.TeacherId,
            Classid = dto.ClassId,
            Title = dto.Title,
            Description = dto.Description,
            Attachmenturl = attachmentUrl,
            Createdat = DateTime.UtcNow,
            Duedate = dto.DueDate
        };

        await _homeworkRepository.AddAsync(homework, save: true, ct: cancellationToken);
        return _mapper.Map<HomeworkResponseDTO>(homework);
    }

    public async Task<HomeworkSubmissionResponseDTO> SubmitHomeworkAsync(HomeworkSubmissionDTO dto, CancellationToken cancellationToken)
    {
        if (await _homeworkRepository.GetByIdAsync(dto.HomeworkId) is null)
        {
            throw new EntityNotFoundException("Homework", dto.HomeworkId.ToString());
        }

        if (await _studentRepository.GetByIdAsync(dto.StudentId) is null)
        {
            throw new EntityNotFoundException("Student", dto.StudentId.ToString());
        }

        string? uploadedFileUrl = null;
        if (dto.UploadedFile != null)
        {
            uploadedFileUrl = await _fileStorageService.UploadFileAsync(dto.UploadedFile, "homeworksubmissions", cancellationToken);
        }

        var submission = await _submissionRepository.Query(true)
            .FirstOrDefaultAsync(x => x.Homeworkid == dto.HomeworkId && x.Studentid == dto.StudentId, cancellationToken);

        if (submission is null)
        {
            submission = new Homeworksubmission
            {
                Homeworkid = dto.HomeworkId,
                Studentid = dto.StudentId,
                Uploadedfileurl = uploadedFileUrl,
                Submittedat = DateTime.UtcNow
            };

            await _submissionRepository.AddAsync(submission, save: true, ct: cancellationToken);
        }
        else
        {
            if (uploadedFileUrl != null)
            {
                
                submission.Uploadedfileurl = uploadedFileUrl;
            }
            submission.Submittedat = DateTime.UtcNow;
            await _submissionRepository.UpdateAsync(submission, save: true, ct: cancellationToken);
        }

        return _mapper.Map<HomeworkSubmissionResponseDTO>(submission);
    }

    public async Task<HomeworkSubmissionResponseDTO> EvaluateHomeworkAsync(EvaluateHomeworkDTO dto, CancellationToken cancellationToken)
    {
        var submission = await _submissionRepository.GetByIdAsync(dto.HomeworkSubmissionId);
        if (submission is null)
        {
            throw new EntityNotFoundException("Homework submission", dto.HomeworkSubmissionId.ToString());
        }

        submission.Marks = dto.Marks;
        submission.Remarks = dto.Remarks;
        submission.Verificationstatus = dto.VerificationStatus;

        await _submissionRepository.UpdateAsync(submission, save: true, ct: cancellationToken);
        return _mapper.Map<HomeworkSubmissionResponseDTO>(submission);
    }

    public async Task<IReadOnlyList<HomeworkResponseDTO>> GetHomeworksAsync(int classId, int? subjectId, CancellationToken cancellationToken)
    {
        var query = _homeworkRepository.Query(true).Where(h => h.Classid == classId);
        
        if (subjectId.HasValue)
        {
            query = query.Where(h => h.Subjectid == subjectId.Value);
        }

        var homeworks = await query.OrderByDescending(h => h.Createdat).ToListAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<HomeworkResponseDTO>>(homeworks);
    }

    public async Task<IReadOnlyList<HomeworkResponseDTO>> GetHomeworksByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.Query(true)
            .Include(s => s.Studentenrollments)
            .FirstOrDefaultAsync(s => s.Userid == userId, cancellationToken);

        if (student != null)
        {
            var enrollment = student.Studentenrollments.OrderByDescending(e => e.Id).FirstOrDefault();
            if (enrollment != null)
            {
                var studentHomeworks = await _homeworkRepository.Query(true)
                    .Where(h => h.Classid == enrollment.Classid)
                    .OrderByDescending(h => h.Createdat)
                    .ToListAsync(cancellationToken);
                return _mapper.Map<IReadOnlyList<HomeworkResponseDTO>>(studentHomeworks);
            }
            return Array.Empty<HomeworkResponseDTO>();
        }

        var teacher = await _teacherRepository.Query(true)
            .FirstOrDefaultAsync(t => t.Userid == userId, cancellationToken);
        
        if (teacher != null)
        {
            var teacherHomeworks = await _homeworkRepository.Query(true)
                .Where(h => h.Teacherid == teacher.Id)
                .OrderByDescending(h => h.Createdat)
                .ToListAsync(cancellationToken);
            return _mapper.Map<IReadOnlyList<HomeworkResponseDTO>>(teacherHomeworks);
        }

        return Array.Empty<HomeworkResponseDTO>();
    }

    public async Task<IReadOnlyList<HomeworkResponseDTO>> GetHomeworksByStudentIdAsync(int studentId, CancellationToken cancellationToken)
    {
        var student = await _studentRepository.Query(true)
            .Include(s => s.Studentenrollments)
            .FirstOrDefaultAsync(s => s.Id == studentId, cancellationToken);

        if (student != null)
        {
            var enrollment = student.Studentenrollments.OrderByDescending(e => e.Id).FirstOrDefault();
            if (enrollment != null)
            {
                var studentHomeworks = await _homeworkRepository.Query(true)
                    .Where(h => h.Classid == enrollment.Classid)
                    .OrderByDescending(h => h.Createdat)
                    .ToListAsync(cancellationToken);
                return _mapper.Map<IReadOnlyList<HomeworkResponseDTO>>(studentHomeworks);
            }
        }
        return Array.Empty<HomeworkResponseDTO>();
    }

    private async Task EnsureReferencesAsync(int subjectId, int teacherId, int classId, CancellationToken cancellationToken)
    {
        if (await _subjectRepository.GetByIdAsync(subjectId) is null)
        {
            throw new EntityNotFoundException("Subject", subjectId.ToString());
        }

        if (await _teacherRepository.GetByIdAsync(teacherId) is null)
        {
            throw new EntityNotFoundException("Teacher", teacherId.ToString());
        }

        if (await _classRepository.GetByIdAsync(classId) is null)
        {
            throw new EntityNotFoundException("Class", classId.ToString());
        }
    }
}
