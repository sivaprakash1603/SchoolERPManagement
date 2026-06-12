using AutoMapper;
using System.Linq;
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

        
        CreateMap<Attendance, AttendanceResponseDTO>();
        CreateMap<Staffattendance, StaffAttendanceResponseDTO>();

        
        CreateMap<User, AuthResponseDTO>()
            .ConstructUsing(src => new AuthResponseDTO(src.Id, src.Username, src.Email, src.Roleid, src.Role != null ? src.Role.Rolename : string.Empty, null));

        
        CreateMap<Class, ClassResponseDTO>();

        
        CreateMap<Studentdocument, StudentDocumentResponseDTO>();
        CreateMap<Teacherdocument, TeacherDocumentResponseDTO>();

        
        CreateMap<Exam, ExamResponseDTO>();
        CreateMap<Examresult, ExamResultResponseDTO>();

        
        CreateMap<Feepayment, FeePaymentResponseDTO>();
        CreateMap<Feestructure, FeeStructureResponseDTO>();
        CreateMap<Feestructure, FeeComponentDTO>()
            .ConstructUsing(src => new FeeComponentDTO(src.Id, src.Feename, src.Totalamount));

        
        CreateMap<Homework, HomeworkResponseDTO>();
        CreateMap<Homeworksubmission, HomeworkSubmissionResponseDTO>();

        
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
            .ConstructUsing(src => new ParentResponseDTO(src.Id, src.Userid, src.Name, src.Relation, src.Phonenumber, null));

        
        CreateMap<Student, StudentResponseDTO>()
            .ConstructUsing(src => new StudentResponseDTO(src.Id, src.Userid, src.Regno, src.Name, src.Parentid, null));

        
        CreateMap<Subject, SubjectResponseDTO>();

        
        CreateMap<Teacher, TeacherResponseDTO>()
            .ConstructUsing(src => new TeacherResponseDTO(
                src.Id, src.Userid, src.Name, src.Phonenumber, src.Joiningdate, src.Qualifications, null, 
                src.User != null ? src.User.Username : string.Empty, 
                src.Classes.FirstOrDefault() != null ? src.Classes.FirstOrDefault().Classname : null, 
                src.Classes.FirstOrDefault() != null ? src.Classes.FirstOrDefault().Section : null));
        
        CreateMap<Teachersubject, TeacherSubjectResponseDTO>();

        
        CreateMap<Timetable, TimetableResponseDTO>();
    }
}
