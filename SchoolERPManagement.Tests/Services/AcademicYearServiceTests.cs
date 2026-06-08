using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.AcademicYear;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class AcademicYearServiceTests
{
    private readonly Mock<IRepository<int, Academicyear>> _academicYearRepoMock;
    private readonly AcademicYearService _academicYearService;

    public AcademicYearServiceTests()
    {
        _academicYearRepoMock = new Mock<IRepository<int, Academicyear>>();
        _academicYearService = new AcademicYearService(_academicYearRepoMock.Object);
    }

    [Fact]
    public async Task CreateAcademicYearAsync_ShouldCreateAndReturnAcademicYear()
    {
        // Arrange
        var dto = new CreateAcademicYearDTO("2025-2026", DateOnly.Parse("2025-06-01"), DateOnly.Parse("2026-05-31"));

        // Act
        var result = await _academicYearService.CreateAcademicYearAsync(dto, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.YearName.Should().Be("2025-2026");
        result.IsCurrent.Should().BeFalse();
        
        _academicYearRepoMock.Verify(r => r.AddAsync(It.IsAny<Academicyear>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAcademicYearsAsync_ShouldReturnListOfAcademicYears()
    {
        // Arrange
        var years = new List<Academicyear>
        {
            new Academicyear { Id = 1, Yearname = "2023-2024", Startdate = DateOnly.Parse("2023-06-01"), Iscurrent = false },
            new Academicyear { Id = 2, Yearname = "2024-2025", Startdate = DateOnly.Parse("2024-06-01"), Iscurrent = true }
        };

        _academicYearRepoMock.Setup(r => r.Query(true)).Returns(years.AsQueryable().BuildMock());

        // Act
        var result = await _academicYearService.GetAllAcademicYearsAsync(CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.First().YearName.Should().Be("2024-2025"); // Ordered descending by Startdate
    }

    [Fact]
    public async Task SetCurrentAcademicYearAsync_ValidId_ShouldUpdateCurrentYear()
    {
        // Arrange
        var oldCurrent = new Academicyear { Id = 1, Yearname = "2023-2024", Iscurrent = true };
        var newCurrent = new Academicyear { Id = 2, Yearname = "2024-2025", Iscurrent = false };

        var yearsList = new List<Academicyear> { oldCurrent, newCurrent };
        
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(newCurrent);
        _academicYearRepoMock.Setup(r => r.Query(false)).Returns(yearsList.AsQueryable().BuildMock());

        // Act
        await _academicYearService.SetCurrentAcademicYearAsync(2, CancellationToken.None);

        // Assert
        oldCurrent.Iscurrent.Should().BeFalse();
        newCurrent.Iscurrent.Should().BeTrue();

        _academicYearRepoMock.Verify(r => r.UpdateAsync(oldCurrent, false, It.IsAny<CancellationToken>()), Times.Once);
        _academicYearRepoMock.Verify(r => r.UpdateAsync(newCurrent, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetCurrentAcademicYearAsync_InvalidId_ShouldThrowEntityNotFoundException()
    {
        // Arrange
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Academicyear?)null);

        // Act
        Func<Task> action = async () => await _academicYearService.SetCurrentAcademicYearAsync(999, CancellationToken.None);

        // Assert
        await action.Should().ThrowAsync<EntityNotFoundException>().WithMessage("AcademicYear with identifier '999' was not found.");
    }
}
