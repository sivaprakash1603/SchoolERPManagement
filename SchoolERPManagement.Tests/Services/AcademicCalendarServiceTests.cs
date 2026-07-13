using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.AcademicCalendar;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;
using MockQueryable.Moq;

namespace SchoolERPManagement.Tests.Services;

public class AcademicCalendarServiceTests
{
    private readonly Mock<IRepository<int, Academiccalendar>> _calendarRepoMock;
    private readonly Mock<IRepository<int, Academicyear>> _academicYearRepoMock;
    private readonly AcademicCalendarService _service;

    public AcademicCalendarServiceTests()
    {
        _calendarRepoMock = new Mock<IRepository<int, Academiccalendar>>();
        _academicYearRepoMock = new Mock<IRepository<int, Academicyear>>();
        _service = new AcademicCalendarService(
            _calendarRepoMock.Object,
            _academicYearRepoMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task CreateCalendarEventAsync_ValidSingleDay_ShouldCreateEvent()
    {
        var dto = new CreateCalendarEventDTO(new DateOnly(2025, 1, 15), "Holiday", true, 1);
        var academicYear = new Academicyear { Id = 1, Startdate = new DateOnly(2025, 1, 1), Enddate = new DateOnly(2025, 12, 31) };

        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(academicYear);
        _calendarRepoMock.Setup(r => r.Query(true)).Returns(new List<Academiccalendar>().BuildMockDbSet().Object);

        var result = await _service.CreateCalendarEventAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.Date.Should().Be(dto.Date);
        _calendarRepoMock.Verify(r => r.AddAsync(It.Is<Academiccalendar>(x => x.Date == dto.Date), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCalendarEventAsync_ValidRange_ShouldCreateEvents()
    {
        var dto = new CreateCalendarEventDTO(new DateOnly(2025, 1, 15), "Holiday Range", true, 1, new DateOnly(2025, 1, 16));
        var academicYear = new Academicyear { Id = 1, Startdate = new DateOnly(2025, 1, 1), Enddate = new DateOnly(2025, 12, 31) };

        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(academicYear);
        _calendarRepoMock.Setup(r => r.Query(true)).Returns(new List<Academiccalendar>().BuildMockDbSet().Object);

        var result = await _service.CreateCalendarEventAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        _calendarRepoMock.Verify(r => r.AddAsync(It.IsAny<Academiccalendar>(), true, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task CreateCalendarEventAsync_YearNotFound_ShouldThrowEntityNotFoundException()
    {
        var dto = new CreateCalendarEventDTO(new DateOnly(2025, 1, 15), "Holiday", true, 99);
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Academicyear)null);

        Func<Task> act = async () => await _service.CreateCalendarEventAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task CreateCalendarEventAsync_StartDateOutOfBounds_ShouldThrowBusinessRuleException()
    {
        var dto = new CreateCalendarEventDTO(new DateOnly(2024, 12, 31), "Holiday", true, 1);
        var academicYear = new Academicyear { Id = 1, Startdate = new DateOnly(2025, 1, 1), Enddate = new DateOnly(2025, 12, 31) };
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(academicYear);

        Func<Task> act = async () => await _service.CreateCalendarEventAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("The event date must fall within the selected academic year start and end dates.");
    }

    [Fact]
    public async Task CreateCalendarEventAsync_EndDateOutOfBounds_ShouldThrowBusinessRuleException()
    {
        var dto = new CreateCalendarEventDTO(new DateOnly(2025, 1, 15), "Holiday", true, 1, new DateOnly(2026, 1, 1));
        var academicYear = new Academicyear { Id = 1, Startdate = new DateOnly(2025, 1, 1), Enddate = new DateOnly(2025, 12, 31) };
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(academicYear);

        Func<Task> act = async () => await _service.CreateCalendarEventAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("The event end date must fall within the selected academic year start and end dates.");
    }

    [Fact]
    public async Task CreateCalendarEventAsync_EndDateBeforeStartDate_ShouldThrowBusinessRuleException()
    {
        var dto = new CreateCalendarEventDTO(new DateOnly(2025, 1, 15), "Holiday", true, 1, new DateOnly(2025, 1, 10));
        var academicYear = new Academicyear { Id = 1, Startdate = new DateOnly(2025, 1, 1), Enddate = new DateOnly(2025, 12, 31) };
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(academicYear);

        Func<Task> act = async () => await _service.CreateCalendarEventAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("The event end date cannot be earlier than the start date.");
    }

    [Fact]
    public async Task CreateCalendarEventAsync_SingleDayDuplicate_ShouldThrowDuplicateEntityException()
    {
        var dto = new CreateCalendarEventDTO(new DateOnly(2025, 1, 15), "Holiday", true, 1);
        var academicYear = new Academicyear { Id = 1, Startdate = new DateOnly(2025, 1, 1), Enddate = new DateOnly(2025, 12, 31) };
        var existingEvent = new Academiccalendar { Academicyearid = 1, Date = new DateOnly(2025, 1, 15) };

        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(academicYear);
        _calendarRepoMock.Setup(r => r.Query(true)).Returns(new List<Academiccalendar> { existingEvent }.BuildMockDbSet().Object);

        Func<Task> act = async () => await _service.CreateCalendarEventAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task CreateCalendarEventAsync_RangeAllDuplicates_ShouldThrowBusinessRuleException()
    {
        var dto = new CreateCalendarEventDTO(new DateOnly(2025, 1, 15), "Holiday", true, 1, new DateOnly(2025, 1, 16));
        var academicYear = new Academicyear { Id = 1, Startdate = new DateOnly(2025, 1, 1), Enddate = new DateOnly(2025, 12, 31) };
        var existingEvent1 = new Academiccalendar { Academicyearid = 1, Date = new DateOnly(2025, 1, 15) };
        var existingEvent2 = new Academiccalendar { Academicyearid = 1, Date = new DateOnly(2025, 1, 16) };

        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(academicYear);
        _calendarRepoMock.Setup(r => r.Query(true)).Returns(new List<Academiccalendar> { existingEvent1, existingEvent2 }.BuildMockDbSet().Object);

        Func<Task> act = async () => await _service.CreateCalendarEventAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("All dates in the specified range already have registered events/holidays.");
    }

    [Fact]
    public async Task DeleteCalendarEventAsync_Valid_ShouldDelete()
    {
        var calendarEvent = new Academiccalendar { Id = 1 };
        _calendarRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(calendarEvent);

        await _service.DeleteCalendarEventAsync(1, CancellationToken.None);

        _calendarRepoMock.Verify(r => r.DeleteAsync(calendarEvent, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCalendarEventAsync_NotFound_ShouldThrowEntityNotFoundException()
    {
        _calendarRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Academiccalendar)null);

        Func<Task> act = async () => await _service.DeleteCalendarEventAsync(99, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetAcademicCalendarSummaryAsync_Valid_ShouldReturnSummary()
    {
        // Jan 1 to Jan 7 (7 days). Suppose Jan 4 and 5 are Saturday and Sunday.
        var start = new DateOnly(2025, 1, 1); // Wed
        var end = new DateOnly(2025, 1, 7);   // Tue
        var academicYear = new Academicyear { Id = 1, Startdate = start, Enddate = end };

        var events = new List<Academiccalendar>
        {
            new Academiccalendar { Academicyearid = 1, Date = new DateOnly(2025, 1, 1), Isholiday = true }, // Weekday holiday
            new Academiccalendar { Academicyearid = 1, Date = new DateOnly(2025, 1, 4), Isholiday = true }  // Weekend holiday (shouldn't double count)
        };

        _academicYearRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(academicYear);
        _calendarRepoMock.Setup(r => r.Query(true)).Returns(events.BuildMockDbSet().Object);

        var result = await _service.GetAcademicCalendarSummaryAsync(1, CancellationToken.None);

        result.Should().NotBeNull();
        result.TotalDays.Should().Be(7);
        result.WeekendDays.Should().Be(2); // Jan 4, Jan 5
        result.HolidayDays.Should().Be(1); // Jan 1
        result.WorkingDays.Should().Be(4); // 7 - 2 - 1 = 4
    }

    [Fact]
    public async Task GetAcademicCalendarSummaryAsync_NotFound_ShouldThrowEntityNotFoundException()
    {
        _academicYearRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Academicyear)null);

        Func<Task> act = async () => await _service.GetAcademicCalendarSummaryAsync(99, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }
}
