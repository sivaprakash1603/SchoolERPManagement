using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SchoolERPManagementDALLibrary.Migrations
{
    
    public partial class initial : Migration
    {
        
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academicyears",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    yearname = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    startdate = table.Column<DateOnly>(type: "date", nullable: false),
                    enddate = table.Column<DateOnly>(type: "date", nullable: false),
                    iscurrent = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("academicyears_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "assettypes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    typename = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("assettypes_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    rolename = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("roles_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stafftypes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    typename = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("stafftypes_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subjectname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("subjects_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "exams",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    examname = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    academicyearid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("exams_pkey", x => x.id);
                    table.ForeignKey(
                        name: "exams_academicyearid_fkey",
                        column: x => x.academicyearid,
                        principalTable: "academicyears",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    passwordhash = table.Column<string>(type: "text", nullable: false),
                    roleid = table.Column<int>(type: "integer", nullable: false),
                    isactive = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("users_pkey", x => x.id);
                    table.ForeignKey(
                        name: "users_roleid_fkey",
                        column: x => x.roleid,
                        principalTable: "roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    createdbyuserid = table.Column<int>(type: "integer", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("notifications_pkey", x => x.id);
                    table.ForeignKey(
                        name: "notifications_createdbyuserid_fkey",
                        column: x => x.createdbyuserid,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "parents",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    relation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    phonenumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("parents_pkey", x => x.id);
                    table.ForeignKey(
                        name: "parents_userid_fkey",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "staff",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    stafftypeid = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phonenumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    joiningdate = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("staff_pkey", x => x.id);
                    table.ForeignKey(
                        name: "staff_stafftypeid_fkey",
                        column: x => x.stafftypeid,
                        principalTable: "stafftypes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "staff_userid_fkey",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "teachers",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    phonenumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    joiningdate = table.Column<DateOnly>(type: "date", nullable: true),
                    qualifications = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("teachers_pkey", x => x.id);
                    table.ForeignKey(
                        name: "teachers_userid_fkey",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "usernotifications",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    notificationid = table.Column<int>(type: "integer", nullable: false),
                    isread = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("usernotifications_pkey", x => x.id);
                    table.ForeignKey(
                        name: "usernotifications_notificationid_fkey",
                        column: x => x.notificationid,
                        principalTable: "notifications",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "usernotifications_userid_fkey",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "parentdocuments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parentid = table.Column<int>(type: "integer", nullable: false),
                    documenttype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bloburl = table.Column<string>(type: "text", nullable: false),
                    uploadedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("parentdocuments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "parentdocuments_parentid_fkey",
                        column: x => x.parentid,
                        principalTable: "parents",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userid = table.Column<int>(type: "integer", nullable: false),
                    regno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    bloodgroup = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    parentid = table.Column<int>(type: "integer", nullable: true),
                    dateofbirth = table.Column<DateOnly>(type: "date", nullable: true),
                    admissiondate = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "CURRENT_DATE")
                },
                constraints: table =>
                {
                    table.PrimaryKey("students_pkey", x => x.id);
                    table.ForeignKey(
                        name: "students_parentid_fkey",
                        column: x => x.parentid,
                        principalTable: "parents",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "students_userid_fkey",
                        column: x => x.userid,
                        principalTable: "users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "classes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    classname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    section = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    classteacherid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("classes_pkey", x => x.id);
                    table.ForeignKey(
                        name: "classes_classteacherid_fkey",
                        column: x => x.classteacherid,
                        principalTable: "teachers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "salaries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    staffid = table.Column<int>(type: "integer", nullable: true),
                    teacherid = table.Column<int>(type: "integer", nullable: true),
                    currentsalary = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    lastsalarydate = table.Column<DateOnly>(type: "date", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("salaries_pkey", x => x.id);
                    table.ForeignKey(
                        name: "salaries_staffid_fkey",
                        column: x => x.staffid,
                        principalTable: "staff",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "salaries_teacherid_fkey",
                        column: x => x.teacherid,
                        principalTable: "teachers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "teacherdocuments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    teacherid = table.Column<int>(type: "integer", nullable: false),
                    documenttype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bloburl = table.Column<string>(type: "text", nullable: false),
                    uploadedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("teacherdocuments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "teacherdocuments_teacherid_fkey",
                        column: x => x.teacherid,
                        principalTable: "teachers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "attendance",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    studentid = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    markedbyteacherid = table.Column<int>(type: "integer", nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("attendance_pkey", x => x.id);
                    table.ForeignKey(
                        name: "attendance_markedbyteacherid_fkey",
                        column: x => x.markedbyteacherid,
                        principalTable: "teachers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "attendance_studentid_fkey",
                        column: x => x.studentid,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "examresults",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    examid = table.Column<int>(type: "integer", nullable: false),
                    subjectid = table.Column<int>(type: "integer", nullable: false),
                    studentid = table.Column<int>(type: "integer", nullable: false),
                    marks = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    uploadedcorrectedanswersheeturl = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("examresults_pkey", x => x.id);
                    table.ForeignKey(
                        name: "examresults_examid_fkey",
                        column: x => x.examid,
                        principalTable: "exams",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "examresults_studentid_fkey",
                        column: x => x.studentid,
                        principalTable: "students",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "examresults_subjectid_fkey",
                        column: x => x.subjectid,
                        principalTable: "subjects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "feepayments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    studentid = table.Column<int>(type: "integer", nullable: false),
                    amountpaid = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    paymentdate = table.Column<DateOnly>(type: "date", nullable: true, defaultValueSql: "CURRENT_DATE"),
                    paymentmethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    transactionid = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("feepayments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "feepayments_studentid_fkey",
                        column: x => x.studentid,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "studentdocuments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    studentid = table.Column<int>(type: "integer", nullable: false),
                    documenttype = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bloburl = table.Column<string>(type: "text", nullable: false),
                    uploadedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("studentdocuments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "studentdocuments_studentid_fkey",
                        column: x => x.studentid,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "assets",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    assetname = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    assettypeid = table.Column<int>(type: "integer", nullable: true),
                    purchasedate = table.Column<DateOnly>(type: "date", nullable: true),
                    warrantyexpiry = table.Column<DateOnly>(type: "date", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    assignedclassid = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("assets_pkey", x => x.id);
                    table.ForeignKey(
                        name: "assets_assettypeid_fkey",
                        column: x => x.assettypeid,
                        principalTable: "assettypes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "assets_assignedclassid_fkey",
                        column: x => x.assignedclassid,
                        principalTable: "classes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "examschedules",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    examid = table.Column<int>(type: "integer", nullable: false),
                    subjectid = table.Column<int>(type: "integer", nullable: false),
                    classid = table.Column<int>(type: "integer", nullable: false),
                    examdate = table.Column<DateOnly>(type: "date", nullable: false),
                    durationminutes = table.Column<int>(type: "integer", nullable: false),
                    session = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("examschedules_pkey", x => x.id);
                    table.ForeignKey(
                        name: "examschedules_classid_fkey",
                        column: x => x.classid,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "examschedules_examid_fkey",
                        column: x => x.examid,
                        principalTable: "exams",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "examschedules_subjectid_fkey",
                        column: x => x.subjectid,
                        principalTable: "subjects",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "feestructures",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    classid = table.Column<int>(type: "integer", nullable: false),
                    academicyearid = table.Column<int>(type: "integer", nullable: false),
                    totalamount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    feename = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("feestructures_pkey", x => x.id);
                    table.ForeignKey(
                        name: "feestructures_academicyearid_fkey",
                        column: x => x.academicyearid,
                        principalTable: "academicyears",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "feestructures_classid_fkey",
                        column: x => x.classid,
                        principalTable: "classes",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "homework",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subjectid = table.Column<int>(type: "integer", nullable: false),
                    teacherid = table.Column<int>(type: "integer", nullable: false),
                    classid = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    attachmenturl = table.Column<string>(type: "text", nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP"),
                    duedate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("homework_pkey", x => x.id);
                    table.ForeignKey(
                        name: "homework_classid_fkey",
                        column: x => x.classid,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "homework_subjectid_fkey",
                        column: x => x.subjectid,
                        principalTable: "subjects",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "homework_teacherid_fkey",
                        column: x => x.teacherid,
                        principalTable: "teachers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "studentenrollments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    studentid = table.Column<int>(type: "integer", nullable: false),
                    classid = table.Column<int>(type: "integer", nullable: false),
                    academicyearid = table.Column<int>(type: "integer", nullable: false),
                    rollnumber = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("studentenrollments_pkey", x => x.id);
                    table.ForeignKey(
                        name: "studentenrollments_academicyearid_fkey",
                        column: x => x.academicyearid,
                        principalTable: "academicyears",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "studentenrollments_classid_fkey",
                        column: x => x.classid,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "studentenrollments_studentid_fkey",
                        column: x => x.studentid,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "teachersubjects",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    teacherid = table.Column<int>(type: "integer", nullable: false),
                    subjectid = table.Column<int>(type: "integer", nullable: false),
                    classid = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("teachersubjects_pkey", x => x.id);
                    table.ForeignKey(
                        name: "teachersubjects_classid_fkey",
                        column: x => x.classid,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "teachersubjects_subjectid_fkey",
                        column: x => x.subjectid,
                        principalTable: "subjects",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "teachersubjects_teacherid_fkey",
                        column: x => x.teacherid,
                        principalTable: "teachers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "timetables",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    classid = table.Column<int>(type: "integer", nullable: false),
                    subjectid = table.Column<int>(type: "integer", nullable: false),
                    teacherid = table.Column<int>(type: "integer", nullable: false),
                    dayofweek = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    starttime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    endtime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    roomno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("timetables_pkey", x => x.id);
                    table.ForeignKey(
                        name: "timetables_classid_fkey",
                        column: x => x.classid,
                        principalTable: "classes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "timetables_subjectid_fkey",
                        column: x => x.subjectid,
                        principalTable: "subjects",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "timetables_teacherid_fkey",
                        column: x => x.teacherid,
                        principalTable: "teachers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "assetreports",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    assetid = table.Column<int>(type: "integer", nullable: false),
                    report = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    createdat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("assetreports_pkey", x => x.id);
                    table.ForeignKey(
                        name: "assetreports_assetid_fkey",
                        column: x => x.assetid,
                        principalTable: "assets",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "homeworksubmissions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    homeworkid = table.Column<int>(type: "integer", nullable: false),
                    studentid = table.Column<int>(type: "integer", nullable: false),
                    uploadedfileurl = table.Column<string>(type: "text", nullable: true),
                    verificationstatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    marks = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    remarks = table.Column<string>(type: "text", nullable: true),
                    submittedat = table.Column<DateTime>(type: "timestamp without time zone", nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("homeworksubmissions_pkey", x => x.id);
                    table.ForeignKey(
                        name: "homeworksubmissions_homeworkid_fkey",
                        column: x => x.homeworkid,
                        principalTable: "homework",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "homeworksubmissions_studentid_fkey",
                        column: x => x.studentid,
                        principalTable: "students",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_assetreports_assetid",
                table: "assetreports",
                column: "assetid");

            migrationBuilder.CreateIndex(
                name: "IX_assets_assettypeid",
                table: "assets",
                column: "assettypeid");

            migrationBuilder.CreateIndex(
                name: "IX_assets_assignedclassid",
                table: "assets",
                column: "assignedclassid");

            migrationBuilder.CreateIndex(
                name: "assettypes_typename_key",
                table: "assettypes",
                column: "typename",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "attendance_studentid_date_key",
                table: "attendance",
                columns: new[] { "studentid", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_attendance_student_date",
                table: "attendance",
                columns: new[] { "studentid", "date" });

            migrationBuilder.CreateIndex(
                name: "IX_attendance_markedbyteacherid",
                table: "attendance",
                column: "markedbyteacherid");

            migrationBuilder.CreateIndex(
                name: "IX_classes_classteacherid",
                table: "classes",
                column: "classteacherid");

            migrationBuilder.CreateIndex(
                name: "examresults_examid_subjectid_studentid_key",
                table: "examresults",
                columns: new[] { "examid", "subjectid", "studentid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_examresults_student",
                table: "examresults",
                column: "studentid");

            migrationBuilder.CreateIndex(
                name: "IX_examresults_subjectid",
                table: "examresults",
                column: "subjectid");

            migrationBuilder.CreateIndex(
                name: "IX_exams_academicyearid",
                table: "exams",
                column: "academicyearid");

            migrationBuilder.CreateIndex(
                name: "IX_examschedules_classid",
                table: "examschedules",
                column: "classid");

            migrationBuilder.CreateIndex(
                name: "IX_examschedules_examid",
                table: "examschedules",
                column: "examid");

            migrationBuilder.CreateIndex(
                name: "IX_examschedules_subjectid",
                table: "examschedules",
                column: "subjectid");

            migrationBuilder.CreateIndex(
                name: "idx_feepayments_student",
                table: "feepayments",
                column: "studentid");

            migrationBuilder.CreateIndex(
                name: "IX_feestructures_academicyearid",
                table: "feestructures",
                column: "academicyearid");

            migrationBuilder.CreateIndex(
                name: "IX_feestructures_classid",
                table: "feestructures",
                column: "classid");

            migrationBuilder.CreateIndex(
                name: "IX_homework_classid",
                table: "homework",
                column: "classid");

            migrationBuilder.CreateIndex(
                name: "IX_homework_subjectid",
                table: "homework",
                column: "subjectid");

            migrationBuilder.CreateIndex(
                name: "IX_homework_teacherid",
                table: "homework",
                column: "teacherid");

            migrationBuilder.CreateIndex(
                name: "idx_homeworksubmission_student",
                table: "homeworksubmissions",
                column: "studentid");

            migrationBuilder.CreateIndex(
                name: "IX_homeworksubmissions_homeworkid",
                table: "homeworksubmissions",
                column: "homeworkid");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_createdbyuserid",
                table: "notifications",
                column: "createdbyuserid");

            migrationBuilder.CreateIndex(
                name: "IX_parentdocuments_parentid",
                table: "parentdocuments",
                column: "parentid");

            migrationBuilder.CreateIndex(
                name: "parents_userid_key",
                table: "parents",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "roles_rolename_key",
                table: "roles",
                column: "rolename",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_salaries_staffid",
                table: "salaries",
                column: "staffid");

            migrationBuilder.CreateIndex(
                name: "IX_salaries_teacherid",
                table: "salaries",
                column: "teacherid");

            migrationBuilder.CreateIndex(
                name: "IX_staff_stafftypeid",
                table: "staff",
                column: "stafftypeid");

            migrationBuilder.CreateIndex(
                name: "staff_userid_key",
                table: "staff",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "stafftypes_typename_key",
                table: "stafftypes",
                column: "typename",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_studentdocuments_studentid",
                table: "studentdocuments",
                column: "studentid");

            migrationBuilder.CreateIndex(
                name: "IX_studentenrollments_academicyearid",
                table: "studentenrollments",
                column: "academicyearid");

            migrationBuilder.CreateIndex(
                name: "IX_studentenrollments_classid",
                table: "studentenrollments",
                column: "classid");

            migrationBuilder.CreateIndex(
                name: "studentenrollments_studentid_academicyearid_key",
                table: "studentenrollments",
                columns: new[] { "studentid", "academicyearid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_students_regno",
                table: "students",
                column: "regno");

            migrationBuilder.CreateIndex(
                name: "IX_students_parentid",
                table: "students",
                column: "parentid");

            migrationBuilder.CreateIndex(
                name: "students_regno_key",
                table: "students",
                column: "regno",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "students_userid_key",
                table: "students",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "subjects_subjectname_key",
                table: "subjects",
                column: "subjectname",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teacherdocuments_teacherid",
                table: "teacherdocuments",
                column: "teacherid");

            migrationBuilder.CreateIndex(
                name: "teachers_userid_key",
                table: "teachers",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_teachersubjects_classid",
                table: "teachersubjects",
                column: "classid");

            migrationBuilder.CreateIndex(
                name: "IX_teachersubjects_subjectid",
                table: "teachersubjects",
                column: "subjectid");

            migrationBuilder.CreateIndex(
                name: "teachersubjects_teacherid_subjectid_classid_key",
                table: "teachersubjects",
                columns: new[] { "teacherid", "subjectid", "classid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_timetables_classid",
                table: "timetables",
                column: "classid");

            migrationBuilder.CreateIndex(
                name: "IX_timetables_subjectid",
                table: "timetables",
                column: "subjectid");

            migrationBuilder.CreateIndex(
                name: "IX_timetables_teacherid",
                table: "timetables",
                column: "teacherid");

            migrationBuilder.CreateIndex(
                name: "IX_usernotifications_notificationid",
                table: "usernotifications",
                column: "notificationid");

            migrationBuilder.CreateIndex(
                name: "IX_usernotifications_userid",
                table: "usernotifications",
                column: "userid");

            migrationBuilder.CreateIndex(
                name: "IX_users_roleid",
                table: "users",
                column: "roleid");

            migrationBuilder.CreateIndex(
                name: "users_email_key",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "users_username_key",
                table: "users",
                column: "username",
                unique: true);
        }

        
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assetreports");

            migrationBuilder.DropTable(
                name: "attendance");

            migrationBuilder.DropTable(
                name: "examresults");

            migrationBuilder.DropTable(
                name: "examschedules");

            migrationBuilder.DropTable(
                name: "feepayments");

            migrationBuilder.DropTable(
                name: "feestructures");

            migrationBuilder.DropTable(
                name: "homeworksubmissions");

            migrationBuilder.DropTable(
                name: "parentdocuments");

            migrationBuilder.DropTable(
                name: "salaries");

            migrationBuilder.DropTable(
                name: "studentdocuments");

            migrationBuilder.DropTable(
                name: "studentenrollments");

            migrationBuilder.DropTable(
                name: "teacherdocuments");

            migrationBuilder.DropTable(
                name: "teachersubjects");

            migrationBuilder.DropTable(
                name: "timetables");

            migrationBuilder.DropTable(
                name: "usernotifications");

            migrationBuilder.DropTable(
                name: "assets");

            migrationBuilder.DropTable(
                name: "exams");

            migrationBuilder.DropTable(
                name: "homework");

            migrationBuilder.DropTable(
                name: "staff");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "assettypes");

            migrationBuilder.DropTable(
                name: "academicyears");

            migrationBuilder.DropTable(
                name: "classes");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "stafftypes");

            migrationBuilder.DropTable(
                name: "parents");

            migrationBuilder.DropTable(
                name: "teachers");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
