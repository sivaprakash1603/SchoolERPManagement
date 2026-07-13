using FluentAssertions;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.ParentTeacherMeeting;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Services;

public class ParentTeacherMeetingServiceTests
{
    private readonly Mock<IRepository<int, Parentteachermeeting>> _meetingRepoMock;
    private readonly Mock<IRepository<int, Parentteacherslot>> _slotRepoMock;
    private readonly Mock<IRepository<int, Teacher>> _teacherRepoMock;
    private readonly Mock<IRepository<int, Parent>> _parentRepoMock;
    private readonly ParentTeacherMeetingService _service;

    public ParentTeacherMeetingServiceTests()
    {
        _meetingRepoMock = new Mock<IRepository<int, Parentteachermeeting>>();
        _slotRepoMock = new Mock<IRepository<int, Parentteacherslot>>();
        _teacherRepoMock = new Mock<IRepository<int, Teacher>>();
        _parentRepoMock = new Mock<IRepository<int, Parent>>();

        _service = new ParentTeacherMeetingService(
            _meetingRepoMock.Object,
            _slotRepoMock.Object,
            _teacherRepoMock.Object,
            _parentRepoMock.Object
        );
    }

    [Fact]
    public async Task GetUpcomingMeetingsAsync_ReturnsOnlyActiveFutureMeetings()
    {
        var meetings = new List<Parentteachermeeting>
        {
            new() { Id = 1, Isactive = true, Eventdate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), Starttime = new TimeOnly(9, 0), Endtime = new TimeOnly(10, 0), Description = "PTM A" },
            new() { Id = 2, Isactive = true, Eventdate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)), Starttime = new TimeOnly(9, 0), Endtime = new TimeOnly(10, 0), Description = "Past PTM" },
            new() { Id = 3, Isactive = false, Eventdate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(2)), Starttime = new TimeOnly(9, 0), Endtime = new TimeOnly(10, 0), Description = "Inactive PTM" },
        };

        _meetingRepoMock.Setup(r => r.Query(true)).Returns(meetings.BuildMockDbSet().Object);

        var result = await _service.GetUpcomingMeetingsAsync(CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Id.Should().Be(1);
        result[0].Description.Should().Be("PTM A");
    }

    [Fact]
    public async Task GetUpcomingMeetingsAsync_NoMeetings_ReturnsEmptyList()
    {
        _meetingRepoMock.Setup(r => r.Query(true)).Returns(new List<Parentteachermeeting>().BuildMockDbSet().Object);

        var result = await _service.GetUpcomingMeetingsAsync(CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GenerateSlotsAsync_ValidMeeting_GeneratesSlotsForAllTeachers()
    {
        var meeting = new Parentteachermeeting
        {
            Id = 1,
            Eventdate = DateOnly.FromDateTime(DateTime.UtcNow),
            Starttime = new TimeOnly(9, 0),
            Endtime = new TimeOnly(10, 0),
            Slotdurationminutes = 15
        };

        _meetingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(meeting);
        _slotRepoMock.Setup(r => r.Query(true)).Returns(new List<Parentteacherslot>().BuildMockDbSet().Object);

        var teachers = new List<Teacher>
        {
            new() { Id = 1, Name = "Teacher A", User = new User { Isactive = true } },
            new() { Id = 2, Name = "Teacher B", User = new User { Isactive = true } },
        };
        _teacherRepoMock.Setup(r => r.Query(true)).Returns(teachers.BuildMockDbSet().Object);

        await _service.GenerateSlotsAsync(1, CancellationToken.None);

        _slotRepoMock.Verify(r => r.AddAsync(It.IsAny<Parentteacherslot>(), false, It.IsAny<CancellationToken>()), Times.Exactly(8));
        _slotRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateSlotsAsync_MeetingNotFound_ThrowsEntityNotFoundException()
    {
        _meetingRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Parentteachermeeting?)null);

        Func<Task> act = async () => await _service.GenerateSlotsAsync(999, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage("*ParentTeacherMeeting*999*");
    }

    [Fact]
    public async Task GenerateSlotsAsync_ExistingSlots_SkipsGeneration()
    {
        _meetingRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Parentteachermeeting { Id = 1 });

        var existingSlots = new List<Parentteacherslot>
        {
            new() { Id = 1, Meetingid = 1 }
        };
        _slotRepoMock.Setup(r => r.Query(true)).Returns(existingSlots.BuildMockDbSet().Object);

        await _service.GenerateSlotsAsync(1, CancellationToken.None);

        _slotRepoMock.Verify(r => r.AddAsync(It.IsAny<Parentteacherslot>(), false, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task BookSlotAsync_ValidRequest_BooksSlot()
    {
        var meeting = new Parentteachermeeting
        {
            Id = 1, Isactive = true
        };
        var teacher = new Teacher { Id = 10, Name = "Teacher A" };
        var parent = new Parent { Id = 100, Name = "Parent A" };
        var student = new Student { Id = 200, Name = "Student A" };

        var slot = new Parentteacherslot
        {
            Id = 5,
            Meetingid = 1,
            Teacherid = 10,
            Status = "Available",
            Meeting = meeting,
            Teacher = teacher,
            Parent = null,
            Student = null
        };

        var slots = new List<Parentteacherslot> { slot };
        _slotRepoMock.Setup(r => r.Query(true)).Returns(slots.BuildMockDbSet().Object);

        var parentWithChildren = new Parent
        {
            Id = 100,
            Studentparents = new List<Studentparent>
            {
                new() { Studentid = 200 }
            }
        };
        _parentRepoMock.Setup(r => r.Query(true)).Returns(new List<Parent> { parentWithChildren }.BuildMockDbSet().Object);

        var result = await _service.BookSlotAsync(5, 100, 200, CancellationToken.None);

        result.Should().NotBeNull();
        result.Status.Should().Be("Booked");
        slot.Status.Should().Be("Booked");
        slot.Parentid.Should().Be(100);
        slot.Studentid.Should().Be(200);

        _slotRepoMock.Verify(r => r.UpdateAsync(slot, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task BookSlotAsync_SlotNotFound_ThrowsEntityNotFoundException()
    {
        _slotRepoMock.Setup(r => r.Query(true)).Returns(new List<Parentteacherslot>().BuildMockDbSet().Object);

        Func<Task> act = async () => await _service.BookSlotAsync(999, 100, 200, CancellationToken.None);

        await act.Should().ThrowAsync<EntityNotFoundException>().WithMessage("*ParentTeacherSlot*999*");
    }

    [Fact]
    public async Task BookSlotAsync_InactiveMeeting_ThrowsBusinessRuleException()
    {
        var slot = new Parentteacherslot
        {
            Id = 1,
            Status = "Available",
            Meeting = new Parentteachermeeting { Id = 1, Isactive = false }
        };

        _slotRepoMock.Setup(r => r.Query(true)).Returns(new List<Parentteacherslot> { slot }.BuildMockDbSet().Object);

        Func<Task> act = async () => await _service.BookSlotAsync(1, 100, 200, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*no longer accepting*");
    }

    [Fact]
    public async Task BookSlotAsync_SlotAlreadyBooked_ThrowsBusinessRuleException()
    {
        var slot = new Parentteacherslot
        {
            Id = 1,
            Status = "Booked",
            Meeting = new Parentteachermeeting { Id = 1, Isactive = true }
        };

        _slotRepoMock.Setup(r => r.Query(true)).Returns(new List<Parentteacherslot> { slot }.BuildMockDbSet().Object);

        Func<Task> act = async () => await _service.BookSlotAsync(1, 100, 200, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*already been booked*");
    }

    [Fact]
    public async Task BookSlotAsync_DoubleBooking_ThrowsBusinessRuleException()
    {
        var meeting = new Parentteachermeeting { Id = 1, Isactive = true };
        var slot = new Parentteacherslot
        {
            Id = 1,
            Meetingid = 1,
            Teacherid = 10,
            Status = "Available",
            Meeting = meeting
        };

        var slots = new List<Parentteacherslot>
        {
            slot,
            new() { Id = 2, Meetingid = 1, Teacherid = 10, Parentid = 100, Status = "Booked" }
        };

        _slotRepoMock.Setup(r => r.Query(true)).Returns(slots.BuildMockDbSet().Object);

        Func<Task> act = async () => await _service.BookSlotAsync(1, 100, 200, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*already booked a slot*");
    }

    [Fact]
    public async Task BookSlotAsync_NotOwnChild_ThrowsBusinessRuleException()
    {
        var slot = new Parentteacherslot
        {
            Id = 1,
            Meetingid = 1,
            Teacherid = 10,
            Status = "Available",
            Meeting = new Parentteachermeeting { Id = 1, Isactive = true }
        };

        _slotRepoMock.Setup(r => r.Query(true)).Returns(new List<Parentteacherslot> { slot }.BuildMockDbSet().Object);

        var parentWithoutChild = new Parent
        {
            Id = 100,
            Studentparents = new List<Studentparent>()
        };
        _parentRepoMock.Setup(r => r.Query(true)).Returns(new List<Parent> { parentWithoutChild }.BuildMockDbSet().Object);

        Func<Task> act = async () => await _service.BookSlotAsync(1, 100, 999, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>().WithMessage("*only book slots for your own*");
    }

    [Fact]
    public async Task CancelBookingAsync_ValidRequest_CancelsBooking()
    {
        var slot = new Parentteacherslot
        {
            Id = 1,
            Meetingid = 1,
            Teacherid = 10,
            Status = "Booked",
            Parentid = 100,
            Studentid = 200,
            Bookedat = DateTime.UtcNow
        };

        _slotRepoMock.Setup(r => r.Query(true)).Returns(new List<Parentteacherslot> { slot }.BuildMockDbSet().Object);

        var parent = new Parent { Id = 100, Userid = 50 };
        _parentRepoMock.Setup(r => r.Query(true)).Returns(new List<Parent> { parent }.BuildMockDbSet().Object);

        await _service.CancelBookingAsync(1, 50, CancellationToken.None);

        slot.Status.Should().Be("Available");
        slot.Parentid.Should().BeNull();
        slot.Studentid.Should().BeNull();
        slot.Bookedat.Should().BeNull();
        _slotRepoMock.Verify(r => r.UpdateAsync(slot, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMyBookingsAsync_ReturnsBookedSlots()
    {
        var slots = new List<Parentteacherslot>
        {
            new()
            {
                Id = 1, Parentid = 100, Status = "Booked",
                Teacher = new Teacher { Id = 10, Name = "Teacher A" },
                Starttime = new TimeOnly(9, 0), Endtime = new TimeOnly(9, 30),
                Meetingid = 1, Teacherid = 10
            },
            new()
            {
                Id = 2, Parentid = 100, Status = "Booked",
                Teacher = new Teacher { Id = 11, Name = "Teacher B" },
                Starttime = new TimeOnly(10, 0), Endtime = new TimeOnly(10, 30),
                Meetingid = 1, Teacherid = 11
            },
            new()
            {
                Id = 3, Parentid = 200, Status = "Booked",
                Teacher = new Teacher { Id = 12, Name = "Teacher C" },
                Starttime = new TimeOnly(11, 0), Endtime = new TimeOnly(11, 30),
                Meetingid = 1, Teacherid = 12
            },
        };

        _slotRepoMock.Setup(r => r.Query(true)).Returns(slots.BuildMockDbSet().Object);

        var result = await _service.GetMyBookingsAsync(100, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(b => b.ParentId.Should().Be(100));
    }
}
