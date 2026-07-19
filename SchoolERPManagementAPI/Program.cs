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
using SchoolERPManagementAPI.Hubs;
using SchoolERPManagementAPI.Services;
using Stripe;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Serilog;
using Microsoft.Extensions.FileProviders;
using FluentValidation;
using SchoolERPManagementAPI.Filters;
using SchoolERPManagementBLLibrary.Validators;
using QuestPDF.Infrastructure;
using Azure.Identity;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Configure QuestPDF Community License
QuestPDF.Settings.License = LicenseType.Community;

builder.Host.UseSerilog((context, configuration) => 
{
    configuration.ReadFrom.Configuration(context.Configuration);
    
    var blobConnString = context.Configuration["AzureBlobStorage:ConnectionString"] ?? context.Configuration["AzureBlobStorage--ConnectionString"];
    if (!string.IsNullOrEmpty(blobConnString))
    {
        configuration.WriteTo.AzureBlobStorage(
            connectionString: blobConnString,
            storageContainerName: "school-erp-logs",
            storageFileName: "logs/log-{yyyyMMdd}.txt"
        );
    }
});

var keyVaultUrl = builder.Configuration["KeyVault:Url"];
if (!string.IsNullOrEmpty(keyVaultUrl))
{
    builder.Configuration.AddAzureKeyVault(
        new Uri(keyVaultUrl),
        new DefaultAzureCredential());
}

StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

builder.Services.AddDbContext<SchoolERPDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default"), 
        o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
builder.Services.AddHealthChecks()
    .AddDbContextCheck<SchoolERPDbContext>();

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
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notification"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .SetIsOriginAllowed(origin => true)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
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
                PermitLimit = 500,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429;
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
    };
});

builder.Services.AddScoped<ActiveUserFilterAttribute>();

builder.Services.AddControllers(options => 
{
    options.Filters.Add<ValidationFilterAttribute>();
    options.Filters.AddService<ActiveUserFilterAttribute>();
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new SchoolERPManagementAPI.Converters.IstDateTimeConverter());
    options.JsonSerializerOptions.Converters.Add(new SchoolERPManagementAPI.Converters.IstNullableDateTimeConverter());
});
#region Repository
builder.Services.AddScoped<IRepository<int, Academicyear>, AbstractRepository<int, Academicyear>>();
builder.Services.AddScoped<IRepository<int, Academiccalendar>, AbstractRepository<int, Academiccalendar>>();
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
builder.Services.AddScoped<IRepository<int, Classsubject>, AbstractRepository<int, Classsubject>>();
builder.Services.AddScoped<IRepository<int, Parentteachermeeting>, AbstractRepository<int, Parentteachermeeting>>();
builder.Services.AddScoped<IRepository<int, Parentteacherslot>, AbstractRepository<int, Parentteacherslot>>();
#endregion

#region Helpers 
builder.Services.AddScoped<JwtTokenGenerator>();
#endregion

#region Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAcademicCalendarService, AcademicCalendarService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IParentService, ParentService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IFeeService, FeeService>();
builder.Services.AddScoped<ISystemSetupService, SystemSetupService>();
builder.Services.AddScoped<IHomeworkService, HomeworkService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<ITimetableService, TimetableService>();
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
builder.Services.AddScoped<INotificationPusher, SignalRNotificationPusher>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IClassService, ClassService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
builder.Services.AddScoped<IStaffAttendanceService, StaffAttendanceService>();
builder.Services.AddScoped<ISubjectService, SubjectService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IFileStorageService, AzureBlobStorageService>();
builder.Services.AddScoped<IPaymentGatewayService, StripePaymentService>();
builder.Services.AddScoped<IParentTeacherMeetingService, ParentTeacherMeetingService>();

builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<SchoolERPManagementBLLibrary.Profiles.AppMappingProfile>();
});

var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrEmpty(redisConnectionString))
{
    builder.Services.AddSignalR().AddStackExchangeRedis(redisConnectionString);
}
else
{
    builder.Services.AddSignalR();
}

builder.Services.AddScoped<IDocumentVerificationStrategy, StudentDocumentVerificationStrategy>();
builder.Services.AddScoped<IDocumentVerificationStrategy, TeacherDocumentVerificationStrategy>();
builder.Services.AddScoped<IDocumentVerificationStrategy, ParentDocumentVerificationStrategy>();

// Register Reminder Services
builder.Services.AddScoped<HomeworkDueReminderService>();
builder.Services.AddScoped<FeesDueReminderService>();

// Register FluentValidation Validators from the assembly containing our validators
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
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
app.UseMiddleware<UserLoggingMiddleware>();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notification");
app.MapHealthChecks("/health/db");


app.SeedData();

app.Run();
