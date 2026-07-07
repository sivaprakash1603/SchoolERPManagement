using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Homework;
using SchoolERPManagementBLLibrary.DTOs.Notification;
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
    private readonly IRepository<int, Teachersubject> _teacherSubjectRepository;
    private readonly IRepository<int, Timetable> _timetableRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public HomeworkService(
        IRepository<int, Homework> homeworkRepository,
        IRepository<int, Homeworksubmission> submissionRepository,
        IRepository<int, Subject> subjectRepository,
        IRepository<int, Teacher> teacherRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Student> studentRepository,
        IRepository<int, Teachersubject> teacherSubjectRepository,
        IRepository<int, Timetable> timetableRepository,
        IFileStorageService fileStorageService,
        INotificationService notificationService,
        IMapper mapper)
    {
        _homeworkRepository = homeworkRepository;
        _submissionRepository = submissionRepository;
        _subjectRepository = subjectRepository;
        _teacherRepository = teacherRepository;
        _classRepository = classRepository;
        _studentRepository = studentRepository;
        _teacherSubjectRepository = teacherSubjectRepository;
        _timetableRepository = timetableRepository;
        _fileStorageService = fileStorageService;
        _notificationService = notificationService;
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

        // Fetch enrolled student user IDs for the class
        var studentUserIds = await _studentRepository.Query(true)
            .Where(s => s.Studentenrollments.Any(e => e.Classid == dto.ClassId))
            .Select(s => s.Userid)
            .ToListAsync(cancellationToken);

        if (studentUserIds.Any())
        {
            var teacherEntity = await _teacherRepository.GetByIdAsync(dto.TeacherId);
            int? senderUserId = teacherEntity?.Userid;

            var notificationDto = new SendNotificationDTO(
                Title: "New Homework Posted",
                Message: $"A new homework assignment '{dto.Title}' has been posted.",
                CreatedByUserId: senderUserId,
                TargetUserIds: studentUserIds
            );
            await _notificationService.SendNotificationAsync(notificationDto, cancellationToken);
        }

        return _mapper.Map<HomeworkResponseDTO>(homework);
    }

    public async Task<HomeworkSubmissionResponseDTO> SubmitHomeworkAsync(HomeworkSubmissionDTO dto, int? userId, string userRole, CancellationToken cancellationToken)
    {
        if (userRole == "Student" && userId.HasValue)
        {
            var studentEntity = await _studentRepository.Query(true).FirstOrDefaultAsync(s => s.Userid == userId.Value, cancellationToken);
            if (studentEntity == null || studentEntity.Id != dto.StudentId)
            {
                throw new BusinessRuleException("You can only submit homework for your own account.");
            }
        }

        var homework = await _homeworkRepository.GetByIdAsync(dto.HomeworkId);
        if (homework is null)
        {
            throw new EntityNotFoundException("Homework", dto.HomeworkId.ToString());
        }

        var student = await _studentRepository.Query(true)
            .Include(s => s.Studentenrollments)
            .FirstOrDefaultAsync(s => s.Id == dto.StudentId, cancellationToken);

        if (student is null)
        {
            throw new EntityNotFoundException("Student", dto.StudentId.ToString());
        }

        var activeEnrollment = student.Studentenrollments.OrderByDescending(e => e.Id).FirstOrDefault();
        if (activeEnrollment == null || activeEnrollment.Classid != homework.Classid)
        {
            throw new BusinessRuleException("The student is not enrolled in the class for this homework.");
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
                Submittedat = DateTime.UtcNow,
                Verificationstatus = "pending"
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
            submission.Verificationstatus = "pending";
            await _submissionRepository.UpdateAsync(submission, save: true, ct: cancellationToken);
        }

        return _mapper.Map<HomeworkSubmissionResponseDTO>(submission);
    }

    public async Task<HomeworkSubmissionResponseDTO> EvaluateHomeworkAsync(EvaluateHomeworkDTO dto, int? userId, string userRole, CancellationToken cancellationToken)
    {
        var submission = await _submissionRepository.GetByIdAsync(dto.HomeworkSubmissionId);
        if (submission is null)
        {
            throw new EntityNotFoundException("Homework submission", dto.HomeworkSubmissionId.ToString());
        }

        if (userRole == "Teacher" && userId.HasValue)
        {
            var homework = await _homeworkRepository.GetByIdAsync(submission.Homeworkid);
            var teacher = await _teacherRepository.Query(true).FirstOrDefaultAsync(t => t.Userid == userId.Value, cancellationToken);
            if (teacher == null || homework == null || homework.Teacherid != teacher.Id)
            {
                throw new BusinessRuleException("You are not authorized to evaluate this homework.");
            }
        }

        submission.Marks = dto.Marks;
        submission.Remarks = dto.Remarks;
        submission.Verificationstatus = dto.VerificationStatus?.ToLower();

        await _submissionRepository.UpdateAsync(submission, save: true, ct: cancellationToken);

        // Fetch student user details and homework details to notify
        var studentEntity = await _studentRepository.GetByIdAsync(submission.Studentid);
        var homeworkEntity = await _homeworkRepository.GetByIdAsync(submission.Homeworkid);

        if (studentEntity != null && homeworkEntity != null)
        {
            var notificationDto = new SendNotificationDTO(
                Title: "Homework Graded",
                Message: $"Your submission for homework '{homeworkEntity.Title}' has been graded. Status: {submission.Verificationstatus}. Marks: {submission.Marks}.",
                CreatedByUserId: userId,
                TargetUserIds: new[] { studentEntity.Userid }
            );
            await _notificationService.SendNotificationAsync(notificationDto, cancellationToken);
        }

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
                    .Include(h => h.Homeworksubmissions.Where(sub => sub.Studentid == student.Id))
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
                    .Include(h => h.Subject)
                    .Include(h => h.Teacher)
                    .Include(h => h.Homeworksubmissions.Where(sub => sub.Studentid == studentId))
                    .Where(h => h.Classid == enrollment.Classid)
                    .OrderByDescending(h => h.Createdat)
                    .ToListAsync(cancellationToken);
                return _mapper.Map<IReadOnlyList<HomeworkResponseDTO>>(studentHomeworks);
            }
        }
        return Array.Empty<HomeworkResponseDTO>();
    }

    public async Task<IReadOnlyList<HomeworkSubmissionDetailsDTO>> GetSubmissionsByHomeworkIdAsync(int homeworkId, CancellationToken cancellationToken)
    {
        var submissions = await _submissionRepository.Query(true)
            .Include(x => x.Student)
            .Where(x => x.Homeworkid == homeworkId)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<HomeworkSubmissionDetailsDTO>>(submissions);
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

        bool isAssigned = await _teacherSubjectRepository.Query(true)
            .AnyAsync(ts => ts.Teacherid == teacherId && ts.Classid == classId && ts.Subjectid == subjectId, cancellationToken);

        bool isAssignedInTimetable = await _timetableRepository.Query(true)
            .AnyAsync(t => t.Teacherid == teacherId && t.Classid == classId && t.Subjectid == subjectId, cancellationToken);

        if (!isAssigned && !isAssignedInTimetable)
        {
            throw new BusinessRuleException("The teacher is not assigned to teach this subject for this class.");
        }
    }

    public async Task<bool> DeleteSubmissionAsync(int submissionId, int? userId, string userRole, CancellationToken cancellationToken)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId);
        if (submission is null)
        {
            throw new EntityNotFoundException("Homework submission", submissionId.ToString());
        }

        // Only allow deletion if submission is pending/waiting for grading
        var status = submission.Verificationstatus?.ToLower();
        if (status != null && status != "pending")
        {
            throw new BusinessRuleException("Cannot unsubmit this homework assignment as it has already been evaluated.");
        }

        // Security check
        if (userRole != "Admin")
        {
            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessException();
            }

            var student = await _studentRepository.Query(true)
                .FirstOrDefaultAsync(s => s.Userid == userId.Value, cancellationToken);

            if (student is null || submission.Studentid != student.Id)
            {
                throw new BusinessRuleException("You do not have permission to delete/unsubmit this homework.");
            }
        }

        // Delete attachment if present
        if (!string.IsNullOrEmpty(submission.Uploadedfileurl))
        {
            _fileStorageService.DeleteFile(submission.Uploadedfileurl);
        }

        await _submissionRepository.DeleteAsync(submission, save: true, ct: cancellationToken);
        return true;
    }
}
