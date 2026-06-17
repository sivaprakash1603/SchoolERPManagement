using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SchoolERPManagementModelLibrary.Models;

namespace SchoolERPManagementDALLibrary.Contexts;

public partial class SchoolERPDbContext : DbContext
{
    public SchoolERPDbContext()
    {
    }

    public SchoolERPDbContext(DbContextOptions<SchoolERPDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Academicyear> Academicyears { get; set; }

    public virtual DbSet<Asset> Assets { get; set; }

    public virtual DbSet<Assetreport> Assetreports { get; set; }

    public virtual DbSet<Assettype> Assettypes { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Exam> Exams { get; set; }

    public virtual DbSet<Examresult> Examresults { get; set; }

    public virtual DbSet<Examschedule> Examschedules { get; set; }

    public virtual DbSet<Feepayment> Feepayments { get; set; }

    public virtual DbSet<Feestructure> Feestructures { get; set; }

    public virtual DbSet<Homework> Homeworks { get; set; }

    public virtual DbSet<Homeworksubmission> Homeworksubmissions { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Parent> Parents { get; set; }

    public virtual DbSet<Parentdocument> Parentdocuments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Salary> Salaries { get; set; }

    public virtual DbSet<Staff> Staff { get; set; }

    public virtual DbSet<Staffattendance> Staffattendances { get; set; }

    public virtual DbSet<Stafftype> Stafftypes { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<Studentdocument> Studentdocuments { get; set; }

    public virtual DbSet<Studentenrollment> Studentenrollments { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Teacher> Teachers { get; set; }

    public virtual DbSet<Teacherdocument> Teacherdocuments { get; set; }

    public virtual DbSet<Teachersubject> Teachersubjects { get; set; }

    public virtual DbSet<Timetable> Timetables { get; set; }

    public virtual DbSet<User> Users { get; set; }
    
    public virtual DbSet<Usernotification> Usernotifications { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=SchoolERPSystem;Username=postgres;Password=postgres");

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTime>()
            .HaveConversion<DateTimeToUtcConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Academicyear>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("academicyears_pkey");

            entity.ToTable("academicyears");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Enddate).HasColumnName("enddate");
            entity.Property(e => e.Iscurrent)
                .HasDefaultValue(false)
                .HasColumnName("iscurrent");
            entity.Property(e => e.Startdate).HasColumnName("startdate");
            entity.Property(e => e.Yearname)
                .HasMaxLength(20)
                .HasColumnName("yearname");
        });

        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("assets_pkey");

            entity.ToTable("assets");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Assetname)
                .HasMaxLength(150)
                .HasColumnName("assetname");
            entity.Property(e => e.Assettypeid).HasColumnName("assettypeid");
            entity.Property(e => e.Assignedclassid).HasColumnName("assignedclassid");
            entity.Property(e => e.Purchasedate).HasColumnName("purchasedate");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Warrantyexpiry).HasColumnName("warrantyexpiry");

            entity.HasOne(d => d.Assettype).WithMany(p => p.Assets)
                .HasForeignKey(d => d.Assettypeid)
                .HasConstraintName("assets_assettypeid_fkey");

            entity.HasOne(d => d.Assignedclass).WithMany(p => p.Assets)
                .HasForeignKey(d => d.Assignedclassid)
                .HasConstraintName("assets_assignedclassid_fkey");
        });

        modelBuilder.Entity<Assetreport>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("assetreports_pkey");

            entity.ToTable("assetreports");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Assetid).HasColumnName("assetid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Report).HasColumnName("report");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Asset).WithMany(p => p.Assetreports)
                .HasForeignKey(d => d.Assetid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("assetreports_assetid_fkey");
        });

        modelBuilder.Entity<Assettype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("assettypes_pkey");

            entity.ToTable("assettypes");

            entity.HasIndex(e => e.Typename, "assettypes_typename_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Typename)
                .HasMaxLength(100)
                .HasColumnName("typename");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("attendance_pkey");

            entity.ToTable("attendance");

            entity.HasIndex(e => new { e.Studentid, e.Date }, "attendance_studentid_date_key").IsUnique();

            entity.HasIndex(e => new { e.Studentid, e.Date }, "idx_attendance_student_date");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Date).HasColumnName("date");
            entity.Property(e => e.Markedbyteacherid).HasColumnName("markedbyteacherid");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.Studentid).HasColumnName("studentid");

            entity.HasOne(d => d.Markedbyteacher).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.Markedbyteacherid)
                .HasConstraintName("attendance_markedbyteacherid_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.Studentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("attendance_studentid_fkey");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("classes_pkey");

            entity.ToTable("classes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Classname)
                .HasMaxLength(50)
                .HasColumnName("classname");
            entity.Property(e => e.Classteacherid).HasColumnName("classteacherid");
            entity.Property(e => e.Section)
                .HasMaxLength(10)
                .HasColumnName("section");

            entity.HasOne(d => d.Classteacher).WithMany(p => p.Classes)
                .HasForeignKey(d => d.Classteacherid)
                .HasConstraintName("classes_classteacherid_fkey");
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("exams_pkey");

            entity.ToTable("exams");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Academicyearid).HasColumnName("academicyearid");
            entity.Property(e => e.Examname)
                .HasMaxLength(100)
                .HasColumnName("examname");

            entity.HasOne(d => d.Academicyear).WithMany(p => p.Exams)
                .HasForeignKey(d => d.Academicyearid)
                .HasConstraintName("exams_academicyearid_fkey");
        });

        modelBuilder.Entity<Examresult>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("examresults_pkey");

            entity.ToTable("examresults");

            entity.HasIndex(e => new { e.Examid, e.Subjectid, e.Studentid }, "examresults_examid_subjectid_studentid_key").IsUnique();

            entity.HasIndex(e => e.Studentid, "idx_examresults_student");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Examid).HasColumnName("examid");
            entity.Property(e => e.Marks)
                .HasPrecision(5, 2)
                .HasColumnName("marks");
            entity.Property(e => e.Studentid).HasColumnName("studentid");
            entity.Property(e => e.Subjectid).HasColumnName("subjectid");
            entity.Property(e => e.Uploadedcorrectedanswersheeturl).HasColumnName("uploadedcorrectedanswersheeturl");

            entity.HasOne(d => d.Exam).WithMany(p => p.Examresults)
                .HasForeignKey(d => d.Examid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("examresults_examid_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.Examresults)
                .HasForeignKey(d => d.Studentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("examresults_studentid_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.Examresults)
                .HasForeignKey(d => d.Subjectid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("examresults_subjectid_fkey");
        });

        modelBuilder.Entity<Examschedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("examschedules_pkey");

            entity.ToTable("examschedules");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Durationminutes).HasColumnName("durationminutes");
            entity.Property(e => e.Examdate).HasColumnName("examdate");
            entity.Property(e => e.Examid).HasColumnName("examid");
            entity.Property(e => e.Session)
                .HasMaxLength(50)
                .HasColumnName("session");
            entity.Property(e => e.Subjectid).HasColumnName("subjectid");

            entity.HasOne(d => d.Class).WithMany(p => p.Examschedules)
                .HasForeignKey(d => d.Classid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("examschedules_classid_fkey");

            entity.HasOne(d => d.Exam).WithMany(p => p.Examschedules)
                .HasForeignKey(d => d.Examid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("examschedules_examid_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.Examschedules)
                .HasForeignKey(d => d.Subjectid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("examschedules_subjectid_fkey");
        });

        modelBuilder.Entity<Feepayment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("feepayments_pkey");

            entity.ToTable("feepayments");

            entity.HasIndex(e => e.Studentid, "idx_feepayments_student");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amountpaid)
                .HasPrecision(10, 2)
                .HasColumnName("amountpaid");
            entity.Property(e => e.Paymentdate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("paymentdate");
            entity.Property(e => e.Paymentmethod)
                .HasMaxLength(50)
                .HasColumnName("paymentmethod");
            entity.Property(e => e.Studentid).HasColumnName("studentid");
            entity.Property(e => e.Transactionid)
                .HasMaxLength(150)
                .HasColumnName("transactionid");

            entity.HasOne(d => d.Student).WithMany(p => p.Feepayments)
                .HasForeignKey(d => d.Studentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("feepayments_studentid_fkey");
        });

        modelBuilder.Entity<Feestructure>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("feestructures_pkey");

            entity.ToTable("feestructures");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Academicyearid).HasColumnName("academicyearid");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Totalamount)
                .HasPrecision(10, 2)
                .HasColumnName("totalamount");

            entity.Property(e => e.Feename)
                .HasMaxLength(150)
                .HasColumnName("feename");

            entity.HasOne(d => d.Academicyear).WithMany(p => p.Feestructures)
                .HasForeignKey(d => d.Academicyearid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("feestructures_academicyearid_fkey");

            entity.HasOne(d => d.Class).WithMany(p => p.Feestructures)
                .HasForeignKey(d => d.Classid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("feestructures_classid_fkey");
        });

        modelBuilder.Entity<Homework>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("homework_pkey");

            entity.ToTable("homework");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Attachmenturl).HasColumnName("attachmenturl");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Duedate).HasColumnName("duedate");
            entity.Property(e => e.Subjectid).HasColumnName("subjectid");
            entity.Property(e => e.Teacherid).HasColumnName("teacherid");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Class).WithMany(p => p.Homeworks)
                .HasForeignKey(d => d.Classid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("homework_classid_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.Homeworks)
                .HasForeignKey(d => d.Subjectid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("homework_subjectid_fkey");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Homeworks)
                .HasForeignKey(d => d.Teacherid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("homework_teacherid_fkey");
        });

        modelBuilder.Entity<Homeworksubmission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("homeworksubmissions_pkey");

            entity.ToTable("homeworksubmissions");

            entity.HasIndex(e => e.Studentid, "idx_homeworksubmission_student");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Homeworkid).HasColumnName("homeworkid");
            entity.Property(e => e.Marks)
                .HasPrecision(5, 2)
                .HasColumnName("marks");
            entity.Property(e => e.Remarks).HasColumnName("remarks");
            entity.Property(e => e.Studentid).HasColumnName("studentid");
            entity.Property(e => e.Submittedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("submittedat");
            entity.Property(e => e.Uploadedfileurl).HasColumnName("uploadedfileurl");
            entity.Property(e => e.Verificationstatus)
                .HasMaxLength(50)
                .HasColumnName("verificationstatus");

            entity.HasOne(d => d.Homework).WithMany(p => p.Homeworksubmissions)
                .HasForeignKey(d => d.Homeworkid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("homeworksubmissions_homeworkid_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.Homeworksubmissions)
                .HasForeignKey(d => d.Studentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("homeworksubmissions_studentid_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Createdbyuserid).HasColumnName("createdbyuserid");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .HasColumnName("title");

            entity.HasOne(d => d.Createdbyuser).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.Createdbyuserid)
                .HasConstraintName("notifications_createdbyuserid_fkey");
        });

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("parents_pkey");

            entity.ToTable("parents");

            entity.HasIndex(e => e.Userid, "parents_userid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Relation)
                .HasMaxLength(50)
                .HasColumnName("relation");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithOne(p => p.Parent)
                .HasForeignKey<Parent>(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("parents_userid_fkey");
        });

        modelBuilder.Entity<Parentdocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("parentdocuments_pkey");

            entity.ToTable("parentdocuments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bloburl).HasColumnName("bloburl");
            entity.Property(e => e.Documenttype)
                .HasMaxLength(100)
                .HasColumnName("documenttype");
            entity.Property(e => e.Documentname)
                .HasMaxLength(255)
                .HasColumnName("documentname")
                .HasDefaultValue("");
            entity.Property(e => e.Parentid).HasColumnName("parentid");
            entity.Property(e => e.Uploadedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploadedat");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("status");

            entity.HasOne(d => d.Parent).WithMany(p => p.Parentdocuments)
                .HasForeignKey(d => d.Parentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("parentdocuments_parentid_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Rolename, "roles_rolename_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Rolename)
                .HasMaxLength(50)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<Salary>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("salaries_pkey");

            entity.ToTable("salaries");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Currentsalary)
                .HasPrecision(10, 2)
                .HasColumnName("currentsalary");
            entity.Property(e => e.Lastsalarydate).HasColumnName("lastsalarydate");
            entity.Property(e => e.Staffid).HasColumnName("staffid");
            entity.Property(e => e.Teacherid).HasColumnName("teacherid");

            entity.HasOne(d => d.Staff).WithMany(p => p.Salaries)
                .HasForeignKey(d => d.Staffid)
                .HasConstraintName("salaries_staffid_fkey");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Salaries)
                .HasForeignKey(d => d.Teacherid)
                .HasConstraintName("salaries_teacherid_fkey");
        });

        modelBuilder.Entity<Staff>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("staff_pkey");

            entity.ToTable("staff");

            entity.HasIndex(e => e.Userid, "staff_userid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Joiningdate).HasColumnName("joiningdate");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Stafftypeid).HasColumnName("stafftypeid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Stafftype).WithMany(p => p.Staff)
                .HasForeignKey(d => d.Stafftypeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("staff_stafftypeid_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.Staff)
                .HasForeignKey<Staff>(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("staff_userid_fkey");
        });

        modelBuilder.Entity<Stafftype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stafftypes_pkey");

            entity.ToTable("stafftypes");

            entity.HasIndex(e => e.Typename, "stafftypes_typename_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Typename)
                .HasMaxLength(100)
                .HasColumnName("typename");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("students_pkey");

            entity.ToTable("students");

            entity.HasIndex(e => e.Regno, "idx_students_regno");

            entity.HasIndex(e => e.Regno, "students_regno_key").IsUnique();

            entity.HasIndex(e => e.Userid, "students_userid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Admissiondate)
                .HasDefaultValueSql("CURRENT_DATE")
                .HasColumnName("admissiondate");
            entity.Property(e => e.Bloodgroup)
                .HasMaxLength(10)
                .HasColumnName("bloodgroup");
            entity.Property(e => e.Dateofbirth).HasColumnName("dateofbirth");
            entity.Property(e => e.Gender)
                .HasMaxLength(20)
                .HasColumnName("gender");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Parentid).HasColumnName("parentid");
            entity.Property(e => e.Regno)
                .HasMaxLength(50)
                .HasColumnName("regno");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Parent).WithMany(p => p.Students)
                .HasForeignKey(d => d.Parentid)
                .HasConstraintName("students_parentid_fkey");

            entity.HasOne(d => d.User).WithOne(p => p.Student)
                .HasForeignKey<Student>(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("students_userid_fkey");
        });

        modelBuilder.Entity<Studentdocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("studentdocuments_pkey");

            entity.ToTable("studentdocuments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bloburl).HasColumnName("bloburl");
            entity.Property(e => e.Documenttype)
                .HasMaxLength(100)
                .HasColumnName("documenttype");
            entity.Property(e => e.Documentname)
                .HasMaxLength(255)
                .HasColumnName("documentname")
                .HasDefaultValue("");
            entity.Property(e => e.Studentid).HasColumnName("studentid");
            entity.Property(e => e.Uploadedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploadedat");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("status");

            entity.HasOne(d => d.Student).WithMany(p => p.Studentdocuments)
                .HasForeignKey(d => d.Studentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("studentdocuments_studentid_fkey");
        });

        modelBuilder.Entity<Studentenrollment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("studentenrollments_pkey");

            entity.ToTable("studentenrollments");

            entity.HasIndex(e => new { e.Studentid, e.Academicyearid }, "studentenrollments_studentid_academicyearid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Academicyearid).HasColumnName("academicyearid");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Rollnumber).HasColumnName("rollnumber");
            entity.Property(e => e.Studentid).HasColumnName("studentid");

            entity.HasOne(d => d.Academicyear).WithMany(p => p.Studentenrollments)
                .HasForeignKey(d => d.Academicyearid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("studentenrollments_academicyearid_fkey");

            entity.HasOne(d => d.Class).WithMany(p => p.Studentenrollments)
                .HasForeignKey(d => d.Classid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("studentenrollments_classid_fkey");

            entity.HasOne(d => d.Student).WithMany(p => p.Studentenrollments)
                .HasForeignKey(d => d.Studentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("studentenrollments_studentid_fkey");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subjects_pkey");

            entity.ToTable("subjects");

            entity.HasIndex(e => e.Subjectname, "subjects_subjectname_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Subjectname)
                .HasMaxLength(100)
                .HasColumnName("subjectname");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("teachers_pkey");

            entity.ToTable("teachers");

            entity.HasIndex(e => e.Userid, "teachers_userid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Joiningdate).HasColumnName("joiningdate");
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .HasColumnName("name");
            entity.Property(e => e.Phonenumber)
                .HasMaxLength(20)
                .HasColumnName("phonenumber");
            entity.Property(e => e.Qualifications)
                .HasMaxLength(500)
                .HasColumnName("qualifications");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.User).WithOne(p => p.Teacher)
                .HasForeignKey<Teacher>(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("teachers_userid_fkey");
        });

        modelBuilder.Entity<Teacherdocument>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("teacherdocuments_pkey");

            entity.ToTable("teacherdocuments");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bloburl).HasColumnName("bloburl");
            entity.Property(e => e.Documenttype)
                .HasMaxLength(100)
                .HasColumnName("documenttype");
            entity.Property(e => e.Documentname)
                .HasMaxLength(255)
                .HasColumnName("documentname")
                .HasDefaultValue("");
            entity.Property(e => e.Teacherid).HasColumnName("teacherid");
            entity.Property(e => e.Uploadedat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("uploadedat");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("status");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Teacherdocuments)
                .HasForeignKey(d => d.Teacherid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("teacherdocuments_teacherid_fkey");
        });

        modelBuilder.Entity<Teachersubject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("teachersubjects_pkey");

            entity.ToTable("teachersubjects");

            entity.HasIndex(e => new { e.Teacherid, e.Subjectid, e.Classid }, "teachersubjects_teacherid_subjectid_classid_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Subjectid).HasColumnName("subjectid");
            entity.Property(e => e.Teacherid).HasColumnName("teacherid");

            entity.HasOne(d => d.Class).WithMany(p => p.Teachersubjects)
                .HasForeignKey(d => d.Classid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("teachersubjects_classid_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.Teachersubjects)
                .HasForeignKey(d => d.Subjectid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("teachersubjects_subjectid_fkey");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Teachersubjects)
                .HasForeignKey(d => d.Teacherid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("teachersubjects_teacherid_fkey");
        });

        modelBuilder.Entity<Timetable>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("timetables_pkey");

            entity.ToTable("timetables");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Classid).HasColumnName("classid");
            entity.Property(e => e.Dayofweek)
                .HasMaxLength(20)
                .HasColumnName("dayofweek");
            entity.Property(e => e.Endtime).HasColumnName("endtime");
            entity.Property(e => e.Roomno)
                .HasMaxLength(50)
                .HasColumnName("roomno");
            entity.Property(e => e.Starttime).HasColumnName("starttime");
            entity.Property(e => e.Subjectid).HasColumnName("subjectid");
            entity.Property(e => e.Teacherid).HasColumnName("teacherid");

            entity.HasOne(d => d.Class).WithMany(p => p.Timetables)
                .HasForeignKey(d => d.Classid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("timetables_classid_fkey");

            entity.HasOne(d => d.Subject).WithMany(p => p.Timetables)
                .HasForeignKey(d => d.Subjectid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("timetables_subjectid_fkey");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Timetables)
                .HasForeignKey(d => d.Teacherid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("timetables_teacherid_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Passwordhash).HasColumnName("passwordhash");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("users_roleid_fkey");
        });

        modelBuilder.Entity<Usernotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("usernotifications_pkey");

            entity.ToTable("usernotifications");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Isread)
                .HasDefaultValue(false)
                .HasColumnName("isread");
            entity.Property(e => e.Notificationid).HasColumnName("notificationid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Notification).WithMany(p => p.Usernotifications)
                .HasForeignKey(d => d.Notificationid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usernotifications_notificationid_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.Usernotifications)
                .HasForeignKey(d => d.Userid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("usernotifications_userid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

public class DateTimeToUtcConverter : ValueConverter<DateTime, DateTime>
{
    public DateTimeToUtcConverter()
        : base(
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
