using AutoMapper;
using System.Linq;
using SchoolERPManagementBLLibrary.DTOs.AcademicCalendar;
using SchoolERPManagementBLLibrary.DTOs.AcademicYear;
using SchoolERPManagementBLLibrary.DTOs.Asset;
using SchoolERPManagementBLLibrary.DTOs.Attendance;
using SchoolERPManagementBLLibrary.DTOs.Auth;
using SchoolERPManagementBLLibrary.DTOs.Class;
using SchoolERPManagementBLLibrary.DTOs.Document;
using SchoolERPManagementBLLibrary.DTOs.Exam;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.DTOs.Homework;
using SchoolERPManagementBLLibrary.DTOs.Notification;
using SchoolERPManagementBLLibrary.DTOs.Parent;
using SchoolERPManagementBLLibrary.DTOs.StaffAttendance;
using SchoolERPManagementBLLibrary.DTOs.Student;
using SchoolERPManagementBLLibrary.DTOs.Subject;
using SchoolERPManagementBLLibrary.DTOs.Teacher;
using SchoolERPManagementBLLibrary.DTOs.Timetable;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementBLLibrary.Profiles;

public class AppMappingProfile : Profile
{
    public AppMappingProfile()
    {
        
        CreateMap<Academicyear, AcademicYearResponseDTO>()
            .ConstructUsing(src => new AcademicYearResponseDTO(src.Id, src.Yearname, src.Startdate, src.Enddate, src.Iscurrent ?? false));

        
        CreateMap<Asset, AssetResponseDTO>();
        CreateMap<Assetreport, AssetReportResponseDTO>();
        CreateMap<Assettype, AssetTypeResponseDTO>();

        
        CreateMap<Attendance, AttendanceResponseDTO>();
        CreateMap<Staffattendance, StaffAttendanceResponseDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty));


        
        CreateMap<Class, ClassResponseDTO>()
            .ConstructUsing((src, ctx) => new ClassResponseDTO(
                src.Id,
                src.Classname,
                src.Section,
                src.Classteacherid,
                src.Academicyearid,
                src.Studentenrollments != null ? src.Studentenrollments.Count : 0,
                src.Classsubjects != null ? src.Classsubjects.Select(cs => new SubjectResponseDTO(cs.Subjectid, cs.Subject != null ? cs.Subject.Subjectname : string.Empty)).ToList() : null
            ));

        
        CreateMap<Studentdocument, StudentDocumentResponseDTO>();
        CreateMap<Teacherdocument, TeacherDocumentResponseDTO>();

        
        CreateMap<Exam, ExamResponseDTO>();
        CreateMap<Examresult, ExamResultResponseDTO>();

        
        CreateMap<Feepayment, FeePaymentResponseDTO>();
        CreateMap<Feestructure, FeeStructureResponseDTO>()
            .ConstructUsing(src => new FeeStructureResponseDTO(src.Id, src.Classid, src.Academicyearid, src.Feename ?? string.Empty, src.Totalamount, src.Duedate));
        CreateMap<Feestructure, FeeComponentDTO>()
            .ConstructUsing(src => new FeeComponentDTO(src.Id, src.Feename, src.Totalamount));

        
        CreateMap<Homework, HomeworkResponseDTO>()
            .ConstructUsing(src => new HomeworkResponseDTO(
                src.Id, src.Subjectid, src.Subject != null ? src.Subject.Subjectname : null, src.Teacherid, src.Teacher != null ? src.Teacher.Name : null, src.Classid, src.Title, src.Description, src.Attachmenturl, src.Createdat, src.Duedate, 
                src.Homeworksubmissions != null && src.Homeworksubmissions.Any() ? 
                    new HomeworkSubmissionResponseDTO(
                        src.Homeworksubmissions.First().Id, 
                        src.Homeworksubmissions.First().Homeworkid, 
                        src.Homeworksubmissions.First().Studentid, 
                        src.Homeworksubmissions.First().Uploadedfileurl, 
                        src.Homeworksubmissions.First().Verificationstatus, 
                        src.Homeworksubmissions.First().Marks, 
                        src.Homeworksubmissions.First().Remarks, 
                        src.Homeworksubmissions.First().Submittedat) 
                    : null
            ));
        CreateMap<Homeworksubmission, HomeworkSubmissionResponseDTO>();
        CreateMap<Homeworksubmission, HomeworkSubmissionDetailsDTO>()
            .ConstructUsing(src => new HomeworkSubmissionDetailsDTO(
                src.Id,
                src.Homeworkid,
                src.Studentid,
                src.Student != null ? src.Student.Name : string.Empty,
                src.Uploadedfileurl,
                src.Verificationstatus,
                src.Marks,
                src.Remarks,
                src.Submittedat
            ));

        
        CreateMap<Notification, NotificationResponseDTO>();
        CreateMap<Usernotification, UserNotificationResponseDTO>()
            .ConstructUsing(src => new UserNotificationResponseDTO(
                src.Id,
                src.Notificationid,
                src.Notification != null ? src.Notification.Title : string.Empty,
                src.Notification != null ? src.Notification.Message : string.Empty,
                src.Notification != null ? src.Notification.Createdat : null,
                src.Isread));

        
        CreateMap<Parent, ParentResponseDTO>()
            .ConstructUsing(src => new ParentResponseDTO(
                src.Id, 
                src.Userid, 
                src.Name, 
                src.Studentparents != null && src.Studentparents.Any()
                    ? string.Join(", ", src.Studentparents.Select(sp => sp.Student.Name))
                    : null, 
                src.Phonenumber, 
                src.User != null ? src.User.Email : null, 
                null, 
                src.User != null ? src.User.Username : null
            ));

        
        CreateMap<Student, StudentResponseDTO>()
            .ConstructUsing(src => new StudentResponseDTO(
                src.Id, 
                src.Userid, 
                src.Regno, 
                src.Name, 
                src.Studentparents != null && src.Studentparents.Any() 
                    ? src.Studentparents.First().Parentid 
                    : null, 
                null,
                src.Studentenrollments != null && src.Studentenrollments.Any()
                    ? src.Studentenrollments.OrderByDescending(e => e.Academicyearid).First().Classid
                    : null,
                src.Studentdocuments != null && src.Studentdocuments.Any(d => d.Documentname == "Photo")
                    ? src.Studentdocuments.First(d => d.Documentname == "Photo").Bloburl
                    : null
            ));

        
        CreateMap<Subject, SubjectResponseDTO>();

        
        CreateMap<Teacher, TeacherResponseDTO>()
            .ConstructUsing(src => new TeacherResponseDTO(
                src.Id, 
                src.Userid, 
                src.Name, 
                src.Phonenumber, 
                src.Joiningdate, 
                src.Qualifications, 
                null, 
                src.User != null ? src.User.Username : string.Empty, 
                src.Classes.FirstOrDefault() != null ? src.Classes.FirstOrDefault()!.Classname : null, 
                src.Classes.FirstOrDefault() != null ? src.Classes.FirstOrDefault()!.Section : null,
                src.User != null ? src.User.Email : null,
                src.Teacherdocuments.FirstOrDefault(d => d.Documenttype == "Photo" || d.Documentname == "Photo") != null 
                    ? src.Teacherdocuments.FirstOrDefault(d => d.Documenttype == "Photo" || d.Documentname == "Photo")!.Bloburl 
                    : null,
                src.Teachersubjects != null ? src.Teachersubjects.Count : 0,
                src.SubjectSpecialtyId,
                src.SubjectSpecialty != null ? src.SubjectSpecialty.Subjectname : null
            ));
        
        CreateMap<Teachersubject, TeacherSubjectResponseDTO>();

        
        CreateMap<Timetable, TimetableResponseDTO>();
        CreateMap<Academiccalendar, CalendarEventResponseDTO>();
        CreateMap<Examschedule, ExamScheduleResponseDTO>()
            .ConstructUsing(src => new ExamScheduleResponseDTO(
                src.Id,
                src.Examid,
                src.Subjectid,
                src.Subject != null ? src.Subject.Subjectname : string.Empty,
                src.Classid,
                src.Class != null ? src.Class.Classname : string.Empty,
                src.Class != null ? src.Class.Section : string.Empty,
                src.Examdate,
                src.Durationminutes,
                src.Session
            ));
    }
}
