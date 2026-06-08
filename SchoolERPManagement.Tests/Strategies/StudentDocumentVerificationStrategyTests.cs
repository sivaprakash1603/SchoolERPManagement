using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Document;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Strategies;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Strategies;

public class StudentDocumentVerificationStrategyTests
{
    private readonly Mock<IRepository<int, Studentdocument>> _docRepoMock;
    private readonly Mock<IRepository<int, Studentenrollment>> _enrollmentRepoMock;
    private readonly StudentDocumentVerificationStrategy _strategy;

    public StudentDocumentVerificationStrategyTests()
    {
        _docRepoMock = new Mock<IRepository<int, Studentdocument>>();
        _enrollmentRepoMock = new Mock<IRepository<int, Studentenrollment>>();

        _strategy = new StudentDocumentVerificationStrategy(_docRepoMock.Object, _enrollmentRepoMock.Object);
    }

    [Fact]
    public void CanHandle_ShouldReturnTrueForStudent()
    {
        _strategy.CanHandle("student").Should().BeTrue();
        _strategy.CanHandle("STUDENT").Should().BeTrue();
        _strategy.CanHandle("teacher").Should().BeFalse();
    }

    [Fact]
    public async Task VerifyAsync_AdminRole_ShouldUpdateStatus()
    {
        var dto = new VerifyDocumentDTO("student", 1, "Verified");
        var doc = new Studentdocument { Id = 1, Studentid = 1 };
        _docRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);

        await _strategy.VerifyAsync(dto, 1, "Admin", CancellationToken.None);

        doc.Status.Should().Be("Verified");
        _docRepoMock.Verify(r => r.UpdateAsync(doc, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyAsync_TeacherRoleAuthorized_ShouldUpdateStatus()
    {
        var dto = new VerifyDocumentDTO("student", 1, "Verified");
        var doc = new Studentdocument { Id = 1, Studentid = 1 };
        _docRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);

        var enrollments = new List<Studentenrollment>
        {
            new Studentenrollment { Studentid = 1, Class = new Class { Classteacher = new Teacher { Userid = 1 } } }
        };
        _enrollmentRepoMock.Setup(r => r.Query(true)).Returns(enrollments.AsQueryable().BuildMock());

        await _strategy.VerifyAsync(dto, 1, "Teacher", CancellationToken.None);

        doc.Status.Should().Be("Verified");
    }

    [Fact]
    public async Task VerifyAsync_TeacherRoleUnauthorized_ShouldThrow()
    {
        var dto = new VerifyDocumentDTO("student", 1, "Verified");
        var doc = new Studentdocument { Id = 1, Studentid = 1 };
        _docRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);

        var enrollments = new List<Studentenrollment>
        {
            new Studentenrollment { Studentid = 1, Class = new Class { Classteacher = new Teacher { Userid = 999 } } }
        };
        _enrollmentRepoMock.Setup(r => r.Query(true)).Returns(enrollments.AsQueryable().BuildMock());

        Func<Task> action = async () => await _strategy.VerifyAsync(dto, 1, "Teacher", CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Only the class teacher or an admin can verify this document.");
    }
}
