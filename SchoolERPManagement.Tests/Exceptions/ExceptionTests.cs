using FluentAssertions;
using SchoolERPManagementBLLibrary.Exceptions;
using Xunit;

namespace SchoolERPManagement.Tests.Exceptions;

public class ExceptionTests
{
    [Fact]
    public void BusinessRuleException_ShouldSetMessageCorrectly()
    {
        
        var message = "A business rule was violated.";

        
        var exception = new BusinessRuleException(message);

        
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void EntityNotFoundException_WithIdentifier_ShouldFormatMessageCorrectly()
    {
        
        var entityName = "Student";
        var identifier = "10";

        
        var exception = new EntityNotFoundException(entityName, identifier);

        
        exception.Message.Should().Be("Student with identifier '10' was not found.");
    }

    [Fact]
    public void EntityNotFoundException_WithoutIdentifier_ShouldFormatMessageCorrectly()
    {
        
        var entityName = "Student";

        
        var exception = new EntityNotFoundException(entityName);

        
        exception.Message.Should().Be("Student was not found.");
    }

    [Fact]
    public void DuplicateEntityException_ShouldFormatMessageCorrectly()
    {
        
        var entityName = "User";
        var fieldName = "Email";
        var value = "test@example.com";

        
        var exception = new DuplicateEntityException(entityName, fieldName, value);

        
        exception.Message.Should().Be("User with Email 'test@example.com' already exists.");
    }
}
