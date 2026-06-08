using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using SchoolERPManagementDALLibrary.Contexts;
using SchoolERPManagementDALLibrary.Repositories;
using SchoolERPManagementModelLibrary.Models;
using Xunit;

namespace SchoolERPManagement.Tests.Repositories;

public class AbstractRepositoryTests
{
    private readonly Mock<SchoolERPDbContext> _contextMock;
    private readonly AbstractRepository<int, Role> _repository;

    public AbstractRepositoryTests()
    {
        _contextMock = new Mock<SchoolERPDbContext>();
        _repository = new AbstractRepository<int, Role>(_contextMock.Object);
    }

    [Fact]
    public async Task AddAsync_ShouldAddEntityToDatabase()
    {
        // Arrange
        var role = new Role { Rolename = "TestRole" };
        var mockSet = new List<Role>().AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Set<Role>()).Returns(mockSet.Object);

        // Act
        await _repository.AddAsync(role, save: true);

        // Assert
        mockSet.Verify(m => m.AddAsync(role, It.IsAny<CancellationToken>()), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateEntityInDatabase()
    {
        // Arrange
        var role = new Role { Rolename = "OldRole" };
        var mockSet = new List<Role> { role }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Set<Role>()).Returns(mockSet.Object);

        // Act
        role.Rolename = "NewRole";
        await _repository.UpdateAsync(role, save: true);

        // Assert
        mockSet.Verify(m => m.Update(role), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveEntityFromDatabase()
    {
        // Arrange
        var role = new Role { Rolename = "ToDelete" };
        var mockSet = new List<Role> { role }.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Set<Role>()).Returns(mockSet.Object);

        // Act
        await _repository.DeleteAsync(role, save: true);

        // Assert
        mockSet.Verify(m => m.Remove(role), Times.Once);
        _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Query_ShouldReturnQueryableOfEntities()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { Rolename = "Role1" },
            new Role { Rolename = "Role2" }
        };
        var mockSet = roles.AsQueryable().BuildMockDbSet();
        _contextMock.Setup(c => c.Set<Role>()).Returns(mockSet.Object);

        // Act
        var query = _repository.Query();

        // Assert
        query.Count().Should().Be(2);
    }
}
