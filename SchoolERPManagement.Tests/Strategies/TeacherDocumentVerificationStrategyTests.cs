using FluentAssertions;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Document;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Strategies;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Strategies;

public class TeacherDocumentVerificationStrategyTests
{
    private readonly Mock<IRepository<int, Teacherdocument>> _docRepoMock;
    private readonly TeacherDocumentVerificationStrategy _strategy;

    public TeacherDocumentVerificationStrategyTests()
    {
        _docRepoMock = new Mock<IRepository<int, Teacherdocument>>();
        _strategy = new TeacherDocumentVerificationStrategy(_docRepoMock.Object);
    }

    [Fact]
    public void CanHandle_ShouldReturnTrueForTeacher()
    {
        _strategy.CanHandle("teacher").Should().BeTrue();
        _strategy.CanHandle("TEACHER").Should().BeTrue();
        _strategy.CanHandle("student").Should().BeFalse();
    }

    [Fact]
    public async Task VerifyAsync_AdminRole_ShouldUpdateStatus()
    {
        var dto = new VerifyDocumentDTO("teacher", 1, "Verified");
        var doc = new Teacherdocument { Id = 1, Teacherid = 1 };
        _docRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(doc);

        await _strategy.VerifyAsync(dto, 1, "Admin", CancellationToken.None);

        doc.Status.Should().Be("Verified");
        _docRepoMock.Verify(r => r.UpdateAsync(doc, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task VerifyAsync_TeacherRole_ShouldThrow()
    {
        var dto = new VerifyDocumentDTO("teacher", 1, "Verified");

        Func<Task> action = async () => await _strategy.VerifyAsync(dto, 1, "Teacher", CancellationToken.None);

        await action.Should().ThrowAsync<BusinessRuleException>().WithMessage("Only admins can verify teacher documents.");
    }
}
