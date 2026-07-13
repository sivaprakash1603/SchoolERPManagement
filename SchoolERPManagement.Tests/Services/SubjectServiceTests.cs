using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using SchoolERPManagementBLLibrary.DTOs.Subject;
using SchoolERPManagementBLLibrary.Exceptions;
using SchoolERPManagementBLLibrary.Services;
using SchoolERPManagementDALLibrary.Interfaces;
using SchoolERPManagementModelLibrary.Models;
using Xunit;
using MockQueryable.Moq;

namespace SchoolERPManagement.Tests.Services;

public class SubjectServiceTests
{
    private readonly Mock<IRepository<int, Subject>> _subjectRepoMock;
    private readonly Mock<IRepository<int, Teachersubject>> _teacherSubjectRepoMock;
    private readonly Mock<IRepository<int, Timetable>> _timetableRepoMock;
    private readonly SubjectService _service;

    public SubjectServiceTests()
    {
        _subjectRepoMock = new Mock<IRepository<int, Subject>>();
        _teacherSubjectRepoMock = new Mock<IRepository<int, Teachersubject>>();
        _timetableRepoMock = new Mock<IRepository<int, Timetable>>();

        _service = new SubjectService(
            _subjectRepoMock.Object,
            _teacherSubjectRepoMock.Object,
            _timetableRepoMock.Object,
            SchoolERPManagement.Tests.Helpers.TestHelper.GetMapper()
        );
    }

    [Fact]
    public async Task CreateSubjectAsync_ValidData_ShouldReturnSubject()
    {
        var dto = new CreateSubjectDTO("Math");
        _subjectRepoMock.Setup(r => r.Query(true)).Returns(new List<Subject>().BuildMockDbSet().Object);

        var result = await _service.CreateSubjectAsync(dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.SubjectName.Should().Be("Math");
        _subjectRepoMock.Verify(r => r.AddAsync(It.IsAny<Subject>(), true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSubjectAsync_DuplicateName_ShouldThrowDuplicateEntityException()
    {
        var dto = new CreateSubjectDTO("Math");
        var existing = new Subject { Subjectname = "Math" };
        _subjectRepoMock.Setup(r => r.Query(true)).Returns(new List<Subject> { existing }.BuildMockDbSet().Object);

        Func<Task> act = async () => await _service.CreateSubjectAsync(dto, CancellationToken.None);
        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task GetAllSubjectsAsync_ShouldReturnAllSubjects()
    {
        var subjects = new List<Subject>
        {
            new Subject { Id = 1, Subjectname = "Math" },
            new Subject { Id = 2, Subjectname = "Science" }
        };
        _subjectRepoMock.Setup(r => r.Query(true)).Returns(subjects.BuildMockDbSet().Object);

        var result = await _service.GetAllSubjectsAsync(CancellationToken.None);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetSubjectByIdAsync_Valid_ShouldReturnSubject()
    {
        var subject = new Subject { Id = 1, Subjectname = "Math" };
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subject);

        var result = await _service.GetSubjectByIdAsync(1, CancellationToken.None);

        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    [Fact]
    public async Task GetSubjectByIdAsync_NotFound_ShouldThrowEntityNotFoundException()
    {
        _subjectRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Subject)null);

        Func<Task> act = async () => await _service.GetSubjectByIdAsync(99, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task UpdateSubjectAsync_Valid_ShouldReturnSubject()
    {
        var subject = new Subject { Id = 1, Subjectname = "Math" };
        var dto = new CreateSubjectDTO("Mathematics");

        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subject);
        _subjectRepoMock.Setup(r => r.Query(true)).Returns(new List<Subject>().BuildMockDbSet().Object);

        var result = await _service.UpdateSubjectAsync(1, dto, CancellationToken.None);

        result.Should().NotBeNull();
        result.SubjectName.Should().Be("Mathematics");
        _subjectRepoMock.Verify(r => r.UpdateAsync(subject, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSubjectAsync_DuplicateName_ShouldThrowDuplicateEntityException()
    {
        var subject = new Subject { Id = 1, Subjectname = "Math" };
        var dto = new CreateSubjectDTO("Science");
        var existing = new Subject { Id = 2, Subjectname = "Science" };

        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subject);
        _subjectRepoMock.Setup(r => r.Query(true)).Returns(new List<Subject> { existing }.BuildMockDbSet().Object);

        Func<Task> act = async () => await _service.UpdateSubjectAsync(1, dto, CancellationToken.None);
        await act.Should().ThrowAsync<DuplicateEntityException>();
    }

    [Fact]
    public async Task UpdateSubjectAsync_NotFound_ShouldThrowEntityNotFoundException()
    {
        var dto = new CreateSubjectDTO("Mathematics");
        _subjectRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Subject)null);

        Func<Task> act = async () => await _service.UpdateSubjectAsync(99, dto, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task DeleteSubjectAsync_Valid_ShouldDelete()
    {
        var subject = new Subject { Id = 1 };
        _subjectRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(subject);

        await _service.DeleteSubjectAsync(1, CancellationToken.None);

        _subjectRepoMock.Verify(r => r.DeleteAsync(subject, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteSubjectAsync_NotFound_ShouldThrowEntityNotFoundException()
    {
        _subjectRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Subject)null);

        Func<Task> act = async () => await _service.DeleteSubjectAsync(99, CancellationToken.None);
        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    [Fact]
    public async Task GetSubjectsByClassAsync_ShouldReturnSubjects()
    {
        var teacherSubjects = new List<Teachersubject>
        {
            new Teachersubject { Classid = 1, Subjectid = 1 }
        };
        var timetables = new List<Timetable>
        {
            new Timetable { Classid = 1, Subjectid = 2 }
        };
        var subjects = new List<Subject>
        {
            new Subject { Id = 1, Subjectname = "Math" },
            new Subject { Id = 2, Subjectname = "Science" }
        };

        _teacherSubjectRepoMock.Setup(r => r.Query(true)).Returns(teacherSubjects.BuildMockDbSet().Object);
        _timetableRepoMock.Setup(r => r.Query(true)).Returns(timetables.BuildMockDbSet().Object);
        _subjectRepoMock.Setup(r => r.Query(true)).Returns(subjects.BuildMockDbSet().Object);

        var result = await _service.GetSubjectsByClassAsync(1, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(s => s.Id == 1);
        result.Should().Contain(s => s.Id == 2);
    }
}
