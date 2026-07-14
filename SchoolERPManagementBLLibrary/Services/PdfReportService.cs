using System;
using System.Collections.Generic;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SchoolERPManagementBLLibrary.DTOs.Asset;
using SchoolERPManagementBLLibrary.DTOs.Attendance;
using SchoolERPManagementBLLibrary.DTOs.Exam;
using SchoolERPManagementBLLibrary.DTOs.Fee;
using SchoolERPManagementBLLibrary.DTOs.StaffAttendance;
using SchoolERPManagementBLLibrary.Interfaces;

namespace SchoolERPManagementBLLibrary.Services;

public class PdfReportService : IPdfReportService
{
    private void ComposeHeader(IContainer container, string title)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("School ERP Management").FontSize(20).SemiBold().FontColor(Colors.Indigo.Darken2);
                column.Item().Text(title).FontSize(14).FontColor(Colors.Grey.Darken2);
                var istTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                column.Item().Text($"Generated on: {istTime:MMM dd, yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });
    }

    public byte[] GenerateFeeCollectionPdf(IReadOnlyList<FeePaymentResponseDTO> records)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, "Fee Collection Report"));

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Payment ID").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Student ID").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Amount").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Date").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Method").SemiBold();
                    });

                    foreach (var item in records)
                    {
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Id.ToString());
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.StudentId.ToString());
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text($"Rs.{item.AmountPaid:0.00}");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.PaymentDate?.ToString("MMM dd, yyyy") ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.PaymentMethod ?? "-");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateStudentAttendancePdf(IReadOnlyList<AttendanceResponseDTO> records)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, "Student Attendance Report"));

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Record ID").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Student ID").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Date").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Status").SemiBold();
                    });

                    foreach (var item in records)
                    {
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Id.ToString());
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.StudentId.ToString());
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Date.ToString("MMM dd, yyyy"));
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Status ?? "-");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateStaffAttendancePdf(IReadOnlyList<StaffAttendanceResponseDTO> records)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, "Staff Attendance Report"));

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(3);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Username").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Type").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Date").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Status").SemiBold();
                    });

                    foreach (var item in records)
                    {
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Username ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.AttendanceType ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Date.ToString("MMM dd, yyyy"));
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Status ?? "-");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateExamResultsPdf(IReadOnlyList<ExamResultResponseDTO> records)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, "Exam Results Report"));

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Result ID").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Exam ID").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Student ID").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Subject ID").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Marks").SemiBold();
                    });

                    foreach (var item in records)
                    {
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Id.ToString());
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.ExamId.ToString());
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.StudentId.ToString());
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.SubjectId.ToString());
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Marks?.ToString("0.00") ?? "-");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateAssetsPdf(IReadOnlyList<AssetResponseDTO> records)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, "Assets Inventory Report"));

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(2);
                        columns.RelativeColumn(3);
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Asset Name").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Status").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Class ID").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Purchase Date").SemiBold();
                    });

                    foreach (var item in records)
                    {
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Assetname ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Status ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.AssignedClassId?.ToString() ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Purchasedate?.ToString("MMM dd, yyyy") ?? "-");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }
    public byte[] GenerateStudentsPdf(IReadOnlyList<SchoolERPManagementBLLibrary.DTOs.Student.StudentQueryResponseDTO> records)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, "Students Directory Report"));

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Reg No
                        columns.RelativeColumn(3); // Name
                        columns.RelativeColumn(2); // Class
                        columns.RelativeColumn(3); // Parent
                        columns.RelativeColumn(2); // Gender
                        columns.RelativeColumn(2); // Status
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Roll No / GR").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Student Name").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Class / Sec").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Parent Name").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Gender").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Status").SemiBold();
                    });

                    foreach (var item in records)
                    {
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.RegNo ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.FirstName + " " + item.LastName ?? "-");
                        var classSec = !string.IsNullOrEmpty(item.ClassName) ? $"{item.ClassName} / {item.Section}" : "-";
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(classSec);
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.ParentName ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Gender ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Status ?? "-");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateTeachersPdf(IReadOnlyList<SchoolERPManagementBLLibrary.DTOs.Teacher.TeacherResponseDTO> records)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, "Teachers Directory Report"));

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Username
                        columns.RelativeColumn(3); // Name
                        columns.RelativeColumn(2); // Phone
                        columns.RelativeColumn(3); // Qualifications
                        columns.RelativeColumn(2); // Joining Date
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Teacher ID").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Teacher Name").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Phone No").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Qualifications").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Joining Date").SemiBold();
                    });

                    foreach (var item in records)
                    {
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Username ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.FirstName + " " + item.LastName ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Phonenumber ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Qualifications ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Joiningdate?.ToString("MMM dd, yyyy") ?? "-");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    public byte[] GenerateParentsPdf(IReadOnlyList<SchoolERPManagementBLLibrary.DTOs.Parent.ParentResponseDTO> records)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, "Parents Directory Report"));

                page.Content().PaddingVertical(1, Unit.Centimetre).Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2); // Username
                        columns.RelativeColumn(3); // Name
                        columns.RelativeColumn(2); // Relation
                        columns.RelativeColumn(3); // Phone
                    });

                    table.Header(header =>
                    {
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Parent ID").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Parent Name").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Relation").SemiBold();
                        header.Cell().BorderBottom(1).PaddingBottom(5).Text("Phone No").SemiBold();
                    });

                    foreach (var item in records)
                    {
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Username ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.FirstName + " " + item.LastName ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Relation ?? "-");
                        table.Cell().PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Text(item.Phonenumber ?? "-");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                    x.Span(" of ");
                    x.TotalPages();
                });
            });
        }).GeneratePdf();
    }
}
