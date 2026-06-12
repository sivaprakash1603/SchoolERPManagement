using SchoolERPManagementDALLibrary.Contexts;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementDALLibrary.Repositories;
using SchoolERPManagementBLLibrary.Helpers;
using SchoolERPManagementBLLibrary.Interfaces;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementBLLibrary.Strategies;
using SchoolERPManagementModelLibrary.Models;
using Microsoft.EntityFrameworkCore;
using SchoolERPManagementAPI.Middlewares;
using SchoolERPManagementAPI.Extensions;
using Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Serilog;
using Microsoft.Extensions.FileProviders;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.AddDbContext<SchoolERPDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.Configure<SchoolERPManagementBLLibrary.Configuration.SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));


builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
    };
});

builder.Services.AddControllers();
#region Repository
builder.Services.AddScoped<IRepository<int, Academicyear>, AbstractRepository<int, Academicyear>>();
builder.Services.AddScoped<IRepository<int, Asset>, AbstractRepository<int, Asset>>();
builder.Services.AddScoped<IRepository<int, Assetreport>, AbstractRepository<int, Assetreport>>();
builder.Services.AddScoped<IRepository<int, Assettype>, AbstractRepository<int, Assettype>>();
builder.Services.AddScoped<IRepository<int, Attendance>, AbstractRepository<int, Attendance>>();
builder.Services.AddScoped<IRepository<int, Class>, AbstractRepository<int, Class>>();
builder.Services.AddScoped<IRepository<int, Exam>, AbstractRepository<int, Exam>>();
builder.Services.AddScoped<IRepository<int, Examresult>, AbstractRepository<int, Examresult>>();
builder.Services.AddScoped<IRepository<int, Examschedule>, AbstractRepository<int, Examschedule>>();
builder.Services.AddScoped<IRepository<int, Feepayment>, AbstractRepository<int, Feepayment>>();
builder.Services.AddScoped<IRepository<int, Feestructure>, AbstractRepository<int, Feestructure>>();
builder.Services.AddScoped<IRepository<int, Homework>, AbstractRepository<int, Homework>>();
builder.Services.AddScoped<IRepository<int, Homeworksubmission>, AbstractRepository<int, Homeworksubmission>>();
builder.Services.AddScoped<IRepository<int, Student>, AbstractRepository<int, Student>>();
builder.Services.AddScoped<IRepository<int, Teacher>, AbstractRepository<int, Teacher>>();
builder.Services.AddScoped<IRepository<int, Subject>, AbstractRepository<int, Subject>>();
builder.Services.AddScoped<IRepository<int, Timetable>, AbstractRepository<int, Timetable>>();
builder.Services.AddScoped<IRepository<int, User>, AbstractRepository<int, User>>();
builder.Services.AddScoped<IRepository<int, Usernotification>, AbstractRepository<int, Usernotification>>();
builder.Services.AddScoped<IRepository<int, Salary>, AbstractRepository<int, Salary>>();
builder.Services.AddScoped<IRepository<int, Teachersubject>, AbstractRepository<int, Teachersubject>>();
builder.Services.AddScoped<IRepository<int, Stafftype>, AbstractRepository<int, Stafftype>>();
builder.Services.AddScoped<IRepository<int, Parent>, AbstractRepository<int, Parent>>();
builder.Services.AddScoped<IRepository<int, Role>, AbstractRepository<int, Role>>();
builder.Services.AddScoped<IRepository<int, Staff>, AbstractRepository<int, Staff>>();
builder.Services.AddScoped<IRepository<int, Studentenrollment>, AbstractRepository<int, Studentenrollment>>();
builder.Services.AddScoped<IRepository<int, Staffattendance>, AbstractRepository<int, Staffattendance>>();
builder.Services.AddScoped<IRepository<int, Studentdocument>, AbstractRepository<int, Studentdocument>>();
builder.Services.AddScoped<IRepository<int, Teacherdocument>, AbstractRepository<int, Teacherdocument>>();
builder.Services.AddScoped<IRepository<int, Parentdocument>, AbstractRepository<int, Parentdocument>>();
builder.Services.AddScoped<IRepository<int, Notification>, AbstractRepository<int, Notification>>();
#endregion

#region Helpers 
builder.Services.AddScoped<JwtTokenGenerator>();
#endregion

#region Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IParentService, ParentService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IFeeService, FeeService>();
builder.Services.AddScoped<IHomeworkService, HomeworkService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<ITimetableService, TimetableService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<IStaffAttendanceService, StaffAttendanceService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IPaymentGatewayService, StripePaymentService>();

builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<SchoolERPManagementBLLibrary.Profiles.AppMappingProfile>();
});


builder.Services.AddScoped<IDocumentVerificationStrategy, StudentDocumentVerificationStrategy>();
builder.Services.AddScoped<IDocumentVerificationStrategy, TeacherDocumentVerificationStrategy>();
builder.Services.AddScoped<IDocumentVerificationStrategy, ParentDocumentVerificationStrategy>();
#endregion








var app = builder.Build();


app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var uploadsPath = Path.Combine(AppContext.BaseDirectory, "wwwroot", "uploads");
if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


app.SeedData();

app.Run();
