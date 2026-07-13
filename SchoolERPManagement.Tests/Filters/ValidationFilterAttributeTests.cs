using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using SchoolERPManagementAPI.Filters;

namespace SchoolERPManagement.Tests.Filters;

public class ValidationFilterAttributeTests
{
    private readonly ValidationFilterAttribute _filter;
    private readonly Mock<ActionExecutionDelegate> _nextMock;
    private readonly DefaultHttpContext _httpContext;
    private readonly ActionExecutingContext _actionExecutingContext;
    private readonly Mock<IServiceProvider> _serviceProviderMock;

    public ValidationFilterAttributeTests()
    {
        _filter = new ValidationFilterAttribute();
        _nextMock = new Mock<ActionExecutionDelegate>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        
        _httpContext = new DefaultHttpContext
        {
            RequestServices = _serviceProviderMock.Object
        };

        var actionContext = new ActionContext(
            _httpContext,
            new RouteData(),
            new ActionDescriptor { Parameters = new List<ParameterDescriptor>() }
        );

        _actionExecutingContext = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object() // Mocking the controller instance
        );
    }

    public class DummyModel
    {
        public string Name { get; set; } = string.Empty;
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenValidatorPasses_ShouldCallNextAndNotSetResult()
    {
        // Arrange
        var model = new DummyModel { Name = "Valid Name" };
        var paramDescriptor = new ParameterDescriptor { Name = "dummy" };
        
        _actionExecutingContext.ActionDescriptor.Parameters.Add(paramDescriptor);
        _actionExecutingContext.ActionArguments["dummy"] = model;

        var mockValidator = new Mock<IValidator<DummyModel>>();
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // Empty result = valid

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IValidator<DummyModel>)))
            .Returns(mockValidator.Object);

        // Act
        await _filter.OnActionExecutionAsync(_actionExecutingContext, _nextMock.Object);

        // Assert
        _actionExecutingContext.Result.Should().BeNull();
        _nextMock.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenValidatorFails_ShouldSetBadRequestResultAndShortCircuit()
    {
        // Arrange
        var model = new DummyModel { Name = "" };
        var paramDescriptor = new ParameterDescriptor { Name = "dummy" };
        
        _actionExecutingContext.ActionDescriptor.Parameters.Add(paramDescriptor);
        _actionExecutingContext.ActionArguments["dummy"] = model;

        var failures = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required.")
        };

        var mockValidator = new Mock<IValidator<DummyModel>>();
        mockValidator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<object>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failures)); 

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IValidator<DummyModel>)))
            .Returns(mockValidator.Object);

        // Act
        await _filter.OnActionExecutionAsync(_actionExecutingContext, _nextMock.Object);

        // Assert
        _actionExecutingContext.Result.Should().NotBeNull();
        _actionExecutingContext.Result.Should().BeOfType<BadRequestObjectResult>();

        var badRequestResult = _actionExecutingContext.Result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
        
        // Assert short-circuiting
        _nextMock.Verify(n => n(), Times.Never);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenNoValidatorFound_ShouldCallNext()
    {
        // Arrange
        var model = new DummyModel { Name = "Testing" };
        var paramDescriptor = new ParameterDescriptor { Name = "dummy" };
        
        _actionExecutingContext.ActionDescriptor.Parameters.Add(paramDescriptor);
        _actionExecutingContext.ActionArguments["dummy"] = model;

        _serviceProviderMock
            .Setup(sp => sp.GetService(typeof(IValidator<DummyModel>)))
            .Returns(null); // No validator registered

        // Act
        await _filter.OnActionExecutionAsync(_actionExecutingContext, _nextMock.Object);

        // Assert
        _actionExecutingContext.Result.Should().BeNull();
        _nextMock.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenArgumentIsNull_ShouldCallNext()
    {
        // Arrange
        var paramDescriptor = new ParameterDescriptor { Name = "dummy" };
        
        _actionExecutingContext.ActionDescriptor.Parameters.Add(paramDescriptor);
        _actionExecutingContext.ActionArguments["dummy"] = null;

        // Act
        await _filter.OnActionExecutionAsync(_actionExecutingContext, _nextMock.Object);

        // Assert
        _actionExecutingContext.Result.Should().BeNull();
        _nextMock.Verify(n => n(), Times.Once);
    }

    [Fact]
    public async Task OnActionExecutionAsync_WhenArgumentNotInDictionary_ShouldCallNext()
    {
        // Arrange
        var paramDescriptor = new ParameterDescriptor { Name = "dummy" };
        
        _actionExecutingContext.ActionDescriptor.Parameters.Add(paramDescriptor);
        // Do not add to ActionArguments dictionary

        // Act
        await _filter.OnActionExecutionAsync(_actionExecutingContext, _nextMock.Object);

        // Assert
        _actionExecutingContext.Result.Should().BeNull();
        _nextMock.Verify(n => n(), Times.Once);
    }
}
