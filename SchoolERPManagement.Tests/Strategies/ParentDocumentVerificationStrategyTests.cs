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

public class ParentDocumentVerificationStrategyTests
{
    private readonly Mock<IRepository<int, Parentdocument>> _docRepoMock;
    private readonly Mock<IRepository<int, Studentenrollment>> _enrollmentRepoMock;
    private readonly ParentDocumentVerificationStrategy _strategy;

    public ParentDocumentVerificationStrategyTests()
    {
        _docRepoMock = new Mock<IRepository<int, Parentdocument>>();
        _enrollmentRepoMock = new Mock<IRepository<int, Studentenrollment>>();

        _strategy = new ParentDocumentVerificationStrategy(_docRepoMock.Object, _enrollmentRepoMock.Object);
    }

    [Fact]
    public void CanHandle_ShouldReturnTrueForParent()
    {
        _strategy.CanHandle("parent").Should().BeTrue();
        _strategy.CanHandle("PARENT").Should().BeTrue();
        _strategy.CanHandle("student").Should().BeFalse();
    }

    [Fact]
    public async Task VerifyAsync_AdminRole_ShouldUpdateStatus()
    {
        var dto = new VerifyDocumentDTO("parent", 1, "Verified");
        var doc = new Parentdocument { Id = 1, Parentid = 1 };
        _docRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);

        await _strategy.VerifyAsync(dto, 1, "Admin", CancellationToken.None);

        doc.Status.Should().Be("Verified");
        _docRepoMock.Verify(r => r.UpdateAsync(doc, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyAsync_TeacherRoleAuthorized_ShouldUpdateStatus()
    {
        var dto = new VerifyDocumentDTO("parent", 1, "Verified");
        var doc = new Parentdocument { Id = 1, Parentid = 1 };
        _docRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);

        var enrollments = new List<Studentenrollment>
        {
            new Studentenrollment { Student = new Student { Parentid = 1 }, Class = new Class { Classteacher = new Teacher { Userid = 1 } } }
        };
        _enrollmentRepoMock.Setup(r => r.Query(true)).Returns(enrollments.BuildMockDbSet().Object);

        await _strategy.VerifyAsync(dto, 1, "Teacher", CancellationToken.None);

        doc.Status.Should().Be("Verified");
    }

    [Fact]
    public async Task VerifyAsync_TeacherRoleUnauthorized_ShouldThrow()
    {
        var dto = new VerifyDocumentDTO("parent", 1, "Verified");
        var doc = new Parentdocument { Id = 1, Parentid = 1 };
        _docRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);

        var enrollments = new List<Studentenrollment>
        {
            new Studentenrollment { Student = new Student { Parentid = 1 }, Class = new Class { Classteacher = new Teacher { Userid = 999 } } }
        };
        _enrollmentRepoMock.Setup(r => r.Query(true)).Returns(enrollments.BuildMockDbSet().Object);

        Func<Task> action = async () => await _strategy.VerifyAsync(dto, 1, "Teacher", CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Only the class teacher of a parent's child or an admin can verify this document.");
    }
}
