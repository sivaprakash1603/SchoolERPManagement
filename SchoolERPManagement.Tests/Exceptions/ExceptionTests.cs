using FluentAssertions;
using SchoolERPManagementBLLibrary.Exceptions;
using Xunit;

namespace SchoolERPManagement.Tests.Exceptions;

public class ExceptionTests
{
    [Fact]
    public void BusinessRuleException_ShouldSetMessageCorrectly()
    {
        // Arrange
        var message = "A business rule was violated.";

        // Act
        var exception = new BusinessRuleException(message);

        // Assert
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void EntityNotFoundException_WithIdentifier_ShouldFormatMessageCorrectly()
    {
        // Arrange
        var entityName = "Student";
        var identifier = "10";

        // Act
        var exception = new EntityNotFoundException(entityName, identifier);

        // Assert
        exception.Message.Should().Be("Student with identifier '10' was not found.");
    }

    [Fact]
    public void EntityNotFoundException_WithoutIdentifier_ShouldFormatMessageCorrectly()
    {
        // Arrange
        var entityName = "Student";

        // Act
        var exception = new EntityNotFoundException(entityName);

        // Assert
        exception.Message.Should().Be("Student was not found.");
    }

    [Fact]
    public void DuplicateEntityException_ShouldFormatMessageCorrectly()
    {
        // Arrange
        var entityName = "User";
        var fieldName = "Email";
        var value = "test@example.com";

        // Act
        var exception = new DuplicateEntityException(entityName, fieldName, value);

        // Assert
        exception.Message.Should().Be("User with Email 'test@example.com' already exists.");
    }
}
