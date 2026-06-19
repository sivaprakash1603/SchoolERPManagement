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
        CreateMap<Staffattendance, StaffAttendanceResponseDTO>()
            .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty));

        
        CreateMap<User, AuthResponseDTO>()
            .ConstructUsing(src => new AuthResponseDTO(src.Id, src.Username, src.Email, src.Roleid, src.Role != null ? src.Role.Rolename : string.Empty, ""));

        
        CreateMap<Class, ClassResponseDTO>();

        
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
                src.Id, src.Subjectid, src.Teacherid, src.Classid, src.Title, src.Description, src.Attachmenturl, src.Createdat, src.Duedate, 
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
            .ConstructUsing(src => new ParentResponseDTO(src.Id, src.Userid, src.Name, null, src.Phonenumber, null, null, null));

        
        CreateMap<Student, StudentResponseDTO>()
            .ConstructUsing(src => new StudentResponseDTO(src.Id, src.Userid, src.Regno, src.Name, null, null));

        
        CreateMap<Subject, SubjectResponseDTO>();

        
        CreateMap<Teacher, TeacherResponseDTO>()
            .ConstructUsing(src => new TeacherResponseDTO(
                src.Id, src.Userid, src.Name, src.Phonenumber, src.Joiningdate, src.Qualifications, null, 
                src.User != null ? src.User.Username : string.Empty, 
                src.Classes.FirstOrDefault() != null ? src.Classes.FirstOrDefault()!.Classname : null, 
                src.Classes.FirstOrDefault() != null ? src.Classes.FirstOrDefault()!.Section : null));
        
        CreateMap<Teachersubject, TeacherSubjectResponseDTO>();

        
        CreateMap<Timetable, TimetableResponseDTO>();
    }
}
