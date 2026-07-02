using Microsoft.EntityFrameworkCore;
using SchoolERPManagementBLLibrary.DTOs.Dashboard;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly IRepository<int, Student> _studentRepository;
    private readonly IRepository<int, Teacher> _teacherRepository;
    private readonly IRepository<int, Parent> _parentRepository;
    private readonly IRepository<int, Feepayment> _feePaymentRepository;
    private readonly IRepository<int, Class> _classRepository;
    private readonly IRepository<int, Asset> _assetRepository;
    private readonly IRepository<int, Attendance> _attendanceRepository;
    private readonly IRepository<int, Staffattendance> _staffAttendanceRepository;
    private readonly IRepository<int, Studentenrollment> _studentEnrollmentRepository;
    private readonly IRepository<int, Academicyear> _academicYearRepository;
    private readonly IRepository<int, Teachersubject> _teacherSubjectRepository;
    private readonly IRepository<int, Timetable> _timetableRepository;
    private readonly IRepository<int, Homework> _homeworkRepository;
    private readonly IRepository<int, Homeworksubmission> _homeworkSubmissionRepository;
    private readonly IRepository<int, Subject> _subjectRepository;

    public DashboardService(
        IRepository<int, Student> studentRepository,
        IRepository<int, Teacher> teacherRepository,
        IRepository<int, Parent> parentRepository,
        IRepository<int, Feepayment> feePaymentRepository,
        IRepository<int, Class> classRepository,
        IRepository<int, Asset> assetRepository,
        IRepository<int, Attendance> attendanceRepository,
        IRepository<int, Staffattendance> staffAttendanceRepository,
        IRepository<int, Studentenrollment> studentEnrollmentRepository,
        IRepository<int, Academicyear> academicYearRepository,
        IRepository<int, Teachersubject> teacherSubjectRepository,
        IRepository<int, Timetable> timetableRepository,
        IRepository<int, Homework> homeworkRepository,
        IRepository<int, Homeworksubmission> homeworkSubmissionRepository,
        IRepository<int, Subject> subjectRepository)
    {
        _studentRepository = studentRepository;
        _teacherRepository = teacherRepository;
        _parentRepository = parentRepository;
        _feePaymentRepository = feePaymentRepository;
        _classRepository = classRepository;
        _assetRepository = assetRepository;
        _attendanceRepository = attendanceRepository;
        _staffAttendanceRepository = staffAttendanceRepository;
        _studentEnrollmentRepository = studentEnrollmentRepository;
        _academicYearRepository = academicYearRepository;
        _teacherSubjectRepository = teacherSubjectRepository;
        _timetableRepository = timetableRepository;
        _homeworkRepository = homeworkRepository;
        _homeworkSubmissionRepository = homeworkSubmissionRepository;
        _subjectRepository = subjectRepository;
    }

    public async Task<AdminDashboardDTO> GetAdminDashboardMetricsAsync(int? academicYearId, CancellationToken cancellationToken)
    {
        var targetYearId = academicYearId;
        if (!targetYearId.HasValue)
        {
            var activeYear = await _academicYearRepository.Query(true)
                .FirstOrDefaultAsync(y => y.Iscurrent == true, cancellationToken);
            if (activeYear == null)
            {
                activeYear = await _academicYearRepository.Query(true)
                    .OrderByDescending(y => y.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            if (activeYear != null)
            {
                targetYearId = activeYear.Id;
            }
        }

        var enrolledStudentIds = targetYearId.HasValue
            ? await _studentEnrollmentRepository.Query(true)
                .Where(se => se.Academicyearid == targetYearId.Value)
                .Select(se => se.Studentid)
                .Distinct()
                .ToListAsync(cancellationToken)
            : new List<int>();

        var totalStudents = enrolledStudentIds.Count;
        var totalTeachers = await _teacherRepository.Query(true).CountAsync(cancellationToken);

        var totalParents = 0;
        if (targetYearId.HasValue)
        {
            totalParents = await _studentEnrollmentRepository.Query(true)
                .Where(se => se.Academicyearid == targetYearId.Value)
                .SelectMany(se => se.Student.Studentparents.Select(sp => sp.Parentid))
                .Distinct()
                .CountAsync(cancellationToken);
        }

        var totalClasses = targetYearId.HasValue
            ? await _classRepository.Query(true).Where(c => c.Academicyearid == targetYearId.Value).CountAsync(cancellationToken)
            : 0;

        var totalSubjects = await _subjectRepository.Query(true).CountAsync(cancellationToken);
        
        var totalTimetables = targetYearId.HasValue
            ? await _timetableRepository.Query(true).Where(t => t.Class.Academicyearid == targetYearId.Value).CountAsync(cancellationToken)
            : 0;

        var feePaymentsQuery = _feePaymentRepository.Query(true);
        if (targetYearId.HasValue)
        {
            feePaymentsQuery = feePaymentsQuery
                .Include(f => f.Feestructure)
                .Where(f => f.Feestructure.Academicyearid == targetYearId.Value);
        }

        var totalRevenue = await feePaymentsQuery.SumAsync(f => f.Amountpaid, cancellationToken);
        var totalAssets = await _assetRepository.Query(true).CountAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        
        var studentAttendanceRecords = new List<Attendance>();
        if (enrolledStudentIds.Any())
        {
            studentAttendanceRecords = await _attendanceRepository.Query(true)
                .Where(a => a.Date == today && enrolledStudentIds.Contains(a.Studentid))
                .ToListAsync(cancellationToken);
        }
        
        double studentAttendanceRate = 0;
        if (totalStudents > 0)
        {
            var presentCount = studentAttendanceRecords.Count(a => a.Status.ToLower() == "present");
            studentAttendanceRate = Math.Round((double)presentCount / totalStudents * 100, 1);
        }

        var staffAttendanceRecords = await _staffAttendanceRepository.Query(true)
            .Where(a => a.Date == today)
            .ToListAsync(cancellationToken);
        
        var totalStaff = totalTeachers;
        double staffAttendanceRate = 0;
        if (totalStaff > 0)
        {
            var presentCount = staffAttendanceRecords.Count(a => a.Status.ToLower() == "present");
            staffAttendanceRate = Math.Round((double)presentCount / totalStaff * 100, 1);
        }

        var sixMonthsAgo = DateTime.UtcNow.AddMonths(-5);
        var paymentsLast6Months = await feePaymentsQuery
            .Where(f => f.Paymentdate >= sixMonthsAgo)
            .ToListAsync(cancellationToken);

        var revenueTrends = paymentsLast6Months
            .Where(p => p.Paymentdate.HasValue)
            .GroupBy(p => new { p.Paymentdate!.Value.Year, p.Paymentdate!.Value.Month })
            .Select(g => new
            {
                Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                Amount = g.Sum(p => p.Amountpaid)
            })
            .OrderBy(x => x.Date)
            .Select(x => new MonthlyRevenueDTO(x.Date.ToString("MMM"), x.Amount))
            .ToList();

        if (!revenueTrends.Any())
        {
            for(int i = 5; i >= 0; i--)
            {
                revenueTrends.Add(new MonthlyRevenueDTO(DateTime.UtcNow.AddMonths(-i).ToString("MMM"), 0));
            }
        }

        var recentTransactions = await feePaymentsQuery
            .Include(f => f.Student)
            .OrderByDescending(f => f.Paymentdate)
            .Take(5)
            .Select(f => new RecentTransactionDTO(
                f.Student != null ? f.Student.Name : "Unknown",
                f.Amountpaid,
                f.Paymentdate ?? DateTime.UtcNow,
                "Completed"
            ))
            .ToListAsync(cancellationToken);

        return new AdminDashboardDTO(
            totalStudents,
            totalTeachers,
            totalParents,
            totalRevenue,
            totalClasses,
            totalSubjects,
            totalTimetables,
            totalAssets,
            studentAttendanceRate,
            staffAttendanceRate,
            revenueTrends,
            recentTransactions
        );
    }

    public async Task<TeacherDashboardDTO> GetTeacherDashboardMetricsAsync(int teacherId, int? academicYearId, CancellationToken cancellationToken)
    {
        var targetYearId = academicYearId;
        if (!targetYearId.HasValue)
        {
            var activeYear = await _academicYearRepository.Query(true)
                .FirstOrDefaultAsync(y => y.Iscurrent == true, cancellationToken);
            if (activeYear == null)
            {
                activeYear = await _academicYearRepository.Query(true)
                    .OrderByDescending(y => y.Id)
                    .FirstOrDefaultAsync(cancellationToken);
            }
            if (activeYear != null)
            {
                targetYearId = activeYear.Id;
            }
        }

        // Get assigned classes and subjects
        var assignedSubjects = await _teacherSubjectRepository.Query(true)
            .Where(ts => ts.Teacherid == teacherId)
            .ToListAsync(cancellationToken);

        var assignedClassesFromTimetable = await _timetableRepository.Query(true)
            .Where(t => t.Teacherid == teacherId)
            .Select(t => new { t.Classid, t.Subjectid })
            .Distinct()
            .ToListAsync(cancellationToken);

        var classIds = assignedSubjects.Select(ts => ts.Classid)
            .Concat(assignedClassesFromTimetable.Select(t => t.Classid))
            .Distinct()
            .ToList();

        var subjectIds = assignedSubjects.Select(ts => ts.Subjectid)
            .Concat(assignedClassesFromTimetable.Select(t => t.Subjectid))
            .Distinct()
            .ToList();

        var totalClasses = classIds.Count;
        var totalSubjects = subjectIds.Count;

        // Get student count in these classes for the target academic year
        var enrolledStudentsQuery = _studentEnrollmentRepository.Query(true);
        if (targetYearId.HasValue)
        {
            enrolledStudentsQuery = enrolledStudentsQuery.Where(se => se.Academicyearid == targetYearId.Value);
        }
        
        var enrolledStudents = await enrolledStudentsQuery
            .Where(se => classIds.Contains(se.Classid))
            .Select(se => se.Studentid)
            .Distinct()
            .ToListAsync(cancellationToken);

        var totalStudents = enrolledStudents.Count;

        // Active / Pending Homework assignments posted by this teacher
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todayStr = today.ToString("yyyy-MM-dd");
        
        var homeworkList = await _homeworkRepository.Query(true)
            .Include(h => h.Class)
            .Include(h => h.Subject)
            .Where(h => h.Teacherid == teacherId)
            .ToListAsync(cancellationToken);

        var pendingHomeworkCount = homeworkList
            .Count(h => string.Compare(h.Duedate.ToString("yyyy-MM-dd"), todayStr) >= 0);

        // Student attendance rate for the teacher's classes today
        var studentAttendanceRecords = new List<Attendance>();
        if (enrolledStudents.Any())
        {
            studentAttendanceRecords = await _attendanceRepository.Query(true)
                .Where(a => a.Date == today && enrolledStudents.Contains(a.Studentid))
                .ToListAsync(cancellationToken);
        }

        double studentAttendanceRate = 0;
        if (enrolledStudents.Count > 0)
        {
            var presentCount = studentAttendanceRecords.Count(a => a.Status.ToLower() == "present" || a.Status.ToLower() == "late");
            studentAttendanceRate = Math.Round((double)presentCount / enrolledStudents.Count * 100, 1);
        }

        // Today's schedule
        var dayOfWeekToday = DateTime.UtcNow.DayOfWeek.ToString();
        var scheduleSlots = await _timetableRepository.Query(true)
            .Include(t => t.Class)
            .Include(t => t.Subject)
            .Where(t => t.Teacherid == teacherId && t.Dayofweek.ToLower() == dayOfWeekToday.ToLower())
            .OrderBy(t => t.Starttime)
            .ToListAsync(cancellationToken);

        var todaySchedule = scheduleSlots.Select(t => new TeacherTimetableSlotDTO(
            t.Id,
            t.Dayofweek,
            t.Starttime.ToString("HH:mm"),
            t.Endtime.ToString("HH:mm"),
            t.Class?.Classname ?? "Unknown",
            t.Class?.Section ?? "A",
            t.Subject?.Subjectname ?? "Unknown"
        )).ToList();

        // Recent homework list
        var recentHomework = new List<TeacherHomeworkDTO>();
        foreach (var hw in homeworkList.OrderByDescending(h => h.Id).Take(5))
        {
            var submissionsCount = await _homeworkSubmissionRepository.Query(true)
                .CountAsync(s => s.Homeworkid == hw.Id, cancellationToken);

            var totalInClass = await _studentEnrollmentRepository.Query(true)
                .CountAsync(se => se.Classid == hw.Classid && se.Academicyearid == (targetYearId ?? 1), cancellationToken);

            recentHomework.Add(new TeacherHomeworkDTO(
                hw.Id,
                hw.Title ?? "Untitled",
                hw.Class?.Classname ?? "Unknown",
                hw.Class?.Section ?? "A",
                hw.Subject?.Subjectname ?? "Unknown",
                hw.Duedate.ToString("yyyy-MM-dd"),
                submissionsCount,
                totalInClass
            ));
        }

        return new TeacherDashboardDTO(
            totalStudents,
            totalClasses,
            totalSubjects,
            pendingHomeworkCount,
            studentAttendanceRate,
            todaySchedule,
            recentHomework
        );
    }
}
