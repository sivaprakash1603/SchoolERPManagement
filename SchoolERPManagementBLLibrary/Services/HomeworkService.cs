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

    public HomeworkService(
        IRepository<int, Homework> homeworkRepository,
        IRepository<int, Homeworksubmission> submissionRepository,
        IRepository<int, Subject> subjectRepository,
        IRepository<int, Teacher> teacherRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Student> studentRepository,
        IFileStorageService fileStorageService)
    {
        _homeworkRepository = homeworkRepository;
        _submissionRepository = submissionRepository;
        _subjectRepository = subjectRepository;
        _teacherRepository = teacherRepository;
        _classRepository = classRepository;
        _studentRepository = studentRepository;
        _fileStorageService = fileStorageService;
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
        return new HomeworkResponseDTO(homework.Id, homework.Subjectid, homework.Teacherid, homework.Classid, homework.Title, homework.Description, homework.Attachmenturl, homework.Createdat, homework.Duedate);
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
                // Optionally delete the old file using _fileStorageService.DeleteFile(submission.Uploadedfileurl);
                submission.Uploadedfileurl = uploadedFileUrl;
            }
            submission.Submittedat = DateTime.UtcNow;
            await _submissionRepository.UpdateAsync(submission, save: true, ct: cancellationToken);
        }

        return new HomeworkSubmissionResponseDTO(submission.Id, submission.Homeworkid, submission.Studentid, submission.Uploadedfileurl, submission.Verificationstatus, submission.Marks, submission.Remarks, submission.Submittedat);
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
        return new HomeworkSubmissionResponseDTO(submission.Id, submission.Homeworkid, submission.Studentid, submission.Uploadedfileurl, submission.Verificationstatus, submission.Marks, submission.Remarks, submission.Submittedat);
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
